﻿using System;

namespace Minsk.CodeAnalysis
{
    public class Evaluator
    {
        private readonly ExpressionSyntax _root;
        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax node)
        {
            // Binary expression
            // Number expression

            if (node is LiteralExpressionSyntax n)
            {
                return (int)n.LiteralToken.Value;
            }

            if (node is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                {
                    return left + right;
                }
                else if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                {
                    return left - right;
                }
                else if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                {
                    return left * right;
                }
                else if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                {
                    return left / right;
                }
                else
                {
                    throw new Exception($"Unexpected binary opertator {b.OperatorToken.Kind}");
                }
            }

            if (node is ParethesizedExpressionSyntax p)
            {
                return EvaluateExpression(p.Expression);
            }

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
