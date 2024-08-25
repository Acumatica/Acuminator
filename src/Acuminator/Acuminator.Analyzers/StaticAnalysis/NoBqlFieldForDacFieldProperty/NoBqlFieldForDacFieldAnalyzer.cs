using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty
{
	public class NoBqlFieldForDacFieldAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1065_NoBqlFieldForDacFieldProperty
			);

		public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dac) => 
			base.ShouldAnalyze(pxContext, dac) && dac.DacFieldsByNames.Count > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (var dacField in dac.DacFields)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (!dacField.HasBqlField && dacField.HasAcumaticaAttributes)
					ReportDacPropertyWithoutBqlField(symbolContext, pxContext, dac, dacField);
			}
		}

		private void ReportDacPropertyWithoutBqlField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrDacExt, 
													  DacFieldInfo dacField)
		{
			var (location, registerCodeFix) = GetLocationToReportAndCodeFixRegistration(dacOrDacExt, dacField);

			if (location == null)
				return;

			var properties = ImmutableDictionary<string, string?>.Empty;
			string? propertyTypeName = dacField.EffectivePropertyType?.GetSimplifiedName();

			if (registerCodeFix && propertyTypeName != null)
			{
				properties = new Dictionary<string, string?>
				{
					{ DiagnosticProperty.RegisterCodeFix, bool.TrueString },
					{ DiagnosticProperty.DacFieldName,	  dacField.Name },
					{ DiagnosticProperty.PropertyType,    propertyTypeName }
				}
				.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
			}

			var diagnostic = Diagnostic.Create(Descriptors.PX1065_NoBqlFieldForDacFieldProperty, location, properties, dacField.Name);
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private (Location? Location, bool RegisterCodeFix) GetLocationToReportAndCodeFixRegistration(DacSemanticModel dac, DacFieldInfo dacField)
		{
			if (dacField.PropertyInfo?.IsInSource == true && dacField.IsDeclaredInType(dac.Symbol))
			{
				var location = dacField.PropertyInfo.Node.Identifier.GetLocation() ??
							   dacField.PropertyInfo.Node.GetLocation() ??
							   dac.Node!.Identifier.GetLocation();

				return (location, RegisterCodeFix: true);
			}

			// Node is not null because aggregated DAC analysis runs only on DACs from the source code
			return (dac.Node!.Identifier.GetLocation(), RegisterCodeFix: false);
		}
	}
}