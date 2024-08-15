#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphExtensionInfo : GraphOrGraphExtInfoBase
	{ 
		public GraphExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
							 base(node, dac, declarationOrder)
		{
		}
	}
}