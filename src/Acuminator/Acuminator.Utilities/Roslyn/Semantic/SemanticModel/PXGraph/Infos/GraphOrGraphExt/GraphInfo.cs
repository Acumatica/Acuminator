#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphInfo : GraphOrGraphExtInfoBase
	{ 
		public GraphInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
					base(node, dac, declarationOrder)
		{
		}
	}
}