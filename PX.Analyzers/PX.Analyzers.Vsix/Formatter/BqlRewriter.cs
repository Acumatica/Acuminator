using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlRewriter : BqlRewriterBase
	{
		public BqlRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTrivia endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, SyntaxTriviaList.Empty)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			node = (GenericNameSyntax)base.VisitGenericName(node);

			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol constructedFromSymbol = typeSymbol?.ConstructedFrom; // get generic type

			if (constructedFromSymbol != null)
			{
				// Check interfaces - it is hard to check inheritance from base generic types because of generic parameters
				// E.g. Search<Field, Where> doesn't inherit from SearchBase directly, we need to check ConstructedFrom
				if (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlSelect)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlSearch)) // TODO: could be Coalesce - handle this case in the future
				{
					SyntaxTriviaList defaultTrivia = GetDefaultLeadingTrivia(node);
					var selectRewriter = new BqlSelectRewriter(this, defaultTrivia);
					node = (GenericNameSyntax) selectRewriter.Visit(node);
				}
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

		private SyntaxTriviaList GetDefaultLeadingTrivia(SyntaxNode node)
		{
			if (node == null) return SyntaxTriviaList.Empty;
			if (node.HasLeadingTrivia 
				|| node.IsKind(SyntaxKind.PropertyDeclaration) // View
				|| node.IsKind(SyntaxKind.TypeOfExpression) // BQL in attribute
				|| node.IsKind(SyntaxKind.SimpleMemberAccessExpression)) // Static call
			{
				return node.GetLeadingTrivia();
			}

			return GetDefaultLeadingTrivia(node.Parent);
		}
	}
}
