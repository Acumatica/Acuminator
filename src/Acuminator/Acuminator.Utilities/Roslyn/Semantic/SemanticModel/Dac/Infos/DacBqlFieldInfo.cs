#nullable enable
using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacBqlFieldInfo : OverridableNodeSymbolItem<DacBqlFieldInfo, ClassDeclarationSyntax, INamedTypeSymbol>, IEquatable<DacBqlFieldInfo>
	{
		/// <summary>
		/// The declared BQL field data type corresponding to the BQL field type. 
		/// <see langword="null"/> if the BQL field is weakly typed and just implements <c>IBqlField</c> interface.
		/// </summary>
		/// <remarks>
		/// For a BQL field type <c>BqlString</c> <see cref="BqlFieldDataTypeDeclared"/> will be <see cref="string"/>.<br/> 
		/// For a BQL field type <c>BqlByteArray</c> <see cref="BqlFieldDataTypeDeclared"/> will be <c>byte[]</c>.
		/// </remarks>
		public ITypeSymbol? BqlFieldDataTypeDeclared { get; }

		/// <summary>
		/// The effective BQL field data type obtained via a combination of <see cref="BqlFieldDataTypeDeclared"/> from this and base BQL field infos.
		/// </summary>
		public ITypeSymbol? BqlFieldDataTypeEffective { get; private set; }

		protected DacBqlFieldInfo(ClassDeclarationSyntax? node, INamedTypeSymbol bqlField, int declarationOrder, DacBqlFieldInfo baseInfo) :
							 this(node, bqlField, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		protected DacBqlFieldInfo(ClassDeclarationSyntax? node, INamedTypeSymbol bqlField, int declarationOrder) :
							 base(node, bqlField, declarationOrder)
		{
			ITypeSymbol? bqlFieldDataType = bqlField.GetBqlFieldDataTypeFromBqlFieldSymbol();

			BqlFieldDataTypeDeclared  = bqlFieldDataType;
			BqlFieldDataTypeEffective = bqlFieldDataType;
		}

		public static DacBqlFieldInfo Create(PXContext pxContext, ClassDeclarationSyntax? bqlFieldNode, INamedTypeSymbol bqlField,
											  int declarationOrder, DacBqlFieldInfo? baseInfo = null)
		{
			return CreateUnsafe(pxContext.CheckIfNull(), bqlFieldNode, bqlField.CheckIfNull(), declarationOrder, baseInfo);
		}

		internal static DacBqlFieldInfo CreateUnsafe(PXContext pxContext, ClassDeclarationSyntax? bqlFieldNode, INamedTypeSymbol bqlField, 
													 int declarationOrder, DacBqlFieldInfo? baseInfo = null)
		{
			return baseInfo != null
				? new DacBqlFieldInfo(bqlFieldNode, bqlField, declarationOrder, baseInfo)
				: new DacBqlFieldInfo(bqlFieldNode, bqlField, declarationOrder);
		}

		protected override void CombineWithBaseInfo(DacBqlFieldInfo baseInfo)
		{
			BqlFieldDataTypeEffective ??= baseInfo.BqlFieldDataTypeEffective;
		}

		public override bool Equals(object obj) => Equals(obj as DacBqlFieldInfo);

		public bool Equals(DacBqlFieldInfo? other) => SymbolEqualityComparer.Default.Equals(Symbol, other?.Symbol);

		public override int GetHashCode() => SymbolEqualityComparer.Default.GetHashCode(Symbol);
	}
}