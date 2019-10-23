namespace Minsk.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        AssignmentExpression,
        BinaryExpression,
        CallExpression,
        ConversionExpression,
        ErrorExpression,
        LiteralExpression,
        UnaryExpression,
        VariableExpression,

        // Statements
        BlockStatement,
        ConditionalGotoStatement,
        DoWhileStatement,
        ExpressionStatement,
        ForStatement,
        GotoStatement,
        IfStatement,
        LabelStatement,
        ReturnStatement,
        VariableDeclaration,
        WhileStatement,
    }
}
