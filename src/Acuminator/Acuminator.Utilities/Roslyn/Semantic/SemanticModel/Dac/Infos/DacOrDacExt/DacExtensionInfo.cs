#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacExtensionInfo : DacOrDacExtInfoBase
	{ 
		public DacExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
						   base(node, dac, declarationOrder)
		{
		}
	}
}