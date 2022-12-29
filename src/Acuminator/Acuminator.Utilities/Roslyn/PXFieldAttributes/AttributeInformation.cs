﻿#nullable enable

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
	/// Helper used to retrieve information about the Acumatica attributes.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class AttributeInformation
	{
		private const int DefaultRecursionDepth = 10;
		
		private readonly INamedTypeSymbol _eventSubscriberAttribute;
		private readonly INamedTypeSymbol _dynamicAggregateAttribute;
		private readonly INamedTypeSymbol _aggregateAttribute;
		private readonly INamedTypeSymbol _defaultAttribute;
		private readonly INamedTypeSymbol _pxDBLocalizableStringAttribute;

		public PXContext Context { get; }

		public ImmutableHashSet<ITypeSymbol> BoundBaseTypes { get; }
		public ImmutableArray<(ITypeSymbol AttributeType, bool IsDbFieldDefaultValue)> TypesContainingIsDBField { get; }

		private const string IsDBField = "IsDBField";
		private const string NonDB = "NonDB";

		public AttributeInformation(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull(nameof(pxContext));
			BoundBaseTypes = GetBoundBaseTypes(Context).ToImmutableHashSet();
			TypesContainingIsDBField = GetTypesContainingIsDBField(Context)
											.OrderBy(typeWithValue => typeWithValue.AttributeType, TypeSymbolsByHierachyComparer.Instance)
											.ToImmutableArray();

			_eventSubscriberAttribute = Context.AttributeTypes.PXEventSubscriberAttribute;
			_dynamicAggregateAttribute = Context.AttributeTypes.PXDynamicAggregateAttribute;
			_aggregateAttribute = Context.AttributeTypes.PXAggregateAttribute;
			_defaultAttribute = Context.AttributeTypes.PXDefaultAttribute;
			_pxDBLocalizableStringAttribute = Context.FieldAttributes.PXDBLocalizableStringAttribute;
		}

		private static HashSet<ITypeSymbol> GetBoundBaseTypes(PXContext context)
		{
			var types = new HashSet<ITypeSymbol>();

			if (context.FieldAttributes.PXDBFieldAttribute != null)
				types.Add(context.FieldAttributes.PXDBFieldAttribute);

			if (context.FieldAttributes.PXDBCalcedAttribute != null)
				types.Add(context.FieldAttributes.PXDBCalcedAttribute);

			if (context.FieldAttributes.PXDBDataLengthAttribute != null)
				types.Add(context.FieldAttributes.PXDBDataLengthAttribute);

			return types;
		}

		private static IEnumerable<(ITypeSymbol AttributeType, bool IsDbFieldDefaultValue)> GetTypesContainingIsDBField(PXContext context)
		{
			if (context.FieldAttributes.PeriodIDAttribute != null)
				yield return (context.FieldAttributes.PeriodIDAttribute, true);

			if (context.FieldAttributes.AcctSubAttribute != null)
				yield return (context.FieldAttributes.AcctSubAttribute, true);

			if (context.FieldAttributes.UnboundAccountAttribute != null)
				yield return (context.FieldAttributes.UnboundAccountAttribute, false);

			if (context.FieldAttributes.PXEntityAttribute != null)
				yield return (context.FieldAttributes.PXEntityAttribute, true);		
		}
			

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
									   .ForEach(baseType => results.Add(baseType!));
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

			//First check attribute for IsDBField property, it takes highest priority in attribute's boundability and can appear in all kinds of attributes like Account/Sub attributes
			bool containsIsDbFieldproperty = attribute.AttributeClass.GetBaseTypesAndThis()
																	 .TakeWhile(attributeType => attributeType != _eventSubscriberAttribute)
																	 .SelectMany(attributeType => attributeType.GetMembers())
																	 .OfType<IPropertySymbol>()                                                                     //only properties are considered
																	 .Any(property => String.Equals(property.Name, IsDBField, StringComparison.OrdinalIgnoreCase));
			if (!containsIsDbFieldproperty)
				return GetBoundTypeFromStandardBoundTypeAttributes(attribute);

			var isDbPropertyAttributeArgs = attribute.NamedArguments.Where(arg => String.Equals(arg.Key, IsDBField, StringComparison.OrdinalIgnoreCase))
																	.ToList(capacity: 1);
			switch (isDbPropertyAttributeArgs.Count)
			{
				case 0:     //Case when there is IsDBField property but it isn't set explicitly in attribute's declaration
					var (typeFromRegister, defaultIsDbFieldValue) = 
						TypesContainingIsDBField.FirstOrDefault(typeWithValue => IsAttributeDerivedFromClass(attribute.AttributeClass, typeWithValue.AttributeType));

					// Query hard-coded register with attributes which has IsDBField property with some initial assigned value
					bool? dbFieldPreInitializedValue = typeFromRegister != null
						? defaultIsDbFieldValue
						: null;

					return dbFieldPreInitializedValue == null
						? GetBoundTypeFromStandardBoundTypeAttributes(attribute)
						: dbFieldPreInitializedValue.Value 
							? BoundType.DbBound 
							: BoundType.Unbound;
						
				case 1 when isDbPropertyAttributeArgs[0].Value.Value is bool isDbPropertyAttributeArgument:     //Case when IsDBField property is set explicitly with correct bool value
					return isDbPropertyAttributeArgument
						? BoundType.DbBound
						: BoundType.Unbound;

				case 1:                        //Strange rare case when IsDBField property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
				default:                       //Strange case when there are multiple different "IsDBField" properties set in attribute constructor (with different letters register case)
					return BoundType.Unknown;
			}
		}

		private BoundType GetBoundTypeFromStandardBoundTypeAttributes(AttributeData attribute)
		{
			if (BoundBaseTypes.Any(boundBaseType => IsAttributeDerivedFromClassInternal(attribute.AttributeClass, boundBaseType)))
			{
				if (_pxDBLocalizableStringAttribute != null && IsAttributeDerivedFromClassInternal(attribute.AttributeClass, _pxDBLocalizableStringAttribute))
					return GetBoundTypeFromLocalizableStringDerivedAttribute(attribute);

				return BoundType.DbBound;
			}

			return BoundType.Unbound;
		}

		private BoundType GetBoundTypeFromLocalizableStringDerivedAttribute(AttributeData attribute)
		{
			var (nonDbArgKey, nonDbArgValue) = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == NonDB);

			if (nonDbArgKey == null)
				return BoundType.DbBound;
			else if (nonDbArgValue.Value is bool nonDbArgValueBool)
			{
				return nonDbArgValueBool
					? BoundType.Unbound
					: BoundType.DbBound;
			}
			else
				return BoundType.Unknown;
		}

		/// <summary>
		/// Query if collection of attributes contains bound attribute.
		/// </summary>
		/// <param name="attributes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsBoundAttributes(IEnumerable<AttributeData> attributes) => 
			attributes.CheckIfNull(nameof(attributes))
					  .Any(a => GetBoundAttributeType(a) == BoundType.DbBound);
		

		/// <summary>
		/// Query if collection of attributes contains bound attribute. Overload for immutable array to prevent boxing.
		/// </summary>
		/// <param name="attributes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsBoundAttributes(ImmutableArray<AttributeData> attributes) => 
			attributes.Any(a => GetBoundAttributeType(a) == BoundType.DbBound);

		/// <summary>
		/// Query if collection of attributes contains unbound attribute.
		/// </summary>
		/// <param name="attributes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsUnboundAttributes(IEnumerable<AttributeData> attributes) =>
			attributes.CheckIfNull(nameof(attributes))
					  .Any(a => GetBoundAttributeType(a) == BoundType.Unbound);

		/// <summary>
		/// Query if collection of attributes contains unbound attribute. Overload for immutable array to prevent boxing.
		/// </summary>
		/// <param name="attributes">The attributes collection.</param>
		/// <returns/>
		public bool ContainsUnboundAttributes(ImmutableArray<AttributeData> attributes) =>
			attributes.Any(a => GetBoundAttributeType(a) == BoundType.Unbound);

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