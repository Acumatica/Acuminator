﻿#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension
{
    public class InheritanceFromPXCacheExtensionAnalyzer : DacAggregatedAnalyzerBase
	{
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create
			(
				Descriptors.PX1009_InheritanceFromPXCacheExtension,
				Descriptors.PX1011_InheritanceFromPXCacheExtension
			);

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			base.ShouldAnalyze(pxContext, dac) &&
			dac.DacType == DacType.DacExtension && 
			dac.Symbol.Name != TypeNames.PXCacheExtension && !dac.IsMappedCacheExtension;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			if (dac.Symbol.BaseType.Name == TypeNames.PXCacheExtension)
			{
				if (!dac.Symbol.IsSealed)
				{
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1011_InheritanceFromPXCacheExtension, dac.Symbol.Locations.First()),
						pxContext.CodeAnalysisSettings);
				}
			}
			else
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1009_InheritanceFromPXCacheExtension, dac.Symbol.Locations.First()),
					pxContext.CodeAnalysisSettings);
			}
		}
    }
}
