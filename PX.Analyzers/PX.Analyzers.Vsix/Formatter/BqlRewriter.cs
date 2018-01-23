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
		private readonly Stack<SyntaxTriviaList> _currentIndentation = new Stack<SyntaxTriviaList>();

		public BqlRewriter(BqlContext context, SemanticModel semanticModel,
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia)
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, SyntaxTriviaList.Empty)
		{
		}


		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol constructedFromSymbol = typeSymbol?.ConstructedFrom; // get generic type

			bool withIndent = false;

			if (constructedFromSymbol != null)
			{
				if (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase)
				         || constructedFromSymbol.ImplementsInterface(Context.IBqlSelect)
				         || constructedFromSymbol.ImplementsInterface(Context.IBqlSearch)) // TODO: could be Coalesce - handle this case in the future
				{
					withIndent = true;
				}
				else if (constructedFromSymbol.ImplementsInterface(Context.IBqlJoin)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlWhere)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlOrderBy))
				{
					node = WithNewLineAndIndentation(node);
					withIndent = true;
				}
				else if (constructedFromSymbol.ImplementsInterface(Context.IBqlSortColumn))
				{
					node = WithNewLineAndIndentation(node);
				}
			}
			
			if (withIndent) IncreaseCurrentIndentation();
			var result = base.VisitGenericName(node);
			if (withIndent) DecreaseCurrentIndentation();
			return result;
		}

		public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
		{
			SyntaxNode topParent = node.Parent?.Parent; // IdentifierNameSyntax -> TypeArgumentSyntaxList -> GenericNameSyntax
			if (topParent != null)
			{
				if (topParent.IsKind(SyntaxKind.InvocationExpression)) // static Select
				{
					IncreaseCurrentIndentation();
					node = WithNewLineAndIndentation(node);
					DecreaseCurrentIndentation();
				}
				else
				{
					INamedTypeSymbol parentType = GetTypeSymbol(topParent);
					INamedTypeSymbol constructedFromSymbol = parentType?.ConstructedFrom; // get generic type

					if (constructedFromSymbol != null)
					{
						if (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase)
						    || constructedFromSymbol.ImplementsInterface(Context.IBqlSelect)
						    || constructedFromSymbol.ImplementsInterface(Context.IBqlSearch)) // TODO: could be Coalesce - handle this case in the future
						{
							node = WithNewLineAndIndentation(node);
						}
					}
				}
			}

			return base.VisitIdentifierName(node);
		}

		public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
		{
			// Move BQL View field name to a new line and indent it
			var parentNode = node.Parent as VariableDeclarationSyntax;
			var genericNameNode = parentNode?.Type as GenericNameSyntax;

			if (genericNameNode != null)
			{
				INamedTypeSymbol parentType = GetTypeSymbol(genericNameNode);
				INamedTypeSymbol constructedFromSymbol = parentType?.ConstructedFrom; // get generic type

				if (constructedFromSymbol != null
					&& constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase))
				{
					IncreaseCurrentIndentation();
					node = WithNewLineAndIndentation(node);
					DecreaseCurrentIndentation();
				}
			}

			return base.VisitVariableDeclarator(node);
		}

		private T WithNewLineAndIndentation<T>(T node)
			where T : SyntaxNode
		{
			SyntaxTriviaList defaultTrivia = GetDefaultLeadingTrivia(node);
			SyntaxTriviaList currentTrivia = GetCurrentIndentationTrivia(node);
			SyntaxTriviaList newTrivia = EndOfLineTrivia.AddRange(defaultTrivia.AddRange(currentTrivia));
			
			return node.WithLeadingTrivia(newTrivia);
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

		private SyntaxTriviaList GetCurrentIndentationTrivia(SyntaxNode node)
		{
			if (node == null) return SyntaxTriviaList.Empty;
			
			SyntaxTriviaList result = SyntaxTriviaList.Empty;
			foreach (SyntaxTriviaList trivia in _currentIndentation)
			{
				result = result.AddRange(trivia);
			}
			return result;
		}

		private void IncreaseCurrentIndentation()
		{
			_currentIndentation.Push(IndentationTrivia);
		}

		private void DecreaseCurrentIndentation()
		{
			_currentIndentation.Pop();
		}
	}
}
