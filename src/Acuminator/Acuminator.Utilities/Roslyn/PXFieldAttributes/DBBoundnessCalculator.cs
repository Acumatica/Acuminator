#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
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

		public PXContext Context { get; }

		public FieldTypeAttributesMetadataProvider AttributesMetadataProvider { get; }

		public DbBoundnessCalculator(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull();
			AttributesMetadataProvider = new FieldTypeAttributesMetadataProvider(pxContext);
		}

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <returns>
		/// The attribute application DB boundness type.
		/// </returns>
		public DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication) =>
			GetAttributeApplicationDbBoundnessType(attributeApplication, preparedFlattenedAttributesWithApplications: null, 
												   preparedFlattenedAttributesSet: null, preparedAttributesMetadata: null);

		/// <summary>
		/// Get DB Boundness type of Acumatica attribute's application to a DAC field.
		/// </summary>
		/// <param name="attributeApplication">Attribute application data.</param>
		/// <param name="preparedFlattenedAttributesWithApplications">
		/// The optional already prepared flattened attributes with applications,<br/>
		/// the result of the <see cref="AcumaticaAttributesRelationsInfoProvider.GetThisAndAllAggregatedAttributesWithApplications(AttributeData?, PXContext, bool)"/> call.<br/>
		/// If <see langword="null"/> then the flattened attributes with applications set will be calculated.
		/// </param>
		/// <param name="preparedFlattenedAttributesSet">The prepared flattened attributes set. If <see langword="null"/> then the flattened attributes set will be calculated.</param>
		/// <param name="preparedAttributesMetadata">
		/// The prepared attribute aggregated metadata, the result of the <see cref="FieldTypeAttributesMetadataProvider.GetDacFieldTypeAttributeInfos(ITypeSymbol)"/>
		/// call. If <see langword="null"/> then the aggregated metadata will be calculated.
		/// </param>
		/// <returns>
		/// The attribute application DB boundness type.
		/// </returns>
		internal DbBoundnessType GetAttributeApplicationDbBoundnessType(AttributeData attributeApplication, 
																		ImmutableHashSet<AttributeWithApplication>? preparedFlattenedAttributesWithApplications,
																		ImmutableHashSet<ITypeSymbol>? preparedFlattenedAttributesSet,
																		IReadOnlyCollection<DataTypeAttributeInfo>? preparedAttributesMetadata)
		{
			attributeApplication.ThrowOnNull();

			if (attributeApplication.AttributeClass == null || !attributeApplication.AttributeClass.IsAcumaticaAttribute(Context))
				return DbBoundnessType.NotDefined;

			// First, check if the attribute is present in the set of known non-data-type attributes or is derived from them
			if (AttributesMetadataProvider.IsWellKnownNonDataTypeAttribute(attributeApplication.AttributeClass))
				return DbBoundnessType.NotDefined;

			var flattenedAttributesWithApplications = preparedFlattenedAttributesWithApplications ??
													  attributeApplication.GetThisAndAllAggregatedAttributesWithApplications(Context, includeBaseTypes: true);

			if (flattenedAttributesWithApplications.Count == 0)
				return GetDbBoundnessSetExplicitlyByAttributeApplication(attributeApplication);

			// Check combined information from attribute applications and metadata
			var flattenedAttributesSet = preparedFlattenedAttributesSet ?? 
										 flattenedAttributesWithApplications.Select(atrWithApp => atrWithApp.Type)
																			.ToImmutableHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
			var attributesMetadata = 
				preparedAttributesMetadata ?? 
				AttributesMetadataProvider.GetDacFieldTypeAttributeInfos_NoWellKnownNonDataTypeAttributesCheck(attributeApplication.AttributeClass,
																											   flattenedAttributesSet);
			DbBoundnessType combinedDbBoundness = GetDbBoundnessFromAttributesApplicationsAndMetadata(flattenedAttributesWithApplications, attributesMetadata);

			if (combinedDbBoundness != DbBoundnessType.NotDefined)
				return combinedDbBoundness;

			return DuckTypingCheckIfAttributeHasMixedDbBoundness(flattenedAttributesSet);
		}

		private DbBoundnessType GetDbBoundnessFromAttributesApplicationsAndMetadata(ImmutableHashSet<AttributeWithApplication> flattenedAttributesWithApplications,
																					IReadOnlyCollection<DataTypeAttributeInfo> attributesMetadata)
		{
			if (attributesMetadata.Count == 0)
			{
				return flattenedAttributesWithApplications
							.Select(attrWithApplication => GetDbBoundnessSetExplicitlyByAttributeApplication(attrWithApplication.Application))
							.Combine();
			}

			var applicationsByAttribute = flattenedAttributesWithApplications
											.ToLookup(attrAppl => attrAppl.Type,
													  SymbolEqualityComparer.Default as IEqualityComparer<ITypeSymbol>);
			var combinedBoundness =
				attributesMetadata.Select(attributeInfo => GetDbBoundnessFromMetadataAndApplications(attributeInfo, applicationsByAttribute))
								  .Combine();
			return combinedBoundness;
		}

		private DbBoundnessType GetDbBoundnessFromMetadataAndApplications(DataTypeAttributeInfo attributeInfo, 
																		  ILookup<ITypeSymbol, AttributeWithApplication> applicationsByAttribute)
		{
			// Even if the DB boundness is set explicitly at the application we have to calculate DB boundness from metadata
			// Because it can be inconsistent in which case explicitly set boundness can't apply
			DbBoundnessType dbBoundnessFromMetadata = GetDbBoundnessFromAttributeInfo(attributeInfo);

			if (dbBoundnessFromMetadata == DbBoundnessType.Error)
				return dbBoundnessFromMetadata;

			// For non-mixed DB boundness attributes there is nothing more to apply
			if (attributeInfo is not MixedDbBoundnessAttributeInfo mixedDbBoundnessAttributeInfo || 
				!applicationsByAttribute.Contains(mixedDbBoundnessAttributeInfo.AttributeType))
			{
				return dbBoundnessFromMetadata;
			}

			var attributeApplications = applicationsByAttribute[mixedDbBoundnessAttributeInfo.AttributeType];
			var explicitDbBoundnessFromApplications = 
				attributeApplications.Select(attrWithAppl => GetDbBoundnessSetExplicitlyByAttributeApplication(attrWithAppl.Application)).Combine();

			var combinedBoundness = CombineExplictlySetAndMetadataBoundnesses(explicitDbBoundnessFromApplications, dbBoundnessFromMetadata);
			return combinedBoundness;
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
			bool isNonDbPresent = isNonDbCounter == 1;

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
					? DbBoundnessType.Unknown           //Strange rare case when IsDBField property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
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

		private DbBoundnessType CombineExplictlySetAndMetadataBoundnesses(DbBoundnessType explicitDbBoundnessFromApplications, 
																		  DbBoundnessType dbBoundnessFromMetadata)
		{
			// Custom combination rules
			switch (explicitDbBoundnessFromApplications)
			{
				case DbBoundnessType.Unbound:
					return dbBoundnessFromMetadata switch
					{
						// normal boundness resolution
						DbBoundnessType.PXDBCalced or DbBoundnessType.PXDBScalar
						or DbBoundnessType.Error or DbBoundnessType.NotDefined	 => dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplications),
						// explicit boundndess takes priority
						DbBoundnessType.Unbound or DbBoundnessType.DbBound or 
						DbBoundnessType.Unknown									 => explicitDbBoundnessFromApplications,
						_														 => dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplications)
					};
					
				case DbBoundnessType.DbBound:
					return dbBoundnessFromMetadata is DbBoundnessType.PXDBScalar or DbBoundnessType.PXDBCalced or DbBoundnessType.Error
						? dbBoundnessFromMetadata.Combine(explicitDbBoundnessFromApplications)
						: explicitDbBoundnessFromApplications;
					
				case DbBoundnessType.Unknown:
				case DbBoundnessType.Error:
					return explicitDbBoundnessFromApplications;

				case DbBoundnessType.NotDefined:
				default:
					return dbBoundnessFromMetadata;
			}
		}

		/// <summary>
		/// Duck typing check if attribute Has mixed database boundness. 
		/// The check will look for a presence of IsDBField properties on the flattened Acumatica attributes set of the checked attribute.<br/>
		/// If there is a suitable property - return <see cref="DbBoundnessType.Unknown"/> since we can only spot the known pattern but can't deduce
		/// attribute's DB boundness. 
		/// </summary>
		/// <param name="flattenedAttributesSet">
		/// Flattened Acumatica attributes of the checked attribute which includes aggregated attributes, aggregates on aggregates and their base types.
		/// </param>
		/// <returns>
		/// DBBoundness deduced by the duck typing.
		/// </returns>
		private DbBoundnessType DuckTypingCheckIfAttributeHasMixedDbBoundness(IReadOnlyCollection<ITypeSymbol> flattenedAttributesSet)
		{
			//only IsDBField properties are considered in analysis for attributes that can be applied to both bound and unbound fields
			foreach (var attributeType in flattenedAttributesSet)
			{
				var members = attributeType.GetMembers();

				if (members.IsDefaultOrEmpty)
					continue;

				var hasIsDbFieldProperties = members.OfType<IPropertySymbol>()
													.Any(property => !property.IsStatic && property.IsExplicitlyDeclared() && 
																	  property.IsDeclaredInType(attributeType) &&
																	  IsDBField.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
				if (hasIsDbFieldProperties)
					return DbBoundnessType.Unknown;
			}

			return DbBoundnessType.NotDefined;
		}
	}
}