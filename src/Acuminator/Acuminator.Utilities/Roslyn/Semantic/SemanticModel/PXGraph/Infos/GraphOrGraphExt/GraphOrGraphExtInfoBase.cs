#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public abstract class GraphOrGraphExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>
	{ 
		protected GraphOrGraphExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
									 base(node, dac, declarationOrder)
		{
		}
	}
}