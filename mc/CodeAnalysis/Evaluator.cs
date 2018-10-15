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

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            if (node is BoundLiteralExpression n)
            {
                return n.Value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = (int)EvaluateExpression(u.Operand);

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
                var left = (int)EvaluateExpression(b.Left);
                var right = (int)EvaluateExpression(b.Right);
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
