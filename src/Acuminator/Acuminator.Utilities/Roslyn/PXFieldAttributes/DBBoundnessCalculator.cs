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

		private readonly INamedTypeSymbol _defaultAttribute;

		public PXContext Context { get; }

		public FieldTypeAttributesMetadataProvider AttributesMetadataProvider { get; }

		public DbBoundnessCalculator(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull(nameof(pxContext));
			AttributesMetadataProvider = new FieldTypeAttributesMetadataProvider(pxContext);
					
			_defaultAttribute = Context.AttributeTypes.PXDefaultAttribute;
		}

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <returns>
		/// The attribute application DB boundness type.
		/// </returns>
		public DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication) =>
			GetAttributeApplicationDbBoundnessType(attributeApplication, preparedFlattenedAttributes: null, preparedAttributesMetadata: null);

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <param name="preparedFlattenedAttributes">
		/// The optional already prepared flattened attributes, the result of the <see cref="AcumaticaAttributesRelationsInfoProvider.GetThisAndAllAggregatedAttributes"/> call.<br/>
		/// If <see langword="null"/> then the flattened attributes set will be calculated.
		/// </param>
		/// <param name="preparedAttributesMetadata">
		/// The prepared attribute aggregated metadata, the result of the <see cref="FieldTypeAttributesMetadataProvider.GetDacFieldTypeAttributeInfos(ITypeSymbol)"/> call.
		/// If <see langword="null"/> then the aggregated metadata will be calculated.
		/// </param>
		/// <returns>
		/// The attribute application DB boundness type.
		/// </returns>
		internal DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication, ImmutableHashSet<ITypeSymbol>? preparedFlattenedAttributes,
																		IReadOnlyCollection<DataTypeAttributeInfo>? preparedAttributesMetadata)
		{
			attributeApplication.ThrowOnNull(nameof(attributeApplication));

			if (!attributeApplication.AttributeClass.IsAcumaticaAttribute(Context) || attributeApplication.AttributeClass.InheritsFromOrEquals(_defaultAttribute))
				return DbBoundnessType.NotDefined;

			//First check if DB boundness is set explicitly on attribute application. In that case it will override all attribute metadata
			DbBoundnessType explicitlySetDbBoundness = GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication);

			if (explicitlySetDbBoundness != DbBoundnessType.NotDefined)
				return explicitlySetDbBoundness;

			// If the explicit DB boundness is not set we can query attribute's register for the metadata
			var flattenedAttributes = preparedFlattenedAttributes ?? attributeApplication.AttributeClass.GetThisAndAllAggregatedAttributes(Context, includeBaseTypes: true);
			var attributesMetadata = preparedAttributesMetadata ?? AttributesMetadataProvider.GetDacFieldTypeAttributeInfos(flattenedAttributes);

			DbBoundnessType dbBoundnessFromMetadata = GetDbBoundnessFromAttributesMetadata(attributesMetadata);

			if (dbBoundnessFromMetadata != DbBoundnessType.NotDefined)
				return explicitlySetDbBoundness;

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

		private DbBoundnessType GetDbBoundnessFromAttributesMetadata(IReadOnlyCollection<DataTypeAttributeInfo> attributesMetadata)
		{
			if (attributesMetadata.Count == 0)
				return DbBoundnessType.NotDefined;

			DbBoundnessType dbBoundnessFromAllMetadata = DbBoundnessType.NotDefined;
			

			foreach (var attributeInfo in attributesMetadata)
			{
				var attributeInfoDbBoundness = GetDbBoundnessFromAttributeInfo(attributeInfo);
				dbBoundnessFromAllMetadata = dbBoundnessFromAllMetadata.Combine(attributeInfoDbBoundness);
			}

			return dbBoundnessFromAllMetadata;
		}

		private DbBoundnessType GetDbBoundnessFromAttributeInfo(DataTypeAttributeInfo attributeInfo)
		{
			switch (attributeInfo.Kind)
			{
				case FieldTypeAttributeKind.BoundTypeAttribute:
					return DbBoundnessType.DbBound;
				case FieldTypeAttributeKind.UnboundTypeAttribute:
					return DbBoundnessType.Unbound;
				case FieldTypeAttributeKind.MixedDbBoundnessTypeAttribute:
					if (attributeInfo is not MixedDbBoundnessAttributeInfo mixedDbBoundnessAttributeInfo ||
						!mixedDbBoundnessAttributeInfo.IsDbBoundByDefault.HasValue)
					{
						return DbBoundnessType.Unknown;
					}

					return mixedDbBoundnessAttributeInfo.IsDbBoundByDefault.Value
						? DbBoundnessType.DbBound
						: DbBoundnessType.Unbound;

				case FieldTypeAttributeKind.PXDBScalarAttribute:
					return DbBoundnessType.PXDBScalar;
				case FieldTypeAttributeKind.PXDBCalcedAttribute:
					return DbBoundnessType.PXDBCalced;
				default:
					return DbBoundnessType.NotDefined;
			}
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