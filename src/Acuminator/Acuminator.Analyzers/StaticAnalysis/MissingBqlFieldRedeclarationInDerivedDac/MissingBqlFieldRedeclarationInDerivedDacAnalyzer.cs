using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived
{
	public class MissingBqlFieldRedeclarationInDerivedDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_SingleField,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_From_2_To_5_Fields,
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_MoreThan5Fields
			);

		public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dac) => 
			base.ShouldAnalyze(pxContext, dac) && dac.DacType == DacType.Dac && dac.BqlFieldsByNames.Count > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var notRedeclaredBqlFieldInfos = GetNotRedeclaredBqlFieldInfos(symbolContext, pxContext, dac);

			if (notRedeclaredBqlFieldInfos.Count == 0)
				return;

			var groupBqlInfosByDeclaration = notRedeclaredBqlFieldInfos.ToLookup(info => info.IsReportedOnProperty);
			var infosReportedOnProperty = groupBqlInfosByDeclaration[true];
			var infosReportedOnDac = groupBqlInfosByDeclaration[false].ToList();

			foreach (NotRedeclaredBqlFieldInfo notRedeclaredInfoWithProperty in infosReportedOnProperty)
			{
				ReportDiagnosticForSingleField(symbolContext, pxContext, dac, notRedeclaredInfoWithProperty);
			}

			ReportDiagnosticOnDac(symbolContext, pxContext, dac, notRedeclaredBqlFieldInfos);
		}

		private List<NotRedeclaredBqlFieldInfo> GetNotRedeclaredBqlFieldInfos(SymbolAnalysisContext symbolContext, PXContext pxContext, 
																			  DacSemanticModel dac)
		{
			var notRedeclaredBqlFieldInfos = new List<NotRedeclaredBqlFieldInfo>();

			foreach (var dacField in dac.DacFields)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (!dacField.HasBqlFieldEffective)     // skip all DACs without BQL field - they have nothing to redeclare
					continue;

				if (dacField.IsDeclaredInType(dac.Symbol))
				{
					// DAC field is declared in DAC, but does not have a BQL field redeclaration in DAC
					if (!dacField.HasBqlFieldDeclared)
					{
						var notRedeclaredInfo = AnalyzeNotRedeclaredBqlField(symbolContext, pxContext, dac, dacField, isDacFieldDeclaredInDac: true);

						if (notRedeclaredInfo.HasValue)
							notRedeclaredBqlFieldInfos.Add(notRedeclaredInfo.Value);
					}
				}
				else    // DAC field is not declared in DAC, but has BQL field which means that DAC should redeclare it
				{
					var notRedeclaredInfo = AnalyzeNotRedeclaredBqlField(symbolContext, pxContext, dac, dacField, isDacFieldDeclaredInDac: false);

					if (notRedeclaredInfo.HasValue)
						notRedeclaredBqlFieldInfos.Add(notRedeclaredInfo.Value);
				}
			}

			return notRedeclaredBqlFieldInfos;
		}

		private NotRedeclaredBqlFieldInfo? AnalyzeNotRedeclaredBqlField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
																		DacFieldInfo notRedeclaredBqlField, bool isDacFieldDeclaredInDac)
		{
			var dacFieldWithDeclaredBqlField = notRedeclaredBqlField.ThisAndOverridenItems()
																	.FirstOrDefault(dacField => dacField.HasBqlFieldDeclared);
			if (dacFieldWithDeclaredBqlField == null)
				return null;

			var (location, isReportedOnProperty) = GetLocationToReport(dac, notRedeclaredBqlField, isDacFieldDeclaredInDac);

			if (location == null)
				return null;

			string bqlFieldName = dacFieldWithDeclaredBqlField.BqlFieldInfo!.Name;
			string? bqlFieldTypeName = GetBqlFieldTypeName(notRedeclaredBqlField, dacFieldWithDeclaredBqlField.BqlFieldInfo!);
			string nameOfBaseDacDeclaringBqlField = dacFieldWithDeclaredBqlField.DacType.Name;

			return new NotRedeclaredBqlFieldInfo(DacFieldName: notRedeclaredBqlField.Name, nameOfBaseDacDeclaringBqlField, bqlFieldName,
												 bqlFieldTypeName, location, isReportedOnProperty);
		}

		private (Location? Location, bool IsReportedOnProperty) GetLocationToReport(DacSemanticModel dac, DacFieldInfo notRedeclaredBqlField, 
																					bool isDacFieldDeclaredInDac)
		{
			if (isDacFieldDeclaredInDac && notRedeclaredBqlField.PropertyInfo?.IsInSource == true)
			{
				var propertyLocation = notRedeclaredBqlField.PropertyInfo.Node.Identifier.GetLocation().NullIfLocationKindIsNone() ??
									   notRedeclaredBqlField.PropertyInfo.Node.GetLocation();

				return (propertyLocation, IsReportedOnProperty: true);
			}
			else
			{
				// Node is not null because aggregated DAC analysis runs only on DACs from the source code
				return (dac.Node!.Identifier.GetLocation().NullIfLocationKindIsNone(), IsReportedOnProperty: false);
			}
		}

		private string? GetBqlFieldTypeName(DacFieldInfo notRedeclaredBqlField, DacBqlFieldInfo declaredBqlFieldFromBaseDacs)
		{
			if (declaredBqlFieldFromBaseDacs.BqlFieldDataTypeEffective == null)
				return GetBqlFieldTypeNameFromPropertyType(notRedeclaredBqlField);

			string propertyTypeName	   = declaredBqlFieldFromBaseDacs.BqlFieldDataTypeEffective.GetSimplifiedName();

			if (propertyTypeName.IsNullOrWhiteSpace())
				return GetBqlFieldTypeNameFromPropertyType(notRedeclaredBqlField);

			var propertyDataTypeName = new DataTypeName(propertyTypeName);
			string? mappedBqlFieldType = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(propertyDataTypeName);
			mappedBqlFieldType		 ??= GetBqlFieldTypeNameFromPropertyType(notRedeclaredBqlField);

			return mappedBqlFieldType;
		}

		private string? GetBqlFieldTypeNameFromPropertyType(DacFieldInfo notRedeclaredBqlField)
		{
			string? propertyTypeName = notRedeclaredBqlField.PropertyTypeUnwrappedNullable?.GetSimplifiedName();

			if (propertyTypeName == null)
				return null;

			var propertyDataTypeName = new DataTypeName(propertyTypeName);
			string? mappedBqlFieldType = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(propertyDataTypeName);

			return mappedBqlFieldType;
		}

		private void ReportDiagnosticOnDac(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
										   List<NotRedeclaredBqlFieldInfo> notRedeclaredBqlFieldInfosReportedOnDac)
		{
			if (notRedeclaredBqlFieldInfosReportedOnDac.Count == 0)
				return;
			else if (notRedeclaredBqlFieldInfosReportedOnDac.Count == 1)
			{
				ReportDiagnosticForSingleField(symbolContext, pxContext, dac, notRedeclaredBqlFieldInfosReportedOnDac[0]);
				return;
			}

			var location = notRedeclaredBqlFieldInfosReportedOnDac[0].Location;
			string bqlFieldsDataString = GetBqlFieldsDataStringForCodeFix(notRedeclaredBqlFieldInfosReportedOnDac);

			var properties = new Dictionary<string, string?>
			{
				{ DiagnosticProperty.DacName						, dac.Name },
				{ PX1067DiagnosticProperty.BqlFieldsWithBqlTypesData, bqlFieldsDataString },
			}
			.ToImmutableDictionary();

			const int fieldsNumberToCutOff = 5;
			Diagnostic diagnostic;

			if (notRedeclaredBqlFieldInfosReportedOnDac.Count > fieldsNumberToCutOff)
			{
				string fieldNamesToDisplay = notRedeclaredBqlFieldInfosReportedOnDac.Take(fieldsNumberToCutOff)
																					.Select(field => $"\"{field.BqlFieldName}\"")
																					.Join(", ");
				int remainingFieldsCount  = notRedeclaredBqlFieldInfosReportedOnDac.Count - fieldsNumberToCutOff;
				string remainingFieldsArg = remainingFieldsCount == 1
					? Resources.PX1067MoreThan5Fields_RemainderSingleField
					: Resources.PX1067MoreThan5Fields_RemainderMultipleFields;

				diagnostic = Diagnostic.Create(Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_MoreThan5Fields, location,
											   properties, dac.Name, fieldNamesToDisplay, remainingFieldsCount.ToString(), remainingFieldsArg);
			}
			else // from 2 to 5 fields
			{
				string fieldNamesToDisplay = notRedeclaredBqlFieldInfosReportedOnDac.Select(field => $"\"{field.BqlFieldName}\"")
																					.Join(", ");
				diagnostic = Diagnostic.Create(Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_From_2_To_5_Fields, location,
											   properties, dac.Name, fieldNamesToDisplay);
			} 

			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private string GetBqlFieldsDataStringForCodeFix(List<NotRedeclaredBqlFieldInfo> notRedeclaredBqlFieldInfos)
		{
			if (notRedeclaredBqlFieldInfos.Count == 0)
				return string.Empty;

			return notRedeclaredBqlFieldInfos.Select(info => info.GetBqlFieldWithTypeDataString())
											 .Join(Separators.FieldsSeparator);
		}

		private void ReportDiagnosticForSingleField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
													NotRedeclaredBqlFieldInfo notRedeclaredBqlFieldInfo)
		{
			string bqlFieldDataString = notRedeclaredBqlFieldInfo.GetBqlFieldWithTypeDataString();

			var properties = new Dictionary<string, string?>
			{
				{ DiagnosticProperty.DacName						, dac.Name },
				{ PX1067DiagnosticProperty.BqlFieldsWithBqlTypesData, bqlFieldDataString },
			}
			.ToImmutableDictionary();
			var diagnostic = Diagnostic.Create(Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac_SingleField, notRedeclaredBqlFieldInfo.Location, 
											   properties, dac.Name, notRedeclaredBqlFieldInfo.BqlFieldName, 
											   notRedeclaredBqlFieldInfo.NameOfBaseDacDeclaringBqlField);

			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}