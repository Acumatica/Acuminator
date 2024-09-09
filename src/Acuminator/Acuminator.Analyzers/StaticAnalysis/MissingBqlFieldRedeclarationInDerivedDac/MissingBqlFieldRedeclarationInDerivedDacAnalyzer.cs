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
				Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac
			);

		public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dac) => 
			base.ShouldAnalyze(pxContext, dac) && dac.DacType == DacType.Dac && dac.BqlFieldsByNames.Count > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			foreach (var dacField in dac.DacFields)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (!dacField.HasBqlFieldEffective)		// skip all DACs without BQL field - they have nothing to redeclare
					continue;

				if (dacField.IsDeclaredInType(dac.Symbol))
				{
					// DAC field is declared in DAC, but does not have a BQL field redeclaration in DAC
					if (!dacField.HasBqlFieldDeclared)
						ReportNotRedeclaredBqlField(symbolContext, pxContext, dac, dacField, isDacFieldDeclaredInDac: true);
				}
				else	// DAC field is not declared in DAC, but has BQL field which means that DAC should redeclare it
				{
					ReportNotRedeclaredBqlField(symbolContext, pxContext, dac, dacField, isDacFieldDeclaredInDac: false);
				}
			}
		}

		private void ReportNotRedeclaredBqlField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
												 DacFieldInfo notRedeclaredBqlField, bool isDacFieldDeclaredInDac)
		{
			var dacFieldWithDeclaredBqlField = notRedeclaredBqlField.ThisAndOverridenItems()
																	.FirstOrDefault(dacField => dacField.HasBqlFieldDeclared);
			if (dacFieldWithDeclaredBqlField == null)
				return;

			var location = GetLocationToReport(dac, notRedeclaredBqlField, isDacFieldDeclaredInDac);

			if (location == null)
				return;

			string? bqlFieldTypeName = GetBqlFieldTypeName(notRedeclaredBqlField, dacFieldWithDeclaredBqlField.BqlFieldInfo!);

			var properties = new Dictionary<string, string?>
			{
				{ DiagnosticProperty.DacName,	   dac.Name},
				{ DiagnosticProperty.DacFieldName, dacFieldWithDeclaredBqlField.BqlFieldInfo!.Name },
				{ DiagnosticProperty.BqlFieldType, bqlFieldTypeName}
			}
			.ToImmutableDictionary();

			string nameOfBaseDacDeclaringBqlField = dacFieldWithDeclaredBqlField.DacType.Name;
			var diagnostic = Diagnostic.Create(Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac, location, properties,
											   dac.Name, dacFieldWithDeclaredBqlField.BqlFieldInfo.Name, nameOfBaseDacDeclaringBqlField);
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private Location? GetLocationToReport(DacSemanticModel dac, DacFieldInfo notRedeclaredBqlField, bool isDacFieldDeclaredInDac)
		{
			if (isDacFieldDeclaredInDac && notRedeclaredBqlField.PropertyInfo?.IsInSource == true)
			{
				var location = notRedeclaredBqlField.PropertyInfo.Node.Identifier.GetLocation() ??
							   notRedeclaredBqlField.PropertyInfo.Node.GetLocation() ??
							   dac.Node!.Identifier.GetLocation();							// Node is not null because aggregated DAC analysis runs only on DACs from the source code
				return location;
			}
			else
			{
				// Node is not null because aggregated DAC analysis runs only on DACs from the source code
				return dac.Node!.Identifier.GetLocation();
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
			string? propertyTypeName = notRedeclaredBqlField.EffectivePropertyType?.GetSimplifiedName();

			if (propertyTypeName == null)
				return null;

			var propertyDataTypeName = new DataTypeName(propertyTypeName);
			string? mappedBqlFieldType = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(propertyDataTypeName);

			return mappedBqlFieldType;
		}
	}
}