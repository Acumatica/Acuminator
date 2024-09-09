#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	/// <summary>
	/// Information about a DAC field - a pair consisting of a DAC field property and a DAC BQL field declared in the same type.
	/// </summary>
	public class DacFieldInfo : IWriteableBaseItem<DacFieldInfo>, IEquatable<DacFieldInfo>
	{
		public string Name { get; }

		public ITypeSymbol DacType { get; }

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

		/// <summary>
		/// Flag indicating whether the DAC field has a DAC field property in the containing DAC or DAC extension, 
		/// and their base and chained types.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(BqlFieldInfo))]
		public bool HasFieldPropertyEffective { get; private set; }

		/// <summary>
		/// Flag indicating whether this particular DAC field has a DAC field property in the containing DAC or DAC extension
		/// without considering its base type.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(BqlFieldInfo))]
		[MemberNotNullWhen(returnValue: true, nameof(PropertyInfo))]
		public bool HasFieldPropertyDeclared { get; }

		/// <summary>
		/// Flag indicating whether the DAC field has a DAC BQL field in the containing DAC or DAC extension, 
		/// and their base and chained types.
		/// </summary>
		[MemberNotNullWhen(returnValue: false, nameof(PropertyInfo))]
		public bool HasBqlFieldEffective { get; private set; }

		/// <summary>
		/// Flag indicating whether this particular DAC field has a DAC BQL field property in the containing DAC or DAC extension
		/// without considering its base type.
		/// </summary>
		[MemberNotNullWhen(returnValue: true, nameof(BqlFieldInfo))]
		[MemberNotNullWhen(returnValue: false, nameof(PropertyInfo))]
		public bool HasBqlFieldDeclared { get; }

		/// <value>
		/// The type of the DAC field property.
		/// </value>
		public ITypeSymbol? FieldPropertyType { get; private set; }

		/// <value>
		/// The effective type of the property. For reference types and non nullable value types it is the same as <see cref="PropertyType"/>. 
		/// For nulable value types it is the underlying type extracted from nullable. It is <c>T</c> for <see cref="Nullable{T}"/>.
		/// </value>
		public ITypeSymbol? EffectivePropertyType { get; private set; }

		/// <summary>
		/// The DB boundness calculated from attributes declared on this DAC property.
		/// </summary>
		public DbBoundnessType DeclaredDbBoundness { get; }

		/// <summary>
		/// The effective bound type for this DAC field obtained by the combination of <see cref="DeclaredDbBoundness"/>s of this propety's override chain. 
		/// </summary>
		public DbBoundnessType EffectiveDbBoundness { get; private set; }

		public bool IsIdentity { get; private set; }

		public bool IsKey { get; private set; }

		public bool IsAutoNumbering { get; private set; }

		public bool HasAcumaticaAttributes { get; private set; }

		public DacFieldInfo(DacPropertyInfo? dacPropertyInfo, DacBqlFieldInfo? dacBqlFieldInfo, DacFieldInfo baseInfo) :
					   this(dacPropertyInfo, dacBqlFieldInfo)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		public DacFieldInfo(DacPropertyInfo? dacPropertyInfo, DacBqlFieldInfo? dacBqlFieldInfo)
		{
			if (dacPropertyInfo == null && dacBqlFieldInfo == null)
				throw new ArgumentNullException($"Both {nameof(dacPropertyInfo)} and {nameof(dacBqlFieldInfo)} parameters cannot be null.");

			PropertyInfo = dacPropertyInfo;
			BqlFieldInfo = dacBqlFieldInfo;
			Name 		 = PropertyInfo?.Name ?? BqlFieldInfo!.Name.ToPascalCase();
			DacType 	 = PropertyInfo?.Symbol.ContainingType ?? BqlFieldInfo!.Symbol.ContainingType;

			HasBqlFieldDeclared  = dacBqlFieldInfo != null;
			HasBqlFieldEffective = HasBqlFieldDeclared;

			if (dacPropertyInfo != null)
			{
				HasFieldPropertyEffective = true;
				HasFieldPropertyDeclared  = true;
				IsKey 					  = dacPropertyInfo.IsKey;
				IsIdentity 				  = dacPropertyInfo.IsIdentity;
				IsAutoNumbering 		  = dacPropertyInfo.IsAutoNumbering;
				FieldPropertyType 		  = dacPropertyInfo.PropertyType;
				EffectivePropertyType 	  = dacPropertyInfo.EffectivePropertyType;
				DeclaredDbBoundness 	  = dacPropertyInfo.DeclaredDbBoundness;
				EffectiveDbBoundness 	  = dacPropertyInfo.EffectiveDbBoundness;
				HasAcumaticaAttributes 	  = dacPropertyInfo.HasAcumaticaAttributesEffective;
			}
			else
			{
				HasFieldPropertyEffective = false;
				HasFieldPropertyDeclared  = false;
				IsKey 					  = false;
				IsIdentity 				  = false;
				IsAutoNumbering 		  = false;
				FieldPropertyType 		  = null;
				EffectivePropertyType 	  = null;
				DeclaredDbBoundness 	  = DbBoundnessType.NotDefined;
				EffectiveDbBoundness 	  = DbBoundnessType.NotDefined;
				HasAcumaticaAttributes 	  = false;
			}
		}

		public bool IsDeclaredInType(ITypeSymbol? type) =>
			 PropertyInfo?.Symbol.IsDeclaredInType(type) ?? BqlFieldInfo!.Symbol.IsDeclaredInType(type);

		void IWriteableBaseItem<DacFieldInfo>.CombineWithBaseInfo(DacFieldInfo baseInfo) => CombineWithBaseInfo(baseInfo);

		private void CombineWithBaseInfo(DacFieldInfo baseInfo)
		{
			HasAcumaticaAttributes 	  = HasAcumaticaAttributes 	  || baseInfo.HasAcumaticaAttributes;
			HasBqlFieldEffective 	  = HasBqlFieldEffective 	  || baseInfo.HasBqlFieldEffective;
			HasFieldPropertyEffective = HasFieldPropertyEffective || baseInfo.HasFieldPropertyEffective;
			IsKey 					  = IsKey 					  || baseInfo.IsKey;
			IsIdentity 				  = IsIdentity 				  || baseInfo.IsIdentity;
			IsAutoNumbering 		  = IsAutoNumbering 		  || baseInfo.IsAutoNumbering;
			HasAcumaticaAttributes 	  = HasAcumaticaAttributes 	  || baseInfo.HasAcumaticaAttributes;

			FieldPropertyType	  ??= baseInfo.FieldPropertyType;
			EffectivePropertyType ??= baseInfo.EffectivePropertyType;

			EffectiveDbBoundness = DeclaredDbBoundness.Combine(baseInfo.EffectiveDbBoundness);
		}

		public override string ToString() => Name;

		public override bool Equals(object obj) => Equals(obj as DacFieldInfo);

		public bool Equals(DacFieldInfo? other) =>
			other != null &&
			SymbolEqualityComparer.Default.Equals(PropertyInfo?.Symbol, other.PropertyInfo?.Symbol) &&
			Equals(BqlFieldInfo, other.BqlFieldInfo);

		public override int GetHashCode()
		{
			int hash = 17;

			unchecked
			{
				hash = hash * 23 + SymbolEqualityComparer.Default.GetHashCode(PropertyInfo?.Symbol);
				hash = hash * 23 + (BqlFieldInfo?.GetHashCode() ?? 0);
			}
			
			return hash;
		}
	}
}
