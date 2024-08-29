#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacBqlFieldInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IWriteableBaseItem<DacBqlFieldInfo>
	{
		protected DacBqlFieldInfo? _baseInfo;

		/// <summary>
		/// The overriden dac field if any
		/// </summary>
		public DacBqlFieldInfo? Base => _baseInfo;
	
		DacBqlFieldInfo? IWriteableBaseItem<DacBqlFieldInfo>.Base
		{
			get => Base;
			set 
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		protected DacBqlFieldInfo(ClassDeclarationSyntax? node, INamedTypeSymbol bqlField, int declarationOrder) :
							 base(node, bqlField, declarationOrder)
		{
		}

		protected DacBqlFieldInfo(ClassDeclarationSyntax? node, INamedTypeSymbol bqlField, int declarationOrder, DacBqlFieldInfo baseInfo) :
							 this(node, bqlField, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		public static DacBqlFieldInfo Create(PXContext pxContext, ClassDeclarationSyntax? node, INamedTypeSymbol bqlField, int declarationOrder,
											 DacBqlFieldInfo? baseInfo = null)
		{
			return CreateUnsafe(pxContext.CheckIfNull(), node, bqlField.CheckIfNull(), declarationOrder, baseInfo);
		}

		internal static DacBqlFieldInfo CreateUnsafe(PXContext pxContext, ClassDeclarationSyntax? node, INamedTypeSymbol bqlField, int declarationOrder,
													 DacBqlFieldInfo? baseInfo = null)
		{
			return baseInfo != null
				? new DacBqlFieldInfo(node, bqlField, declarationOrder, baseInfo)
				: new DacBqlFieldInfo(node, bqlField, declarationOrder);
		}

		void IWriteableBaseItem<DacBqlFieldInfo>.CombineWithBaseInfo(DacBqlFieldInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		private void CombineWithBaseInfo(DacBqlFieldInfo baseInfo)
		{
		}
	}
}