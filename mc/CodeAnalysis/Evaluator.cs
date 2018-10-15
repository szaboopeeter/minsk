using System;
using Minsk.CodeAnalysis.Binding;

namespace Minsk.CodeAnalysis
{
    internal class Evaluator
    {
        private readonly BoundExpression _root;

        public Evaluator(BoundExpression root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(BoundExpression node)
        {
            // Binary expression
            // Number expression

            if (node is BoundLiteralExpression n)
            {
                return (int)n.Value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                switch (u.OperatorKind)
                {
                    case BoundUnaryOperatorKind.Identity:
                        return operand;
                    case BoundUnaryOperatorKind.Negation:
                        return -operand;
                    default:
                        throw new Exception($"Unexpected unary operator {u.OperatorKind}");
                }
            }

            if (node is BoundBinaryExpression b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);
                switch (b.OperatorKind)
                {
                    case BoundBiaryOperatorKind.Addition:
                        return left + right;
                    case BoundBiaryOperatorKind.Subtraction:
                        return left - right;
                    case BoundBiaryOperatorKind.Multiplication:
                        return left * right;
                    case BoundBiaryOperatorKind.Division:
                        return left / right;
                    default:
                        throw new Exception($"Unexpected binary opertator {b.OperatorKind}");
                }
            }

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
