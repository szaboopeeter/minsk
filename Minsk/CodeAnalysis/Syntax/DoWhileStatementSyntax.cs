namespace Minsk.CodeAnalysis.Syntax
{
    public sealed class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax(SyntaxTree syntaxTree,
                                      SyntaxToken doKeyword,
                                      StatementSyntax body,
                                      SyntaxToken whileToken,
                                      ExpressionSyntax condition)
                                      : base(syntaxTree)
        {
            DoKeyword = doKeyword;
            Body = body;
            WhileToken = whileToken;
            Condition = condition;
        }

        public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
        public SyntaxToken DoKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileToken { get; }
        public ExpressionSyntax Condition { get; }
    }
}
