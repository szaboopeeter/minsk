using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace Minsk.CodeAnalysis
{
    public class Compilation
    {
        private BoundGlobalScope _globalScope;
        public Compilation(params SyntaxTree[] syntaxTrees) : this(null, syntaxTrees)
        {
        }

        private Compilation(Compilation previous, params SyntaxTree[] syntaxTrees)
        {
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public Compilation Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var sumbission = this;
            var seenSymbolNames = new HashSet<string>();

            while (sumbission != null)
            {
                const ReflectionBindingFlags bindingFlags =
                    ReflectionBindingFlags.Static |
                    ReflectionBindingFlags.Public |
                    ReflectionBindingFlags.NonPublic;
                var builtinFunctions = typeof(BuiltinFunctions)
                    .GetFields(bindingFlags)
                    .Where(fi => fi.FieldType == typeof(FunctionSymbol))
                    .Select(fi => (FunctionSymbol)fi.GetValue(obj: null))
                    .ToList();
                foreach (var builtin in builtinFunctions)
                    if (seenSymbolNames.Add(builtin.Name))
                        yield return builtin;

                foreach (var function in sumbission.Functions)
                {
                    if (seenSymbolNames.Add(function.Name))
                    {
                        yield return function;
                    }
                }

                foreach (var variable in sumbission.Variables)
                {
                    if (seenSymbolNames.Add(variable.Name))
                    {
                        yield return variable;
                    }
                }

                sumbission = sumbission.Previous;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseDigagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
            var diagnostics = parseDigagnostics.Concat(GlobalScope.Diagnostics).ToArray();
            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics.ToImmutableArray(), null);
            }

            var program = Binder.BindProgram(GlobalScope);

            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDirectory, "cfg.dot");
            var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any() ? program.Functions.Last().Value : program.Statement;
            var cfg = ControlFlowGraph.Create(cfgStatement);
            using (var streamWriter = new StreamWriter(cfgPath))
            {
                cfg.WriteTo(streamWriter);
            }

            if (program.Diagnostics.Any())
            {
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);
            }

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);

            symbol.WriteTo(writer);
            writer.WriteLine();

            if (!program.Functions.TryGetValue(symbol, out var body))
            {
                return;
            }

            body.WriteTo(writer);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);

            if (program.Statement.Statements.Any())
            {
                program.Statement.WriteTo(writer);
            }
            else
            {
                foreach (var functionBody in program.Functions)
                {
                    if (!GlobalScope.Functions.Contains(functionBody.Key))
                    {
                        continue;
                    }

                    functionBody.Key.WriteTo(writer);
                    writer.WriteLine();
                    functionBody.Value.WriteTo(writer);
                }
            }
        }
    }
}
