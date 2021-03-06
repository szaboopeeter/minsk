﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;
using Minsk.IO;

namespace Minsk
{
    internal sealed class MinskRepl : Repl
    {
        private Compilation _previous;
        private bool _showTree;
        private bool _showProgram;
        private bool _loadingSubmissions = false;
        private static readonly Compilation emptyCompilation = Compilation.CreateScript(null);
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();

        public MinskRepl()
        {
            LoadSubmissions();
        }

        [MetaCommand("showProgram", "Shows the bound tree")]
        private void EvaluateShowProgram()
        {
            _showProgram = !_showProgram;
            Console.WriteLine(_showProgram ? "Showing bound trees." : "Not showing bound trees.");
        }

        [MetaCommand("showTree", "Shows the parse tree")]
        private void EvaluateShowTree()
        {
            _showTree = !_showTree;
            Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
        }

        [MetaCommand("cls", "Clears the screen")]
        private void EvaluateCls()
        {
            Console.Clear();
        }

        [MetaCommand("reset", "Clears all previous submissions")]
        private void EvaluateReset()
        {
            _previous = null;
            _variables.Clear();
            ClearSubmissions();
        }

        [MetaCommand("ls", "Lists all symbols")]
        private void EvaluateLs()
        {
            var compilation = _previous ?? emptyCompilation;
            var symbols = compilation.GetSymbols().OrderBy(s => s.Kind).ThenBy(s => s.Name);

            foreach (var symbol in symbols)
            {
                symbol.WriteTo(Console.Out);
                Console.WriteLine();
            }
        }

        [MetaCommand("load", "Loads a script file")]
        private void EvaluateLoad(string path)
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: file does not exist: {path}");
                Console.ResetColor();
                return;
            }

            var text = File.ReadAllText(path);
            EvaluateSubmission(text);
        }

        [MetaCommand("dump", "Shows bound tree of a given function")]
        private void EvaluateDump(string functionName)
        {
            var compilation = _previous ?? emptyCompilation;
            var symbol = compilation.GetSymbols().OfType<FunctionSymbol>().SingleOrDefault(f => f.Name == functionName);

            if (symbol == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: function {functionName} does not exist");
                Console.ResetColor();
                return;
            }

            compilation.EmitTree(symbol, Console.Out);
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            var lastTwoLinesAreBlank = text
                    .Split(Environment.NewLine)
                    .Reverse()
                    .TakeWhile(s => string.IsNullOrEmpty(s))
                    .Take(2)
                    .Count() == 2;

            if (lastTwoLinesAreBlank)
            {
                return true;
            }

            var syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
            {
                return false;
            }

            return true;
        }

        protected override void EvaluateSubmission(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);

            var compilation = Compilation.CreateScript(_previous, syntaxTree);

            if (_showTree)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;

                syntaxTree.Root.WriteTo(Console.Out);

                Console.ResetColor();
            }

            if (_showProgram)
            {
                compilation.EmitTree(Console.Out);
            }

            var result = compilation.Evaluate(_variables);

            if (!result.Diagnostics.Any())
            {
                if (result.Value != null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }

                _previous = compilation;

                SaveSubmission(text);
            }
            else
            {
                Console.Out.WriteDiagnostics(result.Diagnostics);
            }
        }

        private void SaveSubmission(string text)
        {
            if (_loadingSubmissions)
            {
                return;
            }

            var submissionsFolder = GetSubmissionsFolder();
            Directory.CreateDirectory(submissionsFolder);
            var count = Directory.GetFiles(submissionsFolder).Length;
            var name = $"submission{count:0000}";
            var fileName = Path.Combine(submissionsFolder, name);
            File.WriteAllText(fileName, text);
        }

        private static void ClearSubmissions()
        {
            string dir = GetSubmissionsFolder();
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        private void LoadSubmissions()
        {
            var submissionsFolder = GetSubmissionsFolder();
            if (!Directory.Exists(submissionsFolder))
            {
                return;
            }

            var files = Directory.GetFiles(submissionsFolder).OrderBy(f => f).ToArray();
            if (files.Length == 0)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Loaded {files.Length} submissions.");
            Console.ResetColor();

            _loadingSubmissions = true;

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                EvaluateSubmission(text);
            }

            _loadingSubmissions = false;
        }

        private static string GetSubmissionsFolder()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var submissionsFolder = Path.Combine(localAppData, "Minsk", "Submissions");
            return submissionsFolder;
        }

        protected override void RenderLine(string line)
        {
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                var isKeyword = token.Kind.ToString().EndsWith("Keyword");
                var isIdentifier = token.Kind == SyntaxKind.IdentifierToken;
                var isNumber = token.Kind == SyntaxKind.NumberToken;
                var isString = token.Kind == SyntaxKind.StringToken;

                if (isKeyword)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else if (isIdentifier)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else if (isNumber)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (isString)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                Console.Write(token.Text);

                Console.ResetColor();
            }
        }
    }
}
