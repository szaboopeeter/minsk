namespace Minsk.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ExpressionSyntax exression, SyntaxToken endOfFileToken) => Exression = exression;

        public ExpressionSyntax Exression { get; }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
    }
}
