﻿namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        ErrorExpression,
        CallExpression,

        // Statements
        BlockStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        GotoStatement,
        ConditionalGotoStatement,
        LabelStatement,
        ExpressionStatement,
    }
}
