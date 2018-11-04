using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphInitializerInfo : GraphNodeSymbolItem<SyntaxNode, ISymbol>
	{
		public GraphInitializerType Type { get; }

		protected override string DebuggerDisplay => this.ToString();

		public GraphInitializerInfo(GraphInitializerType type, SyntaxNode node, ISymbol symbol, int declarationOrder)
			: base(node, symbol, declarationOrder)
		{
			Type = type;
		}
	}
}
