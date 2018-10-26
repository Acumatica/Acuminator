using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the cache attached event in graph.
	/// </summary>
	public class CacheAttachedInfo : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		public ITypeSymbol DAC { get; }

		public CacheAttachedInfo(MethodDeclarationSyntax node, IMethodSymbol symbol, ITypeSymbol dac, int declarationOrder) :
							base(node, symbol, declarationOrder)
		{
			dac.ThrowOnNull(nameof(dac));

			DAC = dac;
		}
	}
}
