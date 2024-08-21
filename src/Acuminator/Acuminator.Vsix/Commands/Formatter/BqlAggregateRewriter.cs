#nullable enable

using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Vsix.Formatter
{
	class BqlAggregateRewriter : BqlRewriterBase
	{
		public BqlAggregateRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol? originalSymbol = GetOriginalTypeSymbol(node);

			if (originalSymbol != null)
			{
				INamedTypeSymbol? currentType = null;
				if (originalSymbol.InheritsFromOrEqualsGeneric(Context.GroupByBase))
				{
					currentType = Context.GroupByBase;
				}
				else if (originalSymbol.ImplementsInterface(Context.IBqlFunction))
				{
					currentType = Context.IBqlFunction;
				}

				if (currentType != null)
				{ 
					SyntaxTriviaList trivia = DefaultLeadingTrivia;
					if (node.TypeArgumentList.Arguments.Count > 1) // GroupBy node changes for AggregateFunction (Min / Max / etc.)
					{
						INamedTypeSymbol? nextParam = GetTypeSymbol(node.TypeArgumentList.Arguments[1])?.OriginalDefinition;
						if (nextParam != null && !nextParam.InheritsFromOrEqualsGeneric(currentType, true))
							trivia = IndentedDefaultTrivia;
					}
					
					return RewriteGenericNode(node, new BqlAggregateRewriter(this, trivia));
				}
			}

			return base.VisitGenericName(node);
		}
	}
}