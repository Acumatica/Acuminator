using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Helper used to classify Acumatica attributes.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	
	public enum BoundType
	{
		Unknown = 0,
		Unbound = 1,
		DbBound = 2,
		NotDefined = 3
	}
	/// <summary>
	/// Helper used to retrieve information about the Acumatica attributes.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class AttributeInformation
	{
		private const int DefaultRecursionDepth = 10;
		private readonly PXContext _context;

		private readonly INamedTypeSymbol _eventSubscriberAttribute;
		private readonly INamedTypeSymbol _dynamicAggregateAttribute;
		private readonly INamedTypeSymbol _aggregateAttribute;
		private readonly INamedTypeSymbol _defaultAttribute;

		public ImmutableHashSet<ITypeSymbol> BoundBaseTypes { get; }
		public ImmutableDictionary<ITypeSymbol,bool> TypesContainingIsDBField { get; }

		private const string IsDBField = "IsDBField";

		public AttributeInformation(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;

			var boundBaseTypes = GetBoundBaseTypes(_context);
			Dictionary<ITypeSymbol, bool> typesContainingIsDBField = GetTypesContainingIsDBField(_context);

			BoundBaseTypes = boundBaseTypes.ToImmutableHashSet();
			TypesContainingIsDBField = typesContainingIsDBField.ToImmutableDictionary();

			_eventSubscriberAttribute = _context.AttributeTypes.PXEventSubscriberAttribute;
			_dynamicAggregateAttribute = _context.AttributeTypes.PXDynamicAggregateAttribute;
			_aggregateAttribute = _context.AttributeTypes.PXAggregateAttribute;
			_defaultAttribute = _context.AttributeTypes.PXDefaultAttribute;
		}

		private static HashSet<ITypeSymbol> GetBoundBaseTypes(PXContext context) =>
			new HashSet<ITypeSymbol>
			{
				context.FieldAttributes.PXDBFieldAttribute,
				context.FieldAttributes.PXDBCalcedAttribute,
				context.FieldAttributes.PXDBDataLengthAttribute,
			};

		private static Dictionary<ITypeSymbol, bool> GetTypesContainingIsDBField(PXContext context) =>
			new Dictionary<ITypeSymbol, bool>
			{
				{	context.FieldAttributes.PeriodIDAttribute, true },
				{	context.FieldAttributes.AcctSubAttribute, true },
			};

		/// <summary>
		/// Get the collection of Acumatica attributes defined by the <paramref name="attributeType"/> including attributes on aggregates.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="includeBaseTypes">(Optional) True to include, false to exclude the base Acumatica types.</param>
		/// <returns/>
		public IEnumerable<ITypeSymbol> GetAcumaticaAttributesFullList(ITypeSymbol attributeType, bool includeBaseTypes = false)
		{
			if (attributeType == null || attributeType.Equals(_eventSubscriberAttribute))
				return Enumerable.Empty<ITypeSymbol>();

			var baseAcumaticaAttributeTypes = attributeType.GetBaseTypesAndThis().ToList();

			if (!baseAcumaticaAttributeTypes.Contains(_eventSubscriberAttribute))
				return Enumerable.Empty<ITypeSymbol>();

			HashSet<ITypeSymbol> results;

			if (includeBaseTypes)
			{
				results = baseAcumaticaAttributeTypes.TakeWhile(a => !a.Equals(_eventSubscriberAttribute))
													 .ToHashSet();
			}
			else
			{
				results = new HashSet<ITypeSymbol>() { attributeType };
			}

			bool isAggregateAttribute = baseAcumaticaAttributeTypes.Contains(_aggregateAttribute) ||
										baseAcumaticaAttributeTypes.Contains(_dynamicAggregateAttribute);

			if (isAggregateAttribute)
			{
				var allAcumaticaAttributes = attributeType.GetAllAttributesDefinedOnThisAndBaseTypes()
														  .Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute));

				foreach (var attribute in allAcumaticaAttributes)
				{
					CollectAggregatedAttribute(attribute, DefaultRecursionDepth);
				}
			}

			return results;


			void CollectAggregatedAttribute(ITypeSymbol aggregatedAttribute, int depth)
			{
				results.Add(aggregatedAttribute);

				if (depth < 0)
					return;

				if (includeBaseTypes)
				{
					aggregatedAttribute.GetBaseTypes()
									   .TakeWhile(baseType => !baseType.Equals(_eventSubscriberAttribute))
									   .ForEach(baseType => results.Add(baseType));
				}

				if (IsAggregatorAttribute(aggregatedAttribute))
				{
					var allAcumaticaAttributes = aggregatedAttribute.GetAllAttributesDefinedOnThisAndBaseTypes()
																	.Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute));

					foreach (var attribute in allAcumaticaAttributes)
					{
						CollectAggregatedAttribute(attribute, depth - 1);
					}
				}
			}
		}

		/// <summary>
		/// Check if Acumatica attribute is derived from the specified Acumatica attribute type. If non Acumatica attributes are passed then <c>flase</c> is returned.
		/// </summary>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <param name="typeToCheck">The base attribute type to check.</param>
		/// <returns>
		/// True if attribute derived from <paramref name="typeToCheck"/>, false if not.
		/// </returns>
		public bool IsAttributeDerivedFromClass(ITypeSymbol attributeType, ITypeSymbol typeToCheck)
		{
			attributeType.ThrowOnNull(nameof(attributeType));
			typeToCheck.ThrowOnNull(nameof(typeToCheck));

			if (!attributeType.InheritsFromOrEquals(_eventSubscriberAttribute) || !typeToCheck.InheritsFromOrEquals(_eventSubscriberAttribute))
				return false;

			return IsAttributeDerivedFromClassInternal(attributeType, typeToCheck);
		}

		/// <summary>
		/// Query if Acumatica attribute is bound.
		/// </summary>
		/// <param name="attribute">Data of the attribute.</param>
		/// <returns/>
		public BoundType GetBoundAttributeType(AttributeData attribute)
		{
			attribute.ThrowOnNull(nameof(attribute));

			if (!attribute.AttributeClass.InheritsFromOrEquals(_eventSubscriberAttribute) ||
			    attribute.AttributeClass.InheritsFromOrEquals(_defaultAttribute))
			{
			    return BoundType.NotDefined;
			}

			if (BoundBaseTypes.Any(boundBaseType => IsAttributeDerivedFromClassInternal(attribute.AttributeClass, boundBaseType)))
				return BoundType.DbBound;

			bool containsIsDbFieldproperty =
					attribute.AttributeClass.GetMembers().OfType<IPropertySymbol>()  //only properties considered
														 .Any(property => IsDBField.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
			
			if (containsIsDbFieldproperty)
			{
				var isDbPropertyAttributeArgs = attribute.NamedArguments.Where(arg => IsDBField.Equals(arg.Key, StringComparison.OrdinalIgnoreCase)).ToList();    //case insensitive check

				var isDBFieldSetInFalseInBaseAttribute = TypesContainingIsDBField.Any(t => 
					t.Key != null && t.Value == false && IsAttributeDerivedFromClass(attribute.AttributeClass, t.Key));

				var isDBFieldSetInTrueInBaseAttribute = TypesContainingIsDBField.Any(t =>
					t.Key != null && t.Value == true && IsAttributeDerivedFromClass(attribute.AttributeClass, t.Key));

				if (isDbPropertyAttributeArgs.Count == 0)
				{
					if (isDBFieldSetInFalseInBaseAttribute) // IsDBField = false property defined in base Acumatica class
						return BoundType.Unbound;
					else if (isDBFieldSetInTrueInBaseAttribute) // IsDBField = true property defined in base Acumatica class 
						return BoundType.DbBound;
				}

				if (isDbPropertyAttributeArgs.Count != 1)  //rare case when there are multiple different "IsDBField" considered
					return BoundType.Unknown;

				if (!(isDbPropertyAttributeArgs[0].Value.Value is bool isDbPropertyAttributeArgument))
					return BoundType.Unknown;  //if there is null or values of type other than bool then we don't know if attribute is bound

				return isDbPropertyAttributeArgument
					? BoundType.DbBound
					: BoundType.Unbound;
			}

			return BoundType.Unbound;
		}

		/// <summary>
		/// Query if collection of attributes contains bound attribute.
		/// </summary>
		/// <param name="attributes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsBoundAttributes(IEnumerable<AttributeData> attributes)
		{
			attributes.ThrowOnNull();

			return attributes.Any(a => GetBoundAttributeType(a) == BoundType.DbBound);
		}

		/// <summary>
		/// Query if collection of attributes contains unbound attribute.
		/// </summary>
		/// <param name="attributes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsUnboundAttributes(IEnumerable<AttributeData> attributes)
		{
			attributes.ThrowOnNull();

			return attributes.Any(a => GetBoundAttributeType(a) == BoundType.Unbound);
		}

		private bool IsAttributeDerivedFromClassInternal(ITypeSymbol attributeType, ITypeSymbol typeToCheck, int depth = DefaultRecursionDepth)
		{
			if (depth < 0)
				return false;
			else if (attributeType.InheritsFromOrEquals(typeToCheck))
				return true;

			if (IsAggregatorAttribute(attributeType))
			{
				return attributeType.GetAllAttributesDefinedOnThisAndBaseTypes()
									.Where(attribute => attribute.InheritsFrom(_eventSubscriberAttribute))
									.Any(attribute => IsAttributeDerivedFromClassInternal(attribute, typeToCheck, depth - 1));
			}

			return false;
		}

		private bool IsAggregatorAttribute(ITypeSymbol attributeType) =>
			attributeType.InheritsFromOrEquals(_aggregateAttribute) || attributeType.InheritsFromOrEquals(_dynamicAggregateAttribute);

	}
}