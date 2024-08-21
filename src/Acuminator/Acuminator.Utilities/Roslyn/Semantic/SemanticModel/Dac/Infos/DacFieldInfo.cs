#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	/// <summary>
	/// Information about a DAC field - a pair consisting of a DAC field property and a DAC BQL field declared in the same type.
	/// </summary>
	public class DacFieldInfo : IWriteableBaseItem<DacFieldInfo>
	{
		public string Name { get; }

		public ITypeSymbol? DacType { get; }

		public DacPropertyInfo? PropertyInfo { get; }

		public DacBqlFieldInfo? BqlFieldInfo { get; }

		protected DacFieldInfo? _baseInfo;

		public DacFieldInfo? Base => _baseInfo;

		DacFieldInfo? IWriteableBaseItem<DacFieldInfo>.Base
		{
			get => Base;
			set 
			{
				_baseInfo = value;

				if (value != null)
					CombineWithBaseInfo(value);
			}
		}

		public int DeclarationOrder => PropertyInfo?.DeclarationOrder ?? BqlFieldInfo!.DeclarationOrder;

		public bool HasFieldProperty { get; }

		public bool HasBqlField { get; }

		/// <value>
		/// The type of the DAC field property.
		/// </value>
		public ITypeSymbol? FieldPropertyType { get; }

		/// <value>
		/// The effective type of the property. For reference types and non nullable value types it is the same as <see cref="PropertyType"/>. 
		/// For nulable value types it is the underlying type extracted from nullable. It is <c>T</c> for <see cref="Nullable{T}"/>.
		/// </value>
		public ITypeSymbol? EffectivePropertyType { get; }

		/// <summary>
		/// The DB boundness calculated from attributes declared on this DAC property.
		/// </summary>
		public DbBoundnessType DeclaredDbBoundness { get; }

		/// <summary>
		/// The effective bound type for this DAC field obtained by the combination of <see cref="DeclaredDbBoundness"/>s of this propety's override chain. 
		/// </summary>
		public DbBoundnessType EffectiveDbBoundness { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsAutoNumbering { get; }

		public bool HasAcumaticaAttributes { get; }

		public DacFieldInfo(DacPropertyInfo? dacPropertyInfo, DacBqlFieldInfo? dacBqlFieldInfo)
		{
			if (dacPropertyInfo == null && dacBqlFieldInfo == null)
				throw new ArgumentNullException($"Both {nameof(dacPropertyInfo)} and {nameof(dacBqlFieldInfo)} parameters cannot be null.");

			PropertyInfo = dacPropertyInfo;
			BqlFieldInfo = dacBqlFieldInfo;
			Name 		 = PropertyInfo?.Name ?? BqlFieldInfo!.Name.ToPascalCase();
			DacType 	 = PropertyInfo?.Symbol.ContainingType ?? BqlFieldInfo!.Symbol.ContainingType;

			DacFieldMetadata metadata = DacFieldMetadata.FromDacFieldInfo(this);
			HasFieldProperty	   = metadata.HasFieldProperty;
			HasBqlField 		   = metadata.HasBqlField;
			IsIdentity 			   = metadata.IsIdentity;
			IsKey 				   = metadata.IsKey;
			IsAutoNumbering 	   = metadata.IsAutoNumbering;
			FieldPropertyType 	   = metadata.FieldPropertyType;
			EffectivePropertyType  = metadata.EffectivePropertyType;
			DeclaredDbBoundness    = metadata.DeclaredDbBoundness;
			EffectiveDbBoundness   = metadata.EffectiveDbBoundness;
			HasAcumaticaAttributes = metadata.HasAcumaticaAttributes;
		}

		public bool IsDeclaredInType(ITypeSymbol? type) =>
			 PropertyInfo?.Symbol.IsDeclaredInType(type) ?? BqlFieldInfo!.Symbol.IsDeclaredInType(type);

		void IWriteableBaseItem<DacFieldInfo>.CombineWithBaseInfo(DacFieldInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		private void CombineWithBaseInfo(DacFieldInfo baseInfo)
		{
			// No need to combine anything here, combination is done in the constructor
		}

		protected readonly record struct DacFieldMetadata(bool HasBqlField, bool HasFieldProperty, bool HasAcumaticaAttributes, bool IsKey, 
														  bool IsIdentity, bool IsAutoNumbering, ITypeSymbol? FieldPropertyType,
														  ITypeSymbol? EffectivePropertyType, DbBoundnessType DeclaredDbBoundness, 
														  DbBoundnessType EffectiveDbBoundness)
		{
			public static DacFieldMetadata FromDacFieldInfo(DacFieldInfo fieldInfo)
			{
				var fieldsChain = fieldInfo.ThisAndOverridenItems();
				bool hasBqlField = false;
				bool hasFieldProperty = false;
				bool? isKey = null, isIdentity = null, isAutoNumbering = null, hasAcumaticaAttributes = null;
				ITypeSymbol? fieldPropertyType = null, effectivePropertyType = null;
				DbBoundnessType? declaredDbBoundness = null, effectiveDbBoundness = null;

				foreach (DacFieldInfo fieldInChain in fieldsChain)
				{
					var propertyInfo = fieldInChain.PropertyInfo;

					if (propertyInfo != null)
					{
						hasFieldProperty = true;
						isKey 				   ??= propertyInfo.IsKey;
						isIdentity 			   ??= propertyInfo.IsIdentity;
						isAutoNumbering 	   ??= propertyInfo.IsAutoNumbering;
						fieldPropertyType 	   ??= propertyInfo.PropertyType;
						effectivePropertyType  ??= propertyInfo.EffectivePropertyType;
						declaredDbBoundness    ??= propertyInfo.DeclaredDbBoundness;
						effectiveDbBoundness   ??= propertyInfo.EffectiveDbBoundness;
						hasAcumaticaAttributes ??= propertyInfo.HasAcumaticaAttributesEffective;
					}

					if (fieldInChain.BqlFieldInfo != null)
					{
						hasBqlField = true;
					}
				}

				return new DacFieldMetadata(hasBqlField, hasFieldProperty, hasAcumaticaAttributes ?? false, 
											isKey ?? false, isIdentity ?? false, isAutoNumbering ?? false,
											fieldPropertyType, effectivePropertyType, declaredDbBoundness ?? DbBoundnessType.NotDefined,
											effectiveDbBoundness ?? DbBoundnessType.NotDefined);
			}
		}
	}
}
