using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Constants;

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

		public ImmutableArray<AttributeInfo> Attributes { get; }

		/// <summary>
		///  True if this property is DAC property - it has a corresponding DAC field.
		/// </summary
		public bool IsDacProperty { get; }

		/// <value>
		/// The type of the property.
		/// </value>
		public ITypeSymbol PropertyType => Symbol.Type;

		public BoundType BoundType { get; }

		public bool IsIdentity { get; }

		public bool IsKey { get; }

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder, bool isDacProperty,
								  IEnumerable<AttributeInfo> attributeInfos, DacPropertyInfo baseInfo) :
							 this(node, symbol, declarationOrder, isDacProperty, attributeInfos)
		{
			baseInfo.ThrowOnNull(nameof(baseInfo));
			Base = baseInfo;
		}

		protected DacPropertyInfo(PropertyDeclarationSyntax node, IPropertySymbol symbol, int declarationOrder,
								  bool isDacProperty, IEnumerable<AttributeInfo> attributeInfos) :
							 base(node, symbol, declarationOrder)
		{
			Attributes = attributeInfos.ToImmutableArray();
			IsDacProperty = isDacProperty;

			BoundType boundType = BoundType.NotDefined;
			bool isIdentity = false;
			bool isPrimaryKey = false;

			foreach (AttributeInfo attributeInfo in Attributes)
			{
				boundType = boundType.Combine(attributeInfo.BoundType);
				isIdentity = isIdentity || attributeInfo.IsIdentity;
				isPrimaryKey = isPrimaryKey || attributeInfo.IsKey;
			}

			BoundType = boundType;
			IsIdentity = isIdentity;
			IsKey = isPrimaryKey;
		}

		public static DacPropertyInfo Create(PropertyDeclarationSyntax node, IPropertySymbol property, int declarationOrder,
											 AttributeInformation attributesInformation, Dictionary<string, DacFieldInfo> dacFields,
											 DacPropertyInfo baseInfo = null)
		{
			property.ThrowOnNull(nameof(property));
			attributesInformation.ThrowOnNull(nameof(attributesInformation));
			dacFields.ThrowOnNull(nameof(dacFields));

			bool isDacProperty = dacFields.ContainsKey(property.Name);
			var attributeInfos = GetAttributeInfos(property, attributesInformation);

			return baseInfo != null
				? new DacPropertyInfo(node, property, declarationOrder, isDacProperty, attributeInfos, baseInfo)
				: new DacPropertyInfo(node, property, declarationOrder, isDacProperty, attributeInfos);
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