using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlRewriter : BqlRewriterBase
	{
		public BqlRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTrivia endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			node = (GenericNameSyntax)base.VisitGenericName(node);

			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			if (typeSymbol != null
				&& (typeSymbol.InheritsFromOrEquals(Context.SelectBase)
					|| typeSymbol.InheritsFromOrEquals(Context.SearchBase)
					|| typeSymbol.InheritsFromOrEquals(Context.PXSelectBase)))
			{
				var selectRewriter = new BqlSelectRewriter(this);
				node = (GenericNameSyntax)selectRewriter.Visit(node);
			}

			return node;
		}

		public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
		{
			return base.VisitIdentifierName(node);
		}

		public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
		{
			return base.VisitQualifiedName(node);
		}
	}
}
