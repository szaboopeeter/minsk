using System;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression(BoundExpression left, BoundBiaryOperatorKind operatorKind, BoundExpression right)
        {
            Left = left;
            OperatorKind = operatorKind;
            Right = right;
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;

        public BoundExpression Left { get; }
        public BoundBiaryOperatorKind OperatorKind { get; }
        public BoundExpression Right { get; }
        public override Type Type => Left.Type;
    }
}
