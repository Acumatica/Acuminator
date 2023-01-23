#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Helper used to retrieve the information about DB boundness of concrete application of Acumatica attribute.
	/// </summary>
	/// <remarks>
	/// By Acumatica atribute we mean an attribute derived from PXEventSubscriberAttribute.
	/// </remarks>
	public class DbBoundnessCalculator
	{
		private const string IsDBField = "IsDBField";
		private const string NonDB = "NonDB";

		private readonly INamedTypeSymbol _eventSubscriberAttribute;
		private readonly INamedTypeSymbol _defaultAttribute;
		private readonly INamedTypeSymbol _pxDBLocalizableStringAttribute;

		public PXContext Context { get; }

		public FieldTypeAttributesMetadataProvider AttributesRegister { get; }

		public DbBoundnessCalculator(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull(nameof(pxContext));
			AttributesRegister = new FieldTypeAttributesMetadataProvider(pxContext);
					
			_eventSubscriberAttribute = Context.AttributeTypes.PXEventSubscriberAttribute;
			_defaultAttribute = Context.AttributeTypes.PXDefaultAttribute;
			_pxDBLocalizableStringAttribute = Context.FieldAttributes.PXDBLocalizableStringAttribute;
		}

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <returns/>
		public DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication)
		{
			attributeApplication.ThrowOnNull(nameof(attributeApplication));

			if (!attributeApplication.AttributeClass.IsAcumaticaAttribute(Context) || attributeApplication.AttributeClass.InheritsFromOrEquals(_defaultAttribute))
				return DbBoundnessType.NotDefined;

			//First check if DB boundness is set explicitly on attribute application. In that case it will override all attribute metadata
			DbBoundnessType explicitlySetDbBoundness = GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication);

			if (explicitlySetDbBoundness != DbBoundnessType.NotDefined)
				return explicitlySetDbBoundness;

			// If the explicit DB boundness is not set we can query attribute's register for the metadata
			var attributeInfos = AttributesRegister.GetDacFieldTypeAttributeInfos(attributeApplication.AttributeClass);

			



			var flattenedAttributes = attributeApplication.AttributeClass.GetThisAndAllAggregatedAttributes(Context, includeBaseTypes: true);




			//First check attribute for IsDBField property, it takes highest priority in attribute's boundability and can appear in all kinds of attributes like Account/Sub attributes
			bool containsIsDbFieldproperty = attributeApplication.AttributeClass.GetBaseTypesAndThis()
																	 .TakeWhile(attributeType => attributeType != _eventSubscriberAttribute)
																	 .SelectMany(attributeType => attributeType.GetMembers())
																	 .OfType<IPropertySymbol>()                                                                     //only properties are considered
																	 .Any(property => String.Equals(property.Name, IsDBField, StringComparison.OrdinalIgnoreCase));
			if (!containsIsDbFieldproperty)
				return GetBoundTypeFromStandardBoundTypeAttributes(attributeApplication);

			var isDbPropertyAttributeArgs = attributeApplication.NamedArguments.Where(arg => String.Equals(arg.Key, IsDBField, StringComparison.OrdinalIgnoreCase))
																	.ToList(capacity: 1);
			switch (isDbPropertyAttributeArgs.Count)
			{
				case 0:     //Case when there is IsDBField property but it isn't set explicitly in attribute's declaration
					var (typeFromRegister, defaultIsDbFieldValue) = 
						AttributesContainingIsDBField
							.FirstOrDefault(typeWithValue => attributeApplication.AttributeClass.IsDerivedFromAttribute(typeWithValue.AttributeType, Context));

					// Query hard-coded register with attributes which has IsDBField property with some initial assigned value
					bool? dbFieldPreInitializedValue = typeFromRegister != null
						? defaultIsDbFieldValue
						: null;

					return dbFieldPreInitializedValue == null
						? GetBoundTypeFromStandardBoundTypeAttributes(attributeApplication)
						: dbFieldPreInitializedValue.Value 
							? DbBoundnessType.DbBound 
							: DbBoundnessType.Unbound;
						
				case 1 when isDbPropertyAttributeArgs[0].Value.Value is bool isDbPropertyAttributeArgument:     //Case when IsDBField property is set explicitly with correct bool value
					return isDbPropertyAttributeArgument
						? DbBoundnessType.DbBound
						: DbBoundnessType.Unbound;

				case 1:                        
				default:                       
					return DbBoundnessType.Unknown;
			}

			return DuckTypingCheckIfAttributeHasMixedDbBoundness(flattenedAttributes);
		}

		private DbBoundnessType GetDbBoundnessSetExplicitlyByAttributeApplication(AttributeData attributeApplication)
		{
			if (attributeApplication.NamedArguments.IsDefaultOrEmpty)
				return DbBoundnessType.NotDefined;

			int isDbFieldCounter = 0, isNonDbCounter = 0;
			bool? isDbFieldValue = null, isNonDbValue = null;

			foreach (var (argumentName, argumentValue) in attributeApplication.NamedArguments)
			{
				if (string.Equals(argumentName, IsDBField, StringComparison.OrdinalIgnoreCase))
				{
					isDbFieldCounter++;

					if (argumentValue.Value is bool boolIsDbFieldValue)
						isDbFieldValue = boolIsDbFieldValue;
				}

				if (string.Equals(argumentName, NonDB, StringComparison.OrdinalIgnoreCase))
				{
					isNonDbCounter++;

					if (argumentValue.Value is bool boolNonDbValue)
						isNonDbValue = boolNonDbValue;
				}

			}

			if (isDbFieldCounter > 1 || isNonDbCounter > 1)
				return DbBoundnessType.Unknown;     //Strange case when there are multiple different "IsDBField"/"NonDB" properties set in attribute constructor (with different letters register case)
			
			bool isDbFieldPresent = isDbFieldCounter == 1;
			bool isNonDbPresent   = isNonDbCounter == 1;

			if (!isDbFieldPresent && !isNonDbPresent)
				return DbBoundnessType.NotDefined;
			else if (isDbFieldPresent && isNonDbPresent)    //Strange case when there are both IsDBField and NonDB properties
			{
				DbBoundnessType isDbFieldBoundness = GetIsDbFieldBoundness(isDbFieldValue);
				DbBoundnessType nonDbBoundness = GetNonDbBoundness(isNonDbValue);
				return isDbFieldBoundness.Combine(nonDbBoundness);
			}
			else if (isDbFieldPresent)
				return GetIsDbFieldBoundness(isDbFieldValue);
			else
				return GetNonDbBoundness(isNonDbValue);

			//------------------------------------Local function--------------------------------------------------------------------
			static DbBoundnessType GetIsDbFieldBoundness(bool? isDbFieldValue) =>
				!isDbFieldValue.HasValue
					? DbBoundnessType.Unknown			//Strange rare case when IsDBField property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
					: isDbFieldValue.Value
						? DbBoundnessType.DbBound
						: DbBoundnessType.Unbound;

			static DbBoundnessType GetNonDbBoundness(bool? isNonDbValue) =>
				!isNonDbValue.HasValue
					? DbBoundnessType.Unknown          //Strange rare case when IsDBField property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
					: isNonDbValue.Value
						? DbBoundnessType.Unbound
						: DbBoundnessType.DbBound;
		}

		private DbBoundnessType GetBoundTypeFromStandardBoundTypeAttributes(AttributeData attribute)
		{
			if (StandardBoundBaseTypes.Any(boundBaseType => attribute.AttributeClass.IsDerivedFromOrAggregatesAttributeUnsafe(boundBaseType, Context)))
			{
				if (_pxDBLocalizableStringAttribute != null && 
					attribute.AttributeClass.IsDerivedFromOrAggregatesAttributeUnsafe(_pxDBLocalizableStringAttribute, Context))
				{
					return GetBoundTypeFromLocalizableStringDerivedAttribute(attribute);
				}

				return DbBoundnessType.DbBound;
			}

			return DbBoundnessType.Unbound;
		}

		private DbBoundnessType GetBoundTypeFromLocalizableStringDerivedAttribute(AttributeData attribute)
		{
			var (nonDbArgKey, nonDbArgValue) = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == NonDB);

			if (nonDbArgKey == null)
				return DbBoundnessType.DbBound;
			else if (nonDbArgValue.Value is bool nonDbArgValueBool)
			{
				return nonDbArgValueBool
					? DbBoundnessType.Unbound
					: DbBoundnessType.DbBound;
			}
			else
				return DbBoundnessType.Unknown;
		}

		/// <summary>
		/// Duck typing check if attribute Has mixed database boundness. 
		/// The check will look for a presence of IsDBField properties on the flattened Acumatica attributes set of the checked attribute.<br/>
		/// If there is a suitable property - return <see cref="DbBoundnessType.Unknown"/> since we can only spot the known pattern but can't deduce
		/// attribute's DB boundness. 
		/// </summary>
		/// <param name="flattenedAttributes">
		/// Flattened Acumatica attributes of the checked attribute which includes aggregated attributes, aggregates on aggregates and their base types.
		/// </param>
		/// <returns>
		/// DBBoundness deduced by the duck typing.
		/// </returns>
		private DbBoundnessType DuckTypingCheckIfAttributeHasMixedDbBoundness(IReadOnlyCollection<ITypeSymbol> flattenedAttributes)
		{
			//only IsDBField properties are considered in analysis for attributes that can be applied to both bound and unbound fields
			foreach (var attributeType in flattenedAttributes)
			{
				var isDbFieldMembers = attributeType.GetMembers(IsDBField);

				if (isDbFieldMembers.IsDefaultOrEmpty)
					continue;

				foreach (var property in isDbFieldMembers.OfType<IPropertySymbol>())
				{
					if (!property.IsStatic && property.IsExplicitlyDeclared() && property.IsDeclaredInType(attributeType))
						return DbBoundnessType.Unknown;
				}
			}

			return DbBoundnessType.NotDefined;
		}
	}
}