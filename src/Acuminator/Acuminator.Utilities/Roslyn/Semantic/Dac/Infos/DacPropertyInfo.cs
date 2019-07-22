using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

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
			set => Base = value;
		}

		public ImmutableArray<AttributeData> Attributes { get; }

		/// <summary>
		///  True if this property is DAC property - it has a corresponding DAC field.
		/// </summary
		public bool IsDacProperty { get; }

		public FieldTypeAttributeInfo FieldTypeInfo
		{
			get;
			private set;
		}

		public bool IsIdentity
		{
			get;
			private set;
		}

		public bool IsKey
		{
			get;
			private set;
		}

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder, bool isDacProperty,
								  DacPropertyInfo baseInfo) :
							 this(node, symbol, declarationOrder, isDacProperty)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));
			Base = baseInfo;
		}

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder, bool isDacProperty) :
							 base(node, symbol, declarationOrder)
		{
			Attributes = symbol.GetAttributes();
			IsDacProperty = isDacProperty;
		}

		public static DacPropertyInfo Create(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder,
											 FieldTypeAttributesRegister attributesRegister, Dictionary<string, DacFieldInfo> dacFields,
											 DacPropertyInfo baseInfo = null)
		{
			symbol.ThrowOnNull(nameof(symbol));
			attributesRegister.ThrowOnNull(nameof(attributesRegister));
			dacFields.ThrowOnNull(nameof(dacFields));

			bool isDacProperty = dacFields.ContainsKey(symbol.Name);
			DacPropertyInfo propertyInfo = baseInfo != null
				? new DacPropertyInfo(node, symbol, declarationOrder, isDacProperty, baseInfo)
				: new DacPropertyInfo(node, symbol, declarationOrder, isDacProperty);

			if (!propertyInfo.IsDacProperty || propertyInfo.Attributes.Length == 0)
				return propertyInfo;

			//foreach (var item in collection)
			//{
			//	attributesRegister.GetFieldTypeAttributeInfos
			//}

			

			return propertyInfo;
		}

		private Dictionary<AttributeData, List<FieldTypeAttributeInfo>> GetFieldTypeAttributesInfos(FieldTypeAttributesRegister fieldAttributesRegister)
		{
			var fieldTypeInfosByAttribute = new Dictionary<AttributeData, List<FieldTypeAttributeInfo>>(capacity: Attributes.Length);

			foreach (AttributeData attribute in Attributes)
			{
				var attributeInfos = fieldAttributesRegister.GetFieldTypeAttributeInfos(attribute.AttributeClass).ToList();

				if (attributeInfos.Count == 0)
					continue;

				fieldTypeInfosByAttribute.Add(attribute, attributeInfos);
			}

			return fieldTypeInfosByAttribute;
		}
	}
}