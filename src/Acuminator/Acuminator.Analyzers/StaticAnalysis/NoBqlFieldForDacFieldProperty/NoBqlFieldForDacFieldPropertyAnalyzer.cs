using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty
{
	public class NoBqlFieldForDacFieldPropertyAnalyzer : DacAggregatedAnalyzerBase
	{
		private const int MaximumStringDistance = 2;

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1065_NoBqlFieldForDacFieldProperty,
				Descriptors.PX1066_TypoInBqlFieldName
			);

		public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dacOrDacExt) => 
			base.ShouldAnalyze(pxContext, dacOrDacExt) && dacOrDacExt.DacFieldsByNames.Count > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			var declaredBqlFieldsWithWithoutProperty = 
				dacOrDacExt.DeclaredDacFields.Where(dacField => !dacField.HasFieldPropertyEffective)
											 .Select(dacField => dacField.BqlFieldInfo!)
											 .ToList();

			foreach (var dacField in dacOrDacExt.DacFields)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (!dacField.HasBqlFieldEffective && dacField.HasAcumaticaAttributes)
					AnalyzeDacPropertyWithoutBqlField(symbolContext, pxContext, dacOrDacExt, dacField, declaredBqlFieldsWithWithoutProperty);
			}
		}

		private void AnalyzeDacPropertyWithoutBqlField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrDacExt,
													   DacFieldInfo dacFieldWithoutBqlField, List<DacBqlFieldInfo> declaredBqlFieldsWithWithoutProperty)
		{
			var bqlFieldsWithTypo = FindBqlFieldInfosWithinTypoDistance(dacFieldWithoutBqlField, dacOrDacExt, declaredBqlFieldsWithWithoutProperty, 
																		symbolContext.CancellationToken);
			if (bqlFieldsWithTypo.Count == 0)
			{
				ReportDacPropertyWithoutBqlField(symbolContext, pxContext, dacOrDacExt, dacFieldWithoutBqlField);
				return;
			}
			else
			{
				foreach (var bqlFieldWithTypo in bqlFieldsWithTypo)
				{
					symbolContext.CancellationToken.ThrowIfCancellationRequested();
					ReportBqlFieldWithTypo(symbolContext, pxContext, bqlFieldWithTypo, dacFieldWithoutBqlField.Name);
				}
			}
		}

		private List<DacBqlFieldInfo> FindBqlFieldInfosWithinTypoDistance(DacFieldInfo dacFieldWithoutBqlField, DacSemanticModel dacOrDacExt,
																		  List<DacBqlFieldInfo> declaredBqlFieldsWithWithoutProperty,
																		  CancellationToken cancellation)
		{
			if (declaredBqlFieldsWithWithoutProperty.Count == 0 || !dacFieldWithoutBqlField.IsDeclaredInType(dacOrDacExt.Symbol))
				return [];

			string propertyName 	 = dacFieldWithoutBqlField.Name.ToLowerInvariant();
			int minDistance			 = int.MaxValue;
			var nearestBqlFieldInfos = new List<DacBqlFieldInfo>(capacity: 4);

			foreach (var bqlField in declaredBqlFieldsWithWithoutProperty)
			{
				cancellation.ThrowIfCancellationRequested();

				string bqlFieldName = bqlField.Name.ToLowerInvariant();
				int distance = StringExtensions.LevenshteinDistance(propertyName, bqlFieldName);

				if (distance > MaximumStringDistance || distance > minDistance)
					continue;

				if (distance < minDistance)
				{
					minDistance = distance;
					nearestBqlFieldInfos.Clear();
				}

				nearestBqlFieldInfos.Add(bqlField);
			}

			return nearestBqlFieldInfos;
		}

		private void ReportBqlFieldWithTypo(SymbolAnalysisContext symbolContext, PXContext pxContext, DacBqlFieldInfo bqlFieldWithTypo, 
											string propertyName)
		{
			Location? location = bqlFieldWithTypo.Node!.Identifier.GetLocation() ?? bqlFieldWithTypo.Node.GetLocation();

			if (location == null)
				return;

			var properties = ImmutableDictionary<string, string?>.Empty
																 .Add(DiagnosticProperty.DacFieldName, propertyName);
			var diagnostic = Diagnostic.Create(Descriptors.PX1066_TypoInBqlFieldName, location, properties, propertyName);

			symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}

		private void ReportDacPropertyWithoutBqlField(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrDacExt, 
													  DacFieldInfo dacFieldWithoutBqlField)
		{
			var (location, registerCodeFix) = GetLocationToReportAndCodeFixRegistration(dacOrDacExt, dacFieldWithoutBqlField);

			if (location == null)
				return;

			var properties = ImmutableDictionary<string, string?>.Empty;
			string? propertyTypeName = dacFieldWithoutBqlField.EffectivePropertyType?.GetSimplifiedName();

			if (registerCodeFix && propertyTypeName != null)
			{
				properties = new Dictionary<string, string?>
				{
					{ DiagnosticProperty.RegisterCodeFix, bool.TrueString },
					{ DiagnosticProperty.DacFieldName,	  dacFieldWithoutBqlField.Name },
					{ DiagnosticProperty.PropertyType,    propertyTypeName }
				}
				.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
			}

			var diagnostic = Diagnostic.Create(Descriptors.PX1065_NoBqlFieldForDacFieldProperty, location, properties, dacFieldWithoutBqlField.Name);
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