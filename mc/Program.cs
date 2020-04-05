using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;
using Minsk.IO;

namespace Minsk
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage is mc <source-paths>");
                return;
            }

            if (args.Length > 1)
            {
                System.Console.WriteLine("Error: only one path supported right now.");
            }

            var path = args.Single();

            if (!File.Exists(path))
            {
                System.Console.WriteLine($"error: file '{path}' does not exist");
            }
            var syntaxTree = SyntaxTree.Load(path);

            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (result.Diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
            }
            else
            {
                if (result.Value != null)
                {
                    Console.WriteLine(result.Value);
                }
            }
        }
    }
}
