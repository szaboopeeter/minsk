using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Minsk.IO
{
    internal static class TextWriterExtensions
    {
        public static bool IsConsoleOut(this TextWriter writer)
        {
            if (writer == Console.Out)
            {
                return true;
            }

            if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut())
            {
                return true;
            }

            return false;
        }

        public static void SetForegoround(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut())
            {
                Console.ForegroundColor = color;
            }
        }

        public static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsoleOut())
            {
                Console.ResetColor();
            }
        }

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForegoround(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForegoround(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForegoround(ConsoleColor.Cyan);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetForegoround(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColor();
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForegoround(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColor();
        }
    }
}