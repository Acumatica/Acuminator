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

		public ImmutableArray<ITypeSymbol> StandardBoundBaseTypes { get; }

		public ImmutableArray<MixedDbBoundnessAttributeInfo> AttributesContainingIsDBField { get; }

		public DbBoundnessCalculator(PXContext pxContext)
		{
			Context = pxContext.CheckIfNull(nameof(pxContext));
			StandardBoundBaseTypes = GetStandardBoundBaseTypes(Context).ToImmutableArray();
			AttributesContainingIsDBField = FieldTypeAttributesRegister.GetTypesContainingIsDBField(Context)
											.OrderBy(typeWithValue => typeWithValue.AttributeType, TypeSymbolsByHierachyComparer.Instance)
											.ToImmutableArray();

			_eventSubscriberAttribute = Context.AttributeTypes.PXEventSubscriberAttribute;
			_defaultAttribute = Context.AttributeTypes.PXDefaultAttribute;
			_pxDBLocalizableStringAttribute = Context.FieldAttributes.PXDBLocalizableStringAttribute;
		}

		private static List<ITypeSymbol> GetStandardBoundBaseTypes(PXContext context)
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
		/// Query if Acumatica attribute is bound.
		/// </summary>
		/// <param name="attribute">Data of the attribute.</param>
		/// <returns/>
		public DbBoundnessType GetBoundAttributeType(AttributeData attribute)
		{
			attribute.ThrowOnNull(nameof(attribute));

			if (!attribute.AttributeClass.IsAcumaticaAttribute(Context) || attribute.AttributeClass.InheritsFromOrEquals(_defaultAttribute))
				return DbBoundnessType.NotDefined;

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
							? DbBoundnessType.DbBound 
							: DbBoundnessType.Unbound;
						
				case 1 when isDbPropertyAttributeArgs[0].Value.Value is bool isDbPropertyAttributeArgument:     //Case when IsDBField property is set explicitly with correct bool value
					return isDbPropertyAttributeArgument
						? DbBoundnessType.DbBound
						: DbBoundnessType.Unbound;

				case 1:                        //Strange rare case when IsDBField property is set explicitly with value of type other than bool. In this case we don't know if attribute is bound
				default:                       //Strange case when there are multiple different "IsDBField" properties set in attribute constructor (with different letters register case)
					return DbBoundnessType.Unknown;
			}
		}

		private DbBoundnessType GetBoundTypeFromStandardBoundTypeAttributes(AttributeData attribute)
		{
			if (StandardBoundBaseTypes.Any(boundBaseType => attribute.AttributeClass.IsDerivedFromAttributeUnsafe(boundBaseType, Context)))
			{
				if (_pxDBLocalizableStringAttribute != null && 
					attribute.AttributeClass.IsDerivedFromAttributeUnsafe(_pxDBLocalizableStringAttribute, Context))
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
	}
}