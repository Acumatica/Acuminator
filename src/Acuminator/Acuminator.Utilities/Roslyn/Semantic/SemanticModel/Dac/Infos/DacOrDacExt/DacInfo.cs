#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacInfo : DacOrDacExtInfoBase
	{ 
		public DacInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
				  base(node, dac, declarationOrder)
		{
		}
	}
}