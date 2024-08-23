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
			base.ShouldAnalyze(pxContext, dac) && dac.DacType == DacType.Dac && dac.DacFieldsByNames.Count > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (var dacField in dac.DacFields)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (!dacField.HasBqlField && dacField.HasAcumaticaAttributes)
					ReportDacPropertyWithoutBqlField(symbolContext, pxContext, dac, dacField);
			}
		}

		private void ReportDacPropertyWithoutBqlField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac, 
													  DacFieldInfo dacField)
		{
			Location? location = GetLocationToReport(dac, dacField);

			if (location == null)
				return;

			var properties = ImmutableDictionary<string, string>.Empty;

			if (dacField.PropertyInfo?.IsInSource == true && dacField.EffectivePropertyType?.Name is not null)
			{
				properties = new Dictionary<string, string>
				{
					{ DiagnosticProperty.RegisterCodeFix, bool.TrueString },
					{ DiagnosticProperty.DacFieldName,	  dacField.Name },
					{ DiagnosticProperty.PropertyType,	  dacField.EffectivePropertyType.Name }
				}
				.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
			}

			var diagnostic = Diagnostic.Create(Descriptors.PX1065_NoBqlFieldForDacFieldProperty, location, properties, dacField.Name);
			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private Location? GetLocationToReport(DacSemanticModel dac, DacFieldInfo dacField)
		{
			if (dacField.PropertyInfo?.IsInSource == true)
			{
				return dacField.PropertyInfo.Node.Identifier.GetLocation() ??
					   dacField.PropertyInfo.Node.GetLocation() ??
					   dac.Node!.Identifier.GetLocation();
			}

			// Node is not null because aggregated DAC analysis runs only on DACs from the source code
			return dac.Node!.Identifier.GetLocation();
		}
	}
}