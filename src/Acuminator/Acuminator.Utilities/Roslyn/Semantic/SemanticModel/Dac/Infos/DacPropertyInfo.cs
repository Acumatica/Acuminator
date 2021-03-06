﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

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

		DacPropertyInfo IWriteableBaseItem<DacPropertyInfo>.Base
		{
			get => Base;
			set
			{
				Base = value;
				
				if (value != null && BoundType == BoundType.NotDefined) //Inherit BoundType from the base property only if we don't override it
				{
					EffectiveBoundType = value.EffectiveBoundType;
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
		/// The effective type of the property.
		/// </value>
		public ITypeSymbol EffectivePropertyType { get; }

		/// <summary>
		/// The bound type declared on this property.
		/// </summary>
		public BoundType BoundType { get; }

		/// <summary>
		/// The effective bound type for this property obtained by the combination of <see cref="BoundType"/>s of this propety's override chain. 
		/// </summary>
		public BoundType EffectiveBoundType
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
			baseInfo.ThrowOnNull(nameof(baseInfo));
			Base = baseInfo;

			if (BoundType == BoundType.NotDefined)
			{
				EffectiveBoundType = baseInfo.EffectiveBoundType;
			}
		}

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, ITypeSymbol effectivePropertyType,
								  int declarationOrder, bool isDacProperty, IEnumerable<AttributeInfo> attributeInfos) :
							 base(node, symbol, declarationOrder)
		{
			Attributes = attributeInfos.ToImmutableArray();
			IsDacProperty = isDacProperty;

			BoundType boundType = BoundType.NotDefined;
			bool isIdentity = false;
			bool isPrimaryKey = false;
			bool isAutoNumbering = false;

			foreach (AttributeInfo attributeInfo in Attributes)
			{
				boundType = boundType.Combine(attributeInfo.BoundType);
				isIdentity = isIdentity || attributeInfo.IsIdentity;
				isPrimaryKey = isPrimaryKey || attributeInfo.IsKey;
				isAutoNumbering = isAutoNumbering || attributeInfo.IsAutoNumberAttribute;
			}

			BoundType = boundType;
			EffectiveBoundType = BoundType;
			EffectivePropertyType = effectivePropertyType;
			IsIdentity = isIdentity;
			IsKey = isPrimaryKey;
			IsAutoNumbering = isAutoNumbering;
		}

		public static DacPropertyInfo Create(PXContext context, PropertyDeclarationSyntax node, IPropertySymbol property, int declarationOrder,
											 AttributeInformation attributesInformation, IDictionary<string, DacFieldInfo> dacFields,
											 DacPropertyInfo baseInfo = null)
		{
			context.ThrowOnNull(nameof(context));
			property.ThrowOnNull(nameof(property));
			attributesInformation.ThrowOnNull(nameof(attributesInformation));
			dacFields.ThrowOnNull(nameof(dacFields));

			bool isDacProperty = dacFields.ContainsKey(property.Name);
			var attributeInfos = GetAttributeInfos(property, attributesInformation);
			var effectivePropertyType = property.Type.GetUnderlyingTypeFromNullable(context) ?? property.Type;

			return baseInfo != null
				? new DacPropertyInfo(node, property, effectivePropertyType, declarationOrder, isDacProperty, attributeInfos, baseInfo)
				: new DacPropertyInfo(node, property, effectivePropertyType, declarationOrder, isDacProperty, attributeInfos);
		}

		private static IEnumerable<AttributeInfo> GetAttributeInfos(IPropertySymbol property, AttributeInformation attributeInformation)
		{
			int relativeDeclarationOrder = 0;

			foreach (AttributeData attribute in property.GetAttributes())
			{			
				yield return AttributeInfo.Create(attribute, attributeInformation, relativeDeclarationOrder);

				relativeDeclarationOrder++;
			}
		}			
	}
}