using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	/// <summary>
	/// Collects all modifications without modifying original syntax tree
	/// </summary>
	class BqlRewriter : BqlRewriterBase
	{
		public BqlRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, SyntaxTriviaList.Empty)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol constructedFromSymbol = typeSymbol?.ConstructedFrom; // get generic type
			
			if (constructedFromSymbol != null 
				&& (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase) 
					|| constructedFromSymbol.ImplementsInterface(Context.IBqlCreator)))
			{
				var statementRewriter = new BqlStatementRewriter(this, GetDefaultLeadingTrivia(node));
				return statementRewriter.Visit(node);
			}
			
			return base.VisitGenericName(node);
		}

		private SyntaxTriviaList GetDefaultLeadingTrivia(SyntaxNode node)
		{
			if (node == null) return SyntaxTriviaList.Empty;

			if (node.HasLeadingTrivia &&
				(node.IsKind(SyntaxKind.FieldDeclaration) // View
				|| node.IsKind(SyntaxKind.AttributeList) // BQL in attribute
				|| node.IsKind(SyntaxKind.SimpleMemberAccessExpression))) // Static call
			{
				return node.GetLeadingTrivia();
			}

			return GetDefaultLeadingTrivia(node.Parent);
		}
	}
}
