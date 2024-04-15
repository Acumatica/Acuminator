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
	public class DacPropertyInfo : NodeSymbolItem<PropertyDeclarationSyntax, IPropertySymbol>, IWriteableBaseItem<DacPropertyInfo>
	{
		/// <summary>
		/// The overriden property if any
		/// </summary>
		public DacPropertyInfo? Base
		{
			get;
			internal set;
		}

		DacPropertyInfo? IWriteableBaseItem<DacPropertyInfo>.Base
		{
			get => Base;
			set
			{
				Base = value;
				
				if (value != null)
				{
					// TODO - need to add support for PXMergeAttributesAttribute in the future
					EffectiveDbBoundness = DeclaredDbBoundness.Combine(value.EffectiveDbBoundness);
				}
			}
		}

		public ImmutableArray<AttributeInfo> Attributes { get; }

		/// <summary>
		///  True if this property is DAC property - it has a corresponding DAC field.
		/// </summary
		public bool IsDacProperty { get; }

		/// <value>
		/// The type of the property.
		/// </value>
		public ITypeSymbol PropertyType => Symbol.Type;

		/// <value>
		/// The effective type of the property. For reference types and non nullable value types it is the same as <see cref="PropertyType"/>. 
		/// For nulable value types it is the underlying type extracted from nullable. It is <c>T</c> for <see cref="Nullable{T}"/>.
		/// </value>
		public ITypeSymbol EffectivePropertyType { get; }

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

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, ITypeSymbol effectivePropertyType,
								  int declarationOrder, bool isDacProperty, IEnumerable<AttributeInfo> attributeInfos, DacPropertyInfo baseInfo) :
							 this(node, symbol, effectivePropertyType, declarationOrder, isDacProperty, attributeInfos)
		{
			Base = baseInfo.CheckIfNull();

			// TODO - need to add support for PXMergeAttributesAttribute in the future
			EffectiveDbBoundness = DeclaredDbBoundness.Combine(baseInfo.EffectiveDbBoundness);
		}

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, ITypeSymbol effectivePropertyType,
								  int declarationOrder, bool isDacProperty, IEnumerable<AttributeInfo> attributeInfos) :
							 base(node, symbol, declarationOrder)
		{
			Attributes = attributeInfos.ToImmutableArray();
			IsDacProperty = isDacProperty;

			DeclaredDbBoundness = Attributes.Select(a => a.DbBoundness).Combine();
			EffectiveDbBoundness = DeclaredDbBoundness;

			bool isIdentity = false;
			bool isPrimaryKey = false;
			bool isAutoNumbering = false;

			foreach (AttributeInfo attributeInfo in Attributes)
			{
				isIdentity = isIdentity || attributeInfo.IsIdentity;
				isPrimaryKey = isPrimaryKey || attributeInfo.IsKey;
				isAutoNumbering = isAutoNumbering || attributeInfo.IsAutoNumberAttribute;
			}
	
			EffectivePropertyType = effectivePropertyType;
			IsIdentity = isIdentity;
			IsKey = isPrimaryKey;
			IsAutoNumbering = isAutoNumbering;
		}

		public static DacPropertyInfo Create(PXContext context, PropertyDeclarationSyntax node, IPropertySymbol property, int declarationOrder,
											 DbBoundnessCalculator dbBoundnessCalculator, IDictionary<string, DacFieldInfo> dacFields,
											 DacPropertyInfo? baseInfo = null)
		{
			context.ThrowOnNull();
			property.ThrowOnNull();
			dbBoundnessCalculator.ThrowOnNull();
			dacFields.ThrowOnNull();

			bool isDacProperty = dacFields.ContainsKey(property.Name);
			var attributeInfos = GetAttributeInfos(property, dbBoundnessCalculator);
			var effectivePropertyType = property.Type.GetUnderlyingTypeFromNullable(context) ?? property.Type;

			return baseInfo != null
				? new DacPropertyInfo(node, property, effectivePropertyType, declarationOrder, isDacProperty, attributeInfos, baseInfo)
				: new DacPropertyInfo(node, property, effectivePropertyType, declarationOrder, isDacProperty, attributeInfos);
		}

		private static IEnumerable<AttributeInfo> GetAttributeInfos(IPropertySymbol property, DbBoundnessCalculator dbBoundnessCalculator)
		{
			int relativeDeclarationOrder = 0;

			foreach (AttributeData attribute in property.GetAttributes())
			{			
				yield return AttributeInfo.Create(attribute, dbBoundnessCalculator, relativeDeclarationOrder);

				relativeDeclarationOrder++;
			}
		}			
	}
}