using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;


namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacPropertyInfo : NodeSymbolItem<PropertyDeclarationSyntax, IPropertySymbol>
	{
		/// <summary>
		/// The overriden property if any
		/// </summary>
		public DacPropertyInfo Base { get; }

		public DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder) :
						  base(node, symbol, declarationOrder)
		{
		}

		public DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder, DacPropertyInfo baseInfo) :
						  this(node, symbol, declarationOrder)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));
			Base = baseInfo;
		}
	}
}