namespace Minsk.CodeAnalysis.Binding
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
        ConversionExpression,

        // Statements
        BlockStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        GotoStatement,
        ConditionalGotoStatement,
        LabelStatement,
        ExpressionStatement,
    }
}
