#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacBqlFieldInfo : NodeSymbolItem<ClassDeclarationSyntax, INamedTypeSymbol>, IWriteableBaseItem<DacBqlFieldInfo>
	{
		/// <summary>
		/// The overriden dac field if any
		/// </summary>
		public DacBqlFieldInfo? Base
		{
			get;
			internal set;
		}
	
		DacBqlFieldInfo? IWriteableBaseItem<DacBqlFieldInfo>.Base
		{
			get => Base;
			set => Base = value;
		}

		protected DacBqlFieldInfo(ClassDeclarationSyntax node, INamedTypeSymbol bqlField, int declarationOrder) :
							 base(node, bqlField, declarationOrder)
		{
		}

		protected DacBqlFieldInfo(ClassDeclarationSyntax node, INamedTypeSymbol bqlField, int declarationOrder, DacBqlFieldInfo baseInfo) :
							 this(node, bqlField, declarationOrder)
		{
			Base = baseInfo.CheckIfNull();
		}

		public static DacBqlFieldInfo Create(PXContext pxContext, ClassDeclarationSyntax node, INamedTypeSymbol bqlField, int declarationOrder,
											 DacBqlFieldInfo? baseInfo = null)
		{
			return CreateUnsafe(pxContext.CheckIfNull(), node, bqlField.CheckIfNull(), declarationOrder, baseInfo);
		}

		internal static DacBqlFieldInfo CreateUnsafe(PXContext pxContext, ClassDeclarationSyntax node, INamedTypeSymbol bqlField, int declarationOrder,
													 DacBqlFieldInfo? baseInfo = null)
		{
			return baseInfo != null
				? new DacBqlFieldInfo(node, bqlField, declarationOrder, baseInfo)
				: new DacBqlFieldInfo(node, bqlField, declarationOrder);
		}
	}
}