﻿using System;
using System.Collections.Generic;
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
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();

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

            var compilation = _previous == null ? new Compilation(syntaxTree) : _previous.ContinueWith(syntaxTree);

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
            }
            else
            {
                Console.Out.WriteDiagnostics(result.Diagnostics);
            }
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
