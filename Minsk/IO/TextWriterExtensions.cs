using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Syntax;
using Minsk.CodeAnalysis.Text;

namespace Minsk.IO
{
    public static class TextWriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out)
            {
                return !Console.IsOutputRedirected;
            }

            if (writer == Console.Error)
            {
                return !Console.IsErrorRedirected && !Console.IsOutputRedirected;
            }

            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole())
            {
                return true;
            }

            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
            {
                Console.ForegroundColor = color;
            }
        }

        private static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
            {
                Console.ResetColor();
            }
        }

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Cyan);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
        {
            writer.WriteKeyword(SyntaxFacts.GetText(kind));
        }

        public static void WriteSpace(this TextWriter writer)
        {
            writer.WritePunctuation(" ");
        }

        public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
        {
            writer.WritePunctuation(SyntaxFacts.GetText(kind));
        }

        public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics.OrderBy(diag => diag.Location.Text.FileName)
                                        .ThenBy(diag => diag.Location.Span.Start)
                                        .ThenBy(diag => diag.Location.Span.Length))
            {
                var text = diagnostic.Location.Text;
                var fileName = diagnostic.Location.FileName;
                var startLine = diagnostic.Location.StartLine + 1;
                var startCharacter = diagnostic.Location.StartCharacter + 1;
                var endLine = diagnostic.Location.EndLine + 1;
                var endCharacter = diagnostic.Location.EndCharacter + 1;
                var span = diagnostic.Location.Span;
                var lineIndex = text.GetLineIndex(span.Start);
                var line = text.Lines[lineIndex];
                var lineNumber = lineIndex + 1;
                var character = span.Start - line.Start + 1;

                writer.WriteLine();

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
                writer.WriteLine(diagnostic);
                writer.ResetColor();

                var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
                var suffixSpan = TextSpan.FromBounds(span.End, line.End);

                var prefix = text.ToString(prefixSpan);
                var error = text.ToString(span);
                var suffix = text.ToString(suffixSpan);

                writer.Write("    ");
                writer.Write(prefix);

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write(error);
                writer.ResetColor();

                writer.Write(suffix);
                writer.WriteLine();

            }

            writer.WriteLine();
        }
    }
}
