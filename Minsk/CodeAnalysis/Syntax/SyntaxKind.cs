namespace Minsk.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens
        BadToken,
        EndOfFileToken,
        WhiteSpaceToken,
        NumberToken,
        StringToken,
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        BangToken,
        EqualsToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        OpenBraceToken,
        CloseBraceToken,
        IdentifierToken,
        TildeToken,
        PipeToken,
        PipePipeToken,
        AmpersandToken,
        AmpersandAmpersandToken,
        HatToken,
        EqualsEqualsToken,
        LessToken,
        LessOrEqualsToken,
        GreaterToken,
        GraterOrEqualsToken,
        BangEqualsToken,
        CommaToken,

        // Keywords
        ElseKeyword,
        FalseKeyword,
        ForKeyword,
        IfKeyword,
        LetKeyword,
        ToKeyword,
        TrueKeyword,
        VarKeyword,
        WhileKeyword,
        DoKeyword,

        // Nodes
        CompilationUnit,
        ElseClause,

        // Statements
        BlockStatement,
        ExpressionStatement,
        ForStatement,
        IfStatement,
        VariableDeclaration,
        WhileStatement,
        DoWhileStatement,

        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
        CallExpression,
    }
}
