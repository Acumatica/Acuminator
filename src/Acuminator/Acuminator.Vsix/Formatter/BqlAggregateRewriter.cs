using System.Linq;
using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities;

namespace Acuminator.Vsix.Formatter
{
	class BqlAggregateRewriter : BqlRewriterBase
	{
		private readonly INamedTypeSymbol _currentType;

		public BqlAggregateRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}

		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol originalSymbol = GetOriginalTypeSymbol(node);

			if (originalSymbol != null)
			{
				INamedTypeSymbol currentType = null;
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
						INamedTypeSymbol nextParam = GetTypeSymbol(node.TypeArgumentList.Arguments[1])?.OriginalDefinition;
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