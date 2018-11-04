using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class ProcessingDelegateInfo : GraphNodeSymbolItem<SyntaxNode, ISymbol>
	{
		public ProcessingDelegateInfo(SyntaxNode node, ISymbol symbol, int declarationOrder)
			: base(node, symbol, declarationOrder)
		{
		}
	}
}
