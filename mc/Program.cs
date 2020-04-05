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

            var paths = GetFilePaths(args);
            var syntaxTrees = new List<SyntaxTree>(paths.Count());
            var hasErrors = false;

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    System.Console.WriteLine($"error: file '{path}' does not exist");
                    hasErrors = true;
                    continue;
                }
                var syntaxTree = SyntaxTree.Load(path);
                syntaxTrees.Add(syntaxTree);
            }

            if (hasErrors)
            {
                return;
            }

            var compilation = new Compilation(syntaxTrees.ToArray());
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (result.Diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(result.Diagnostics);
            }
            else
            {
                if (result.Value != null)
                {
                    Console.WriteLine(result.Value);
                }
            }
        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
        {
            var result = new SortedSet<string>();

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    result.UnionWith(Directory.EnumerateFiles(path, "*.ms", SearchOption.AllDirectories));
                }
                else
                {
                    result.Add(path);
                }
            }

            return result;
        }
    }
}
