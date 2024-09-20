#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacPropertyInfo : OverridableNodeSymbolItem<DacPropertyInfo, PropertyDeclarationSyntax, IPropertySymbol>
	{
		public ImmutableArray<DacFieldAttributeInfo> Attributes { get; }

		/// <summary>
		///  True if this property has a corresponding declared DAC field.
		/// </summary
		public bool HasBqlFieldDeclared { get;  }

		/// <summary>
		/// The effective indicator if this property has a corresponding DAC field including base properties.
		/// </summary
		public bool HasBqlFieldEffective { get; private set; }

		/// <summary>
		///  True if this property has declared Acumatica attributes, false if not.
		/// </summary>
		public bool HasAcumaticaAttributesDeclared { get; private set; }

		/// <summary>
		/// The effective indicator if this property has Acumatica attributes on it including base properties.
		/// </summary>
		public bool HasAcumaticaAttributesEffective { get; private set; }

		/// <value>
		/// The type of the property.
		/// </value>
		public ITypeSymbol PropertyType => Symbol.Type;

		/// <value>
		/// The effective type of the property. For reference types and non nullable value types it is the same as <see cref="PropertyType"/>. 
		/// For nulable value types it is the underlying type extracted from nullable. It is <c>T</c> for <see cref="Nullable{T}"/>.
		/// </value>
		public ITypeSymbol PropertyTypeUnwrappedNullable { get; }

		/// <summary>
		/// The DB boundness calculated from attributes declared on this DAC property.
		/// </summary>
		public DbBoundnessType DeclaredDbBoundness { get; }

		/// <summary>
		/// The effective bound type for this property obtained by the combination of <see cref="DeclaredDbBoundness"/>s of this propety's override chain. 
		/// </summary>
		public DbBoundnessType EffectiveDbBoundness
		{
			get;
			private set;
		}

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		public bool IsAutoNumbering { get; }

		protected DacPropertyInfo(PropertyDeclarationSyntax? node, IPropertySymbol symbol, ITypeSymbol propertyTypeUnwrappedNullable,
								  int declarationOrder, bool hasBqlField, IEnumerable<DacFieldAttributeInfo> attributeInfos, 
								  DacPropertyInfo baseInfo) :
							 this(node, symbol, propertyTypeUnwrappedNullable, declarationOrder, hasBqlField, attributeInfos)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		protected DacPropertyInfo(PropertyDeclarationSyntax? node, IPropertySymbol symbol, ITypeSymbol propertyTypeUnwrappedNullable,
								  int declarationOrder, bool hasBqlField, IEnumerable<DacFieldAttributeInfo> attributeInfos) :
							 base(node, symbol, declarationOrder)
		{
			Attributes 			 = attributeInfos.ToImmutableArray();
			HasBqlFieldDeclared  = hasBqlField;
			HasBqlFieldEffective = hasBqlField;

			DeclaredDbBoundness = Attributes.Select(a => a.DbBoundness).Combine();
			EffectiveDbBoundness = DeclaredDbBoundness;

			bool isIdentity = false;
			bool isPrimaryKey = false;
			bool isAutoNumbering = false;
			bool hasAcumaticaAttributes = false;

			foreach (DacFieldAttributeInfo attributeInfo in Attributes)
			{
				isIdentity 			   = isIdentity || attributeInfo.IsIdentity;
				isPrimaryKey 		   = isPrimaryKey || attributeInfo.IsKey;
				isAutoNumbering 	   = isAutoNumbering || attributeInfo.IsAutoNumberAttribute;
				hasAcumaticaAttributes = hasAcumaticaAttributes || attributeInfo.IsAcumaticaAttribute;
			}
	
			PropertyTypeUnwrappedNullable = propertyTypeUnwrappedNullable;
			IsIdentity 			  		  = isIdentity;
			IsKey 				  		  = isPrimaryKey;
			IsAutoNumbering 	  		  = isAutoNumbering;

			HasAcumaticaAttributesDeclared  = hasAcumaticaAttributes;
			HasAcumaticaAttributesEffective = hasAcumaticaAttributes;
		}

		public static DacPropertyInfo Create(PXContext context, PropertyDeclarationSyntax? node, IPropertySymbol property, int declarationOrder,
											 DbBoundnessCalculator dbBoundnessCalculator, IDictionary<string, DacBqlFieldInfo> dacFields,
											 DacPropertyInfo? baseInfo = null)
		{
			return CreateUnsafe(context.CheckIfNull(), node, property.CheckIfNull(), declarationOrder, 
								dbBoundnessCalculator.CheckIfNull(), dacFields.CheckIfNull(), baseInfo);
		}

		internal static DacPropertyInfo CreateUnsafe(PXContext context, PropertyDeclarationSyntax? node, IPropertySymbol property, int declarationOrder,
													 DbBoundnessCalculator dbBoundnessCalculator, IDictionary<string, DacBqlFieldInfo> dacFields,
													 DacPropertyInfo? baseInfo = null)
		{
			bool hasBqlField = dacFields.ContainsKey(property.Name);
			var attributeInfos = GetAttributeInfos(property, dbBoundnessCalculator);
			var propertyTypeUnwrappedNullable = property.Type.GetUnderlyingTypeFromNullable(context) ?? property.Type;

			return baseInfo != null
				? new DacPropertyInfo(node, property, propertyTypeUnwrappedNullable, declarationOrder, hasBqlField, attributeInfos, baseInfo)
				: new DacPropertyInfo(node, property, propertyTypeUnwrappedNullable, declarationOrder, hasBqlField, attributeInfos);
		}

		private static IEnumerable<DacFieldAttributeInfo> GetAttributeInfos(IPropertySymbol property, DbBoundnessCalculator dbBoundnessCalculator)
		{
			int relativeDeclarationOrder = 0;

			foreach (AttributeData attribute in property.GetAttributes())
			{			
				yield return DacFieldAttributeInfo.CreateUnsafe(attribute, dbBoundnessCalculator, relativeDeclarationOrder);

				relativeDeclarationOrder++;
			}
		}

		protected override void CombineWithBaseInfo(DacPropertyInfo baseProperty)
		{
			// TODO - need to add support for PXMergeAttributesAttribute in the future
			EffectiveDbBoundness 			= DeclaredDbBoundness.Combine(baseProperty.EffectiveDbBoundness);
			HasBqlFieldEffective 			= HasBqlFieldDeclared || baseProperty.HasBqlFieldEffective;
			HasAcumaticaAttributesEffective = HasAcumaticaAttributesDeclared || baseProperty.HasAcumaticaAttributesEffective;
		}
	}
}