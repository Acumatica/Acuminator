using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public class AttributeInformation
	{
		private readonly PXContext _context;
		private readonly SemanticModel _semanticModel;
		private static int depth = 0; 
		public ImmutableDictionary<ITypeSymbol, ITypeSymbol> CorrespondingSimpleTypes { get; }

		public ImmutableHashSet<ITypeSymbol> UnboundFieldAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> BoundFieldAttributes { get; }
		public ImmutableHashSet<ITypeSymbol> AllFieldAttributes { get; }

		public AttributeInformation(PXContext pxContext,SemanticModel semanticModel)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;
			_semanticModel = semanticModel;
			var unboundFieldAttributes = GetUnboundFieldAttributes(_context);
			UnboundFieldAttributes = unboundFieldAttributes.ToImmutableHashSet();

			var boundFieldAttributes = GetBoundFieldAttributes(_context);
			BoundFieldAttributes = boundFieldAttributes.ToImmutableHashSet();
			AllFieldAttributes = unboundFieldAttributes.Concat(boundFieldAttributes).ToImmutableHashSet();
			CorrespondingSimpleTypes = GetCorrespondingSimpleTypes(_context).ToImmutableDictionary();
		}

		public ITypeSymbol GetTypeSymbol(SyntaxNode attribute)
		{
			attribute.ThrowOnNull(nameof(attribute));

			return null;
		}

		public bool ContainsBaseType(ITypeSymbol attributeSymbol, ITypeSymbol type)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));
			type.ThrowOnNull(nameof(type));

			List<ITypeSymbol> attributeTypeHierarchy = attributeSymbol.GetBaseTypesAndThis().ToList();
			if (attributeTypeHierarchy.Contains(type))
				return true;	
			return false;
		}

		public bool AttributeDerivedFromClass(ITypeSymbol attributeSymbol, ITypeSymbol type,int depth = 10)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));
			type.ThrowOnNull(nameof(type));
			if (depth <= 0 || depth > 100)
				return false;
			if (ContainsBaseType(attributeSymbol, type))
				return true;
		//	PX.Data.PXAggregateAttribute
		//	PX.Data.PXDynamicAggregateAttribute
		
			var aggregateAttribute = _semanticModel.Compilation.GetTypeByMetadataName("PX.Data.PXAggregateAttribute");

			var dynamicAggregateAttribute = _semanticModel.Compilation.GetTypeByMetadataName("PX.Data.PXDynamicAggregateAttribute");
			if (ContainsBaseType(attributeSymbol, aggregateAttribute) || ContainsBaseType(attributeSymbol, dynamicAggregateAttribute))
			{
				var allAttributes = attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes().ToList();
				foreach (var attribute in allAttributes)
				{
					//go recursuion
					var result = AttributeDerivedFromClass(attribute, type, --depth);
					if (depth <= 0 || depth > 100)
						return false;
					if (result)
						return result;
				}
			}
			return false;
		}
		
		public FieldAttributeInfo GetFieldAttributeInfo(ITypeSymbol attributeSymbol)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));

			List<ITypeSymbol> attributeTypeHierarchy = attributeSymbol.GetBaseTypesAndThis().ToList();
			var info = CheckAttributeInheritanceChain(attributeSymbol, attributeTypeHierarchy);

			if (info.HasValue)
				return info.Value;

			var attributesOnHierarchy = attributeTypeHierarchy.SelectMany(a => a.GetAttributes())
															  .Select(a => a.AttributeClass);
														 
			foreach (ITypeSymbol attribute in attributesOnHierarchy)
			{
				info = CheckAttributeInheritanceChain(attribute);

				if (info.HasValue)
					return info.Value;
			}

			return new FieldAttributeInfo(isFieldAttribute: false, isBoundField: false, fieldType: null);
		}

		private FieldAttributeInfo? CheckAttributeInheritanceChain(ITypeSymbol attributeSymbol, List<ITypeSymbol> attributeTypeHierarchy = null)
		{
			var attributeBaseTypesEnum = attributeTypeHierarchy ?? attributeSymbol.GetBaseTypesAndThis();
			ITypeSymbol fieldAttribute = attributeBaseTypesEnum.TakeWhile(a => !a.Equals(_context.FieldAttributes.PXDBScalarAttribute))
															   .FirstOrDefault(a => AllFieldAttributes.Contains(a));

			if (fieldAttribute == null)
				return null;

			bool isBoundField = BoundFieldAttributes.Contains(fieldAttribute);
			return CorrespondingSimpleTypes.TryGetValue(fieldAttribute, out var fieldType)
				? new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType)
				: new FieldAttributeInfo(isFieldAttribute: true, isBoundField, fieldType: null);
		}

		private static HashSet<ITypeSymbol> GetUnboundFieldAttributes(PXContext pxContext) =>
			new HashSet<ITypeSymbol>
			{
				pxContext.FieldAttributes.PXLongAttribute,
				pxContext.FieldAttributes.PXIntAttribute,
				pxContext.FieldAttributes.PXShortAttribute,
				pxContext.FieldAttributes.PXStringAttribute,
				pxContext.FieldAttributes.PXByteAttribute,
				pxContext.FieldAttributes.PXDecimalAttribute,
				pxContext.FieldAttributes.PXFloatAttribute,
				pxContext.FieldAttributes.PXDoubleAttribute,
				pxContext.FieldAttributes.PXDateAttribute,
				pxContext.FieldAttributes.PXGuidAttribute,
				pxContext.FieldAttributes.PXBoolAttribute
			};

		private static HashSet<ITypeSymbol> GetBoundFieldAttributes(PXContext pxContext) =>
			new HashSet<ITypeSymbol>
			{
				pxContext.FieldAttributes.PXDBFieldAttribute,

				pxContext.FieldAttributes.PXDBLongAttribute,
				pxContext.FieldAttributes.PXDBIntAttribute,
				pxContext.FieldAttributes.PXDBShortAttribute,
				pxContext.FieldAttributes.PXDBStringAttribute,
				pxContext.FieldAttributes.PXDBByteAttribute,
				pxContext.FieldAttributes.PXDBDecimalAttribute,
				pxContext.FieldAttributes.PXDBDoubleAttribute,
				pxContext.FieldAttributes.PXDBFloatAttribute,
				pxContext.FieldAttributes.PXDBDateAttribute,
				pxContext.FieldAttributes.PXDBGuidAttribute,
				pxContext.FieldAttributes.PXDBBoolAttribute,
				pxContext.FieldAttributes.PXDBTimestampAttribute,
				pxContext.FieldAttributes.PXDBIdentityAttribute,
				pxContext.FieldAttributes.PXDBLongIdentityAttribute,
				pxContext.FieldAttributes.PXDBBinaryAttribute,
				pxContext.FieldAttributes.PXDBUserPasswordAttribute,
			};

		private static Dictionary<ITypeSymbol, ITypeSymbol> GetCorrespondingSimpleTypes(PXContext pxContext) =>
			new Dictionary<ITypeSymbol, ITypeSymbol>
			{
				{ pxContext.FieldAttributes.PXLongAttribute, pxContext.Int64 },
				{ pxContext.FieldAttributes.PXIntAttribute, pxContext.Int32 },
				{ pxContext.FieldAttributes.PXShortAttribute, pxContext.Int16 },
				{ pxContext.FieldAttributes.PXStringAttribute, pxContext.String },
				{ pxContext.FieldAttributes.PXByteAttribute, pxContext.Byte },
				{ pxContext.FieldAttributes.PXDecimalAttribute, pxContext.Decimal },
				{ pxContext.FieldAttributes.PXDoubleAttribute, pxContext.Double },
				{ pxContext.FieldAttributes.PXFloatAttribute, pxContext.Float },
				{ pxContext.FieldAttributes.PXDateAttribute, pxContext.DateTime },
				{ pxContext.FieldAttributes.PXGuidAttribute, pxContext.Guid },
				{ pxContext.FieldAttributes.PXBoolAttribute, pxContext.Bool },

				{ pxContext.FieldAttributes.PXDBLongAttribute, pxContext.Int64 },
				{ pxContext.FieldAttributes.PXDBIntAttribute, pxContext.Int32 },
				{ pxContext.FieldAttributes.PXDBShortAttribute, pxContext.Int16 },
				{ pxContext.FieldAttributes.PXDBStringAttribute, pxContext.String },
				{ pxContext.FieldAttributes.PXDBByteAttribute, pxContext.Byte },
				{ pxContext.FieldAttributes.PXDBDecimalAttribute, pxContext.Decimal },
				{ pxContext.FieldAttributes.PXDBDoubleAttribute, pxContext.Double },
				{ pxContext.FieldAttributes.PXDBFloatAttribute, pxContext.Float },
				{ pxContext.FieldAttributes.PXDBDateAttribute, pxContext.DateTime },
				{ pxContext.FieldAttributes.PXDBGuidAttribute, pxContext.Guid },
				{ pxContext.FieldAttributes.PXDBBoolAttribute, pxContext.Bool },
				{ pxContext.FieldAttributes.PXDBTimestampAttribute, pxContext.ByteArray },
				{ pxContext.FieldAttributes.PXDBIdentityAttribute, pxContext.Int32 },
				{ pxContext.FieldAttributes.PXDBLongIdentityAttribute, pxContext.Int64 },
				{ pxContext.FieldAttributes.PXDBBinaryAttribute, pxContext.ByteArray },
				{ pxContext.FieldAttributes.PXDBUserPasswordAttribute, pxContext.String },
			};
	}

}
