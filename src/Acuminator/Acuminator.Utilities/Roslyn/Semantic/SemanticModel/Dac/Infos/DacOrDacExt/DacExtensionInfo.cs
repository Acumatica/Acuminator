#nullable enable

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacExtensionInfo : DacOrDacExtInfoBase<DacExtensionInfo>
	{
		public DacExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder, DacExtensionInfo baseInfo) :
						   this(node, dac, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		public DacExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
						   base(node, dac, declarationOrder)
		{
		}
	}
}