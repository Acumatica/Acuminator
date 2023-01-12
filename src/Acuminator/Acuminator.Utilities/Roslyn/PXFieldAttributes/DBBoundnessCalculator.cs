#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Helper used to retrieve the information about Acumatica attributes DB boundness.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class DbBoundnessCalculator
	{		
		private readonly INamedTypeSymbol _eventSubscriberAttribute;
		private readonly INamedTypeSymbol _defaultAttribute;
		private readonly INamedTypeSymbol _pxDBLocalizableStringAttribute;

		public PXContext Context { get; }

		public ImmutableArray<ITypeSymbol> BoundBaseTypes { get; }

		public ImmutableArray<MixedDbBoundnessAttributeInfo> AttributesContainingIsDBField { get; }

		private const string IsDBField = "IsDBField";
		private const string NonDB = "NonDB";

		public DbBoundnessCalculator(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull(nameof(pxContext));
			BoundBaseTypes = GetBoundBaseTypes(Context).ToImmutableArray();
			AttributesContainingIsDBField = FieldTypeAttributesRegister.GetTypesContainingIsDBField(Context)
											.OrderBy(typeWithValue => typeWithValue.AttributeType, TypeSymbolsByHierachyComparer.Instance)
											.ToImmutableArray();

			_eventSubscriberAttribute = Context.AttributeTypes.PXEventSubscriberAttribute;
			_defaultAttribute = Context.AttributeTypes.PXDefaultAttribute;
			_pxDBLocalizableStringAttribute = Context.FieldAttributes.PXDBLocalizableStringAttribute;
		}

		private static List<ITypeSymbol> GetBoundBaseTypes(PXContext context)
		{
			var types = new List<ITypeSymbol>();

			if (context.FieldAttributes.PXDBFieldAttribute != null)
				types.Add(context.FieldAttributes.PXDBFieldAttribute);

			if (context.FieldAttributes.PXDBCalcedAttribute != null)
				types.Add(context.FieldAttributes.PXDBCalcedAttribute);

			if (context.FieldAttributes.PXDBDataLengthAttribute != null)
				types.Add(context.FieldAttributes.PXDBDataLengthAttribute);

			return types;
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

			if (!attribute.AttributeClass.IsAcumaticaAttribute(Context) || attribute.AttributeClass.InheritsFromOrEquals(_defaultAttribute))
				return BoundType.NotDefined;

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
						AttributesContainingIsDBField
							.FirstOrDefault(typeWithValue => attribute.AttributeClass.IsDerivedFromAttribute(typeWithValue.AttributeType, Context));

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
			if (BoundBaseTypes.Any(boundBaseType => attribute.AttributeClass.IsDerivedFromAttributeUnsafe(boundBaseType, Context)))
			{
				if (_pxDBLocalizableStringAttribute != null && 
					attribute.AttributeClass.IsDerivedFromAttributeUnsafe(_pxDBLocalizableStringAttribute, Context))
				{
					return GetBoundTypeFromLocalizableStringDerivedAttribute(attribute);
				}

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
	}
}