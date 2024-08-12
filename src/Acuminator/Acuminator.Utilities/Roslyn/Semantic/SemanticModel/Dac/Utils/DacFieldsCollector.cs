#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	internal static class DacFieldsCollector
	{
		public static ImmutableDictionary<string, DacFieldInfo> CollectDacFieldsFromDacPropertiesAndBqlFields(
																				INamedTypeSymbol dacOrDacExtension, DacType dacType, PXContext pxContext, 
																				ImmutableDictionary<string, DacBqlFieldInfo> bqlFieldsByNames,
																				ImmutableDictionary<string, DacPropertyInfo> propertiesByNames)
		{
			int estimatedCapacity = Math.Max(bqlFieldsByNames.Count, propertiesByNames.Count);

			if (estimatedCapacity == 0)
				return ImmutableDictionary<string, DacFieldInfo>.Empty;

			var dacFields = new OverridableItemsCollection<DacFieldInfo>(estimatedCapacity);
			var bqlFieldsByTypes  = RegroupInfosByType<DacBqlFieldInfo, INamedTypeSymbol>(bqlFieldsByNames.Values);
			var propertiesByTypes = RegroupInfosByType<DacPropertyInfo, IPropertySymbol>(propertiesByNames.Values);

			var typeHierarchy = dacType == DacType.Dac
				? dacOrDacExtension.GetDacWithBaseTypesThatMayStoreDacProperties(pxContext)
				: dacOrDacExtension.GetDacExtensionWithBaseExtensions(pxContext, SortDirection.Ascending, includeDac: true);

			foreach (var type in typeHierarchy)
			{
				bool hasBqlFields = bqlFieldsByTypes.TryGetValue(type, out var declaredBqlFields);
				bool hasFieldProperties = propertiesByTypes.TryGetValue(type, out var declaredProperties);

				if (!hasBqlFields && !hasFieldProperties)
					continue;

				var declaredDacFieldNames = (hasBqlFields, hasFieldProperties) switch
				{
					(true, false) => declaredBqlFields.Select(fieldsWithSameName => fieldsWithSameName.Key),
					(false, true) => declaredProperties.Select(propertiesWithSameName => propertiesWithSameName.Key),
					_			  => declaredBqlFields.Select(fieldsWithSameName => fieldsWithSameName.Key)
													  .Union(declaredProperties.Select(propertiesWithSameName => propertiesWithSameName.Key),
															  StringComparer.OrdinalIgnoreCase)
				};

				foreach (string dacFieldName in declaredDacFieldNames)
				{
					List<DacBqlFieldInfo>? bqlFieldInfos = null;
					List<DacPropertyInfo>? propertyInfos = null;
					declaredBqlFields?.TryGetValue(dacFieldName, out bqlFieldInfos);
					declaredProperties?.TryGetValue(dacFieldName, out propertyInfos);

					var dacFieldInfo = CreateDacFieldInfo(bqlFieldInfos, propertyInfos);

					if (dacFieldInfo != null)
						dacFields.Add(dacFieldInfo);
				}
			}

			return dacFields.ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);
		}

		private static Dictionary<ITypeSymbol, Dictionary<string, List<TInfo>>> RegroupInfosByType<TInfo, TSymbol>(IEnumerable<TInfo> infos)
		where TInfo : SymbolItem<TSymbol>, IOverridableItem<TInfo>
		where TSymbol : ISymbol
		{
			return infos.SelectMany(info => info.ThisAndOverridenItems())
						.GroupBy(info => info.Symbol.ContainingType as ITypeSymbol)
						.ToDictionary(groupedByType => groupedByType.Key,
									  elementSelector: GroupInfosByName);

			//--------------------------------Local Function---------------------------------------------------------------
			// Infos should be grouped by name with duplicates, allowing multiple infos with the same name.
			// This is required to not break the analyzers in case there is an incorrect code with the code duplication in the code editor.
			static Dictionary<string, List<TInfo>> GroupInfosByName(IEnumerable<TInfo> groupedByType) =>
				groupedByType.GroupBy(info => info.Name, StringComparer.OrdinalIgnoreCase)
							 .ToDictionary(groupedByName => groupedByName.Key,
										   groupedByName => groupedByName.ToList(capacity: 1),
										   StringComparer.OrdinalIgnoreCase);
		}

		private static DacFieldInfo? CreateDacFieldInfo(List<DacBqlFieldInfo>? bqlFieldInfos, List<DacPropertyInfo>? propertyInfos)
		{
			switch (bqlFieldInfos?.Count)
			{
				case 1:
					return CreateDacFieldInfoWhenThereIsOneBqlField(bqlFieldInfos[0], propertyInfos);

				case 0:
				case null:
					var propertyInfo = propertyInfos?.FirstOrDefault();
					return propertyInfo != null
						? new DacFieldInfo(propertyInfo, dacBqlFieldInfo: null)
						: null;

				default:
					return CreateDacFieldInfoWhenThereAreMultipleBqlFieldsWithSameName(bqlFieldInfos, propertyInfos);
			}
		}

		private static DacFieldInfo CreateDacFieldInfoWhenThereIsOneBqlField(DacBqlFieldInfo bqlFieldInfo, List<DacPropertyInfo>? propertyInfos)
		{
			switch (propertyInfos?.Count)
			{
				case 1:
					return new DacFieldInfo(propertyInfos[0], bqlFieldInfo);
				case 0:
				case null:
					return new DacFieldInfo(dacPropertyInfo: null, bqlFieldInfo);
				default:
					// Find suitable property: 
					// - first search a property with attributes and name that case-sensitive equals to BQL field name in Pascal Case,
					// - second - just search a property that case-sensitive equals to BQL field name in Pascal Case,
					// - third - search a property that has some attributes,
					// - finally fallback to the first property.
					string capitalizedBqlFieldName = bqlFieldInfo.Name.ToPascalCase();
					var propertyInfo = propertyInfos.FirstOrDefault(p => !p.Attributes.IsDefaultOrEmpty &&
																		 p.Name.Equals(capitalizedBqlFieldName, StringComparison.Ordinal)) ??
									   propertyInfos.FirstOrDefault(p => p.Name.Equals(capitalizedBqlFieldName, StringComparison.Ordinal)) ??
									   propertyInfos.FirstOrDefault(p => !p.Attributes.IsDefaultOrEmpty) ?? propertyInfos[0];
					return new DacFieldInfo(propertyInfo, bqlFieldInfo);
			}
		}

		private static DacFieldInfo CreateDacFieldInfoWhenThereAreMultipleBqlFieldsWithSameName(List<DacBqlFieldInfo> bqlFieldInfos,
																								List<DacPropertyInfo>? propertyInfos)
		{
			switch (propertyInfos?.Count)
			{
				case 1:
					{
						// First attempt to make case sensitive match of the Bql field with the property name in Camel Case, 
						// then fallback to the first BQL field. 
						string propertyNameInCamelCase = propertyInfos[0].Name.FirstCharToLower();
						var bqlFieldInfo = bqlFieldInfos.FirstOrDefault(f => f.Name.Equals(propertyNameInCamelCase, StringComparison.Ordinal)) ??
										   bqlFieldInfos[0];
						return new DacFieldInfo(propertyInfos[0], bqlFieldInfo);
					}
				case 0:
				case null:
					return new DacFieldInfo(dacPropertyInfo: null, bqlFieldInfos[0]);
				default:
					{
						var suitableFieldPropertyPairs = (from bqlFieldInfo in bqlFieldInfos
														  join propertyInfo in propertyInfos.Where(property => !property.Attributes.IsDefaultOrEmpty)
															 on bqlFieldInfo.Name.ToPascalCase() equals propertyInfo.Name
														  select (BqlField: bqlFieldInfo, FieldProperty: propertyInfo))
														 .ToList();

						if (suitableFieldPropertyPairs.Count == 0)
						{
							suitableFieldPropertyPairs = (from bqlFieldInfo in bqlFieldInfos
														  join propertyInfo in propertyInfos
															 on bqlFieldInfo.Name.ToPascalCase() equals propertyInfo.Name
														  select (BqlField: bqlFieldInfo, FieldProperty: propertyInfo))
														 .ToList();
						}

						if (suitableFieldPropertyPairs.Count == 0)
						{
							var suitablePropertyInfo = propertyInfos.FirstOrDefault(p => !p.Attributes.IsDefaultOrEmpty) ?? propertyInfos[0];
							return new DacFieldInfo(suitablePropertyInfo, bqlFieldInfos[0]);
						}
						else
						{
							var (suitableBqlFieldInfo, suitablePropertyInfo) = suitableFieldPropertyPairs[0];
							return new DacFieldInfo(suitablePropertyInfo, suitableBqlFieldInfo);
						}
					}
			}
		}
	}
}
