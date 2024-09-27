using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public abstract class DacOrDacExtInfoBase<TInfo> : DacOrDacExtInfoBase, IWriteableBaseItem<TInfo>
	where TInfo : DacOrDacExtInfoBase<TInfo>
	{
		public new TInfo? Base => base.Base as TInfo;

		TInfo? IWriteableBaseItem<TInfo>.Base
		{
			get => Base;
			set 
			{
				if (this is IWriteableBaseItem<OverridableNodeSymbolItem<DacOrDacExtInfoBase, ClassDeclarationSyntax, INamedTypeSymbol>> baseInterface)
					baseInterface.Base = value;
				else
				{
					_baseInfo = value;

					if (value != null)
						CombineWithBaseInfo(value);
				}
			}
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder, TInfo baseInfo) :
								 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
		{
		}

		void IWriteableBaseItem<TInfo>.CombineWithBaseInfo(TInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		protected sealed override void CombineWithBaseInfo(DacOrDacExtInfoBase baseInfo)
		{
			if (baseInfo is TInfo baseInfoTyped)
				CombineWithBaseInfo(baseInfoTyped);
			else
			{
				throw new ArgumentOutOfRangeException(nameof(baseInfo),
								$"Type \"{baseInfo.GetType().FullName}\" is not \"{typeof(TInfo).FullName}\" or derived from it.");
			}
		}

		/// <inheritdoc cref="IWriteableBaseItem{T}.CombineWithBaseInfo(T)"/>
		protected virtual void CombineWithBaseInfo(TInfo baseInfo)
		{

		}
	}


	public abstract class DacOrDacExtInfoBase : OverridableNodeSymbolItem<DacOrDacExtInfoBase, ClassDeclarationSyntax, INamedTypeSymbol>
	{
		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder, DacOrDacExtInfoBase baseInfo) :
								 base(node, dac, declarationOrder, baseInfo)
		{
		}

		protected DacOrDacExtInfoBase(ClassDeclarationSyntax? node, INamedTypeSymbol dac, int declarationOrder) :
								 base(node, dac, declarationOrder)
		{
		}
	}
}