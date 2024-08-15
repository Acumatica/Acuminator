#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public abstract class DacOrDacExtInfoBase : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>
	{ 
		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
		{
		}
	}
}