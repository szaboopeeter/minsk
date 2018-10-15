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
                var operand = EvaluateExpression(u.Operand);

                switch (u.OperatorKind)
                {
                    case BoundUnaryOperatorKind.Identity:
                        return (int)operand;
                    case BoundUnaryOperatorKind.Negation:
                        return -(int)operand;
                    case BoundUnaryOperatorKind.LogicalNegation:
                        return !(bool)operand;
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
                        return (int)left + (int)right;
                    case BoundBiaryOperatorKind.Subtraction:
                        return (int)left - (int)right;
                    case BoundBiaryOperatorKind.Multiplication:
                        return (int)left * (int)right;
                    case BoundBiaryOperatorKind.Division:
                        return (int)left / (int)right;
                    case BoundBiaryOperatorKind.LogicalAnd:
                        return (bool)left && (bool)right;
                    case BoundBiaryOperatorKind.LogicalOr:
                        return (bool)left || (bool)right;
                    default:
                        throw new Exception($"Unexpected binary opertator {b.OperatorKind}");
                }
            }

            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
