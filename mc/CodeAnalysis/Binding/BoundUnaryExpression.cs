﻿using System;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public BoundUnaryOperatorKind OperatorKind { get; }
        public BoundExpression Operand { get; }
        public override Type Type => Operand.Type;
    }
}
