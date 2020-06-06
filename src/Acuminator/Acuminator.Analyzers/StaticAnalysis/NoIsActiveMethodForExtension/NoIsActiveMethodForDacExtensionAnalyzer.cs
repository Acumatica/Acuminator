using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension
{
	/// <summary>
	/// The analyzer which checks that DAC extensions have IsActive method declared.
	/// </summary>
	public class NoIsActiveMethodForDacExtensionAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
            (
                Descriptors.PX1016_NoIsActiveMethodForDacExtension
            );

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			base.ShouldAnalyze(pxContext, dac) && dac.DacType == DacType.DacExtension && dac.IsActiveMethod == null;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			//Should analyze already filtered everything and left only DAC extensions without IsActive
			Location location = dacOrExtension.Node.Identifier.GetLocation();

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1016_NoIsActiveMethodForDacExtension, location), 
				pxContext.CodeAnalysisSettings);
		}
	}
}