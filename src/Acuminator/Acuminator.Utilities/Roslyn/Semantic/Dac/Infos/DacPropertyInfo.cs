using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;


namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacPropertyInfo : NodeSymbolItem<PropertyDeclarationSyntax, IPropertySymbol>, IWriteableBaseItem<DacPropertyInfo>
	{
		/// <summary>
		/// The overriden property if any
		/// </summary>
		public DacPropertyInfo Base
		{
			get;
			internal set;
		}

		DacPropertyInfo IOverridableItem<DacPropertyInfo>.Base => Base;

		DacPropertyInfo IWriteableBaseItem<DacPropertyInfo>.Base
		{
			get => Base;
			set => Base = value;
		}

		public string Name => Symbol.Name;

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