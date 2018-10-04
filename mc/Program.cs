﻿using System;

namespace mc
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("> ");
            while (true)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var lexer = new Lexer(line);
                while (true)
                {
                    var token = lexer.NextToken();
                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        break;
                    }
                    Console.Write($"{token.Kind}: '{token.Text}'");
                    if (token.Value != null)
                    {
                        Console.Write($" {token.Value}");
                    }
                    Console.WriteLine();
                }

                //if (line == "1 + 2 * 3")
                //{
                //    Console.WriteLine("7");
                //}
                //else
                //{
                //    Console.WriteLine("Error: invalid expression");
                //}
            }
        }
    }

    enum SyntaxKind
    {
        NumberToken,
        WhiteSpaceToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        BadPositionToken,
        EndOfFileToken
    }

    class SyntaxToken
    {
        public SyntaxKind Kind { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            Kind = kind;
            Position = position;
            Text = text;
            Value = value;
        }
    }

    class Lexer
    {
        private readonly string _text;
        private int _position;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                {
                    return '\0';
                }

                return _text[_position];
            }
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {
            // <numbers>
            // = - * / ( )
            // <whitespace>

            if (_position >= _text.Length)
            {
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            }

            if (char.IsDigit(Current))
            {
                var start = _position;
                while (char.IsDigit(Current))
                {
                    Next();
                }

                var lenght = _position - start;
                var text = _text.Substring(start, lenght);

                int.TryParse(text, out var value);

                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;
                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                var lenght = _position - start;
                var text = _text.Substring(start, lenght);

                return new SyntaxToken(SyntaxKind.WhiteSpaceToken, start, text, null);
            }

            if (Current == '+')
            {
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            }

            if (Current == '-')
            {
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            }
            if (Current == '*')
            {
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "-", null);
            }
            if (Current == '/')
            {
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "-", null);
            }
            if (Current == '(')
            {
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "-", null);
            }
            if (Current == ')')
            {
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, "-", null);
            }

            return new SyntaxToken(SyntaxKind.BadPositionToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }
}
