using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	/// <summary>
	/// Collects all modifications without modifying original syntax tree
	/// </summary>
	class BqlRewritingPlanner : BqlRewritingPlannerBase
	{
		public BqlRewritingPlanner(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, SyntaxTriviaList.Empty)
		{
		}


		public override void VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol constructedFromSymbol = typeSymbol?.ConstructedFrom; // get generic type
			
			if (constructedFromSymbol != null)
			{
				if (constructedFromSymbol.ImplementsInterface(Context.IBqlJoin)
					|| constructedFromSymbol.ImplementsInterface(Context.IBqlWhere)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlOrderBy))
				{
					Set(node, OnNewLineAndIndentet(node));
				}
			}

			base.VisitGenericName(node);
		}

		public override void VisitIdentifierName(IdentifierNameSyntax node)
		{
			SyntaxNode genericNameNode = node.Parent?.Parent; // IdentifierNameSyntax -> TypeArgumentSyntaxList -> GenericNameSyntax
			if (genericNameNode != null)
			{
				INamedTypeSymbol parentType = GetTypeSymbol(genericNameNode);
				INamedTypeSymbol constructedFromSymbol = parentType?.ConstructedFrom; // get generic type

				if (constructedFromSymbol != null)
				{
					if (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase)
					    || constructedFromSymbol.ImplementsInterface(Context.IBqlSelect)
					    || constructedFromSymbol.ImplementsInterface(Context.IBqlSearch)) // TODO: could be Coalesce - handle this case in the future
					{
						Set(node, OnNewLineAndIndentet(node));
					}
				}
			}

			base.VisitIdentifierName(node);
		}

		private SyntaxNode OnNewLineAndIndentet(SyntaxNode node)
		{
			return node.WithLeadingTrivia(EndOfLineTrivia
				.AddRange(GetDefaultLeadingTrivia(node))
				.AddRange(IndentationTrivia));
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
