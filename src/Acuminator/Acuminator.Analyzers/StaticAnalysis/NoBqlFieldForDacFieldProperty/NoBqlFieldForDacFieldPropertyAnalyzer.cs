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
			
			// Check every DAC field with a property to report DAC fields with missing BQL fields,
			// and at the same time match declared BQL fields with properties against each DAC field with a property for possible typos
			foreach (var dacField in dacOrDacExt.DacFields)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				//Skip DAC fields without properties or without Acumatica attributes
				if (!dacField.HasFieldPropertyEffective || !dacField.HasAcumaticaAttributes)
					continue;

				if (dacField.IsDeclaredInType(dacOrDacExt.Symbol))
				{
					AnalyzeDacPropertyDeclaredInThisType(symbolContext, pxContext, dacOrDacExt, dacField, declaredBqlFieldsWithWithoutProperty);
				}
				else
				{
					AnalyzeDacPropertyDeclaredInBaseTypes(symbolContext, pxContext, dacOrDacExt, dacField, declaredBqlFieldsWithWithoutProperty);
				}
			}
		}

		private void AnalyzeDacPropertyDeclaredInThisType(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrDacExt,
														  DacFieldInfo declaredDacField, List<DacBqlFieldInfo> declaredBqlFieldsWithWithoutProperty)
		{
			// if the declared DAC field has a BQL field declared in the same type then skip it
			if (declaredDacField.HasBqlFieldDeclared)
				return;

			// if the declared DAC field does not have a BQL field then report it with PX1065 as missing BQL field
			if (!declaredDacField.HasBqlFieldEffective)
				ReportDacPropertyWithoutBqlField(symbolContext, pxContext, dacOrDacExt, declaredDacField);

			// Now check declared BQL fields without corresponding properties for ones with names close to the "declaredDacField" name.
			// 
			// Since the DAC field "declaredDacField" is declared in "dacOrDacExt" type we need to support two scenarios:
			// 1. BQL field with a typo is declared in the dacOrDacExt DAC and there is a DAC field property with correct name 
			// also declared in the dacOrDacExt type that does not have a corresponding BQL field.
			// 
			// 2. BQL field with a typo is declared in the dacOrDacExt DAC and there is a DAC field property with correct name
			// also declared in the dacOrDacExt type that has a corresponding BQL field in dacOrDacExt base types.
			// It's OK to have a BQL field in this case, since it is in base types, and checked bql fields are in derived types, 
			// so they would be hiding base BQL field anyway.
			//
			// The second scenario only makes sense for DACs, because DAC extensions can't have derived types, 
			// and it makes little sense to redeclare existing BQL field in the chained DAC extension.
			if (declaredDacField.HasBqlFieldEffective && dacOrDacExt.DacType != DacType.Dac)
				return;

			var declaredBqlFieldsWithTypo = FindBqlFieldInfosWithinTypoDistance(declaredDacField.Name, dacOrDacExt, declaredBqlFieldsWithWithoutProperty,
																		symbolContext.CancellationToken);
			if (declaredBqlFieldsWithTypo.Count > 0)
			{
				foreach (var bqlFieldWithTypo in declaredBqlFieldsWithTypo)
				{
					symbolContext.CancellationToken.ThrowIfCancellationRequested();
					ReportBqlFieldWithTypo(symbolContext, pxContext, bqlFieldWithTypo, declaredDacField.Name);
				}
			}
		}

		private void AnalyzeDacPropertyDeclaredInBaseTypes(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrDacExt,
														   DacFieldInfo dacPropertyInBaseTypes, List<DacBqlFieldInfo> declaredBqlFieldsWithWithoutProperty)
		{
			// If the property is declared in based types and does not have a BQL field then report DAC field with PX1065 as missing BQL field
			if (!dacPropertyInBaseTypes.HasBqlFieldEffective)
				ReportDacPropertyWithoutBqlField(symbolContext, pxContext, dacOrDacExt, dacPropertyInBaseTypes);

			// Now check declared BQL fields without corresponding properties for ones with names close to the "dacPropertyInBaseTypes" name.
			// 
			// Since the DAC field "dacPropertyInBaseTypes" is declared in dacOrDacExt base types we need to support a scenario 
			// when BQL field with a typo is declared in the dacOrDacExt DAC and there is a DAC field with correct name in the base types.
			// It does not matter if that field has a BQL field or not - it is in base types, and checked bql fields are in derived types, 
			// so they would be hiding base BQL field anyway.
			//
			// This scenario only makes sense for DACs, because DAC extensions can't have derived types, 
			// and it makes little sense to redeclare existing BQL field in the chained DAC extension
			if (dacOrDacExt.DacType != DacType.Dac)
				return;
			
			var declaredBqlFieldsWithTypo = FindBqlFieldInfosWithinTypoDistance(dacPropertyInBaseTypes.Name, dacOrDacExt, declaredBqlFieldsWithWithoutProperty,
																				symbolContext.CancellationToken);
			if (declaredBqlFieldsWithTypo.Count > 0)
			{
				foreach (var bqlFieldWithTypo in declaredBqlFieldsWithTypo)
				{
					symbolContext.CancellationToken.ThrowIfCancellationRequested();
					ReportBqlFieldWithTypo(symbolContext, pxContext, bqlFieldWithTypo, dacPropertyInBaseTypes.Name);
				}
			}
		}

		private List<DacBqlFieldInfo> FindBqlFieldInfosWithinTypoDistance(string dacFieldName, DacSemanticModel dacOrDacExt,
																		  List<DacBqlFieldInfo> declaredBqlFieldsCandidates,
																		  CancellationToken cancellation)
		{
			if (declaredBqlFieldsCandidates.Count == 0)
				return [];

			string propertyName 	 = dacFieldName.ToLowerInvariant();
			int minDistance			 = int.MaxValue;
			var nearestBqlFieldInfos = new List<DacBqlFieldInfo>(capacity: 4);

			foreach (var bqlField in declaredBqlFieldsCandidates)
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
			Location? location = bqlFieldWithTypo.Node!.Identifier.GetLocation().NullIfLocationKindIsNone() ?? bqlFieldWithTypo.Node.GetLocation();

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
			string? propertyTypeName = dacFieldWithoutBqlField.PropertyTypeUnwrappedNullable?.GetSimplifiedName();

			if (registerCodeFix && propertyTypeName != null)
			{
				properties = new Dictionary<string, string?>
				{
					{ DiagnosticProperty.RegisterCodeFix, bool.TrueString },
					{ DiagnosticProperty.DacFieldName,	  dacFieldWithoutBqlField.Name },
					{ DiagnosticProperty.PropertyType,	  propertyTypeName }
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
				var location = dacField.PropertyInfo.Node.Identifier.GetLocation().NullIfLocationKindIsNone() ??
							   dacField.PropertyInfo.Node.GetLocation() ??
							   dac.Node!.Identifier.GetLocation().NullIfLocationKindIsNone();

				return (location, RegisterCodeFix: true);
			}

			// Node is not null because aggregated DAC analysis runs only on DACs from the source code
			return (dac.Node!.Identifier.GetLocation().NullIfLocationKindIsNone(), RegisterCodeFix: false);
		}
	}
}