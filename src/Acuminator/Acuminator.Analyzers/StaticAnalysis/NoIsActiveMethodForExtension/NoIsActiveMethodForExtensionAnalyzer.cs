using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension
{
	/// <summary>
	/// The analyzer which checks that DAC and Graph extensions have IsActive method declared.
	/// </summary>
	public class NoIsActiveMethodForExtensionAnalyzer : IDacAnalyzer, IPXGraphAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1016_NoIsActiveMethodForDacExtension,
								  Descriptors.PX1016_NoIsActiveMethodForGraphExtension);

		public bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dacExtension) =>
			dacExtension?.DacType == DacType.DacExtension && 
			dacExtension.IsActiveMethod == null &&
			!dacExtension.IsMappedCacheExtension && !dacExtension.Symbol.IsAbstract && !dacExtension.Symbol.IsStatic;

		public bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graphExtension) =>
			graphExtension?.Type == GraphType.PXGraphExtension && 
			graphExtension.IsActiveMethod == null &&
			!graphExtension.Symbol.IsAbstract && !graphExtension.Symbol.IsStatic && !graphExtension.Symbol.IsGenericType;

		public void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			//ShouldAnalyze already filtered everything and left only DAC extensions without IsActive
			Location location = dacExtension.Node.Identifier.GetLocation();

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1016_NoIsActiveMethodForDacExtension, location), 
				pxContext.CodeAnalysisSettings);
		}

		public void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphSemanticModel graphExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			//ShouldAnalyze already filtered everything and left only graph extensions without IsActive
			Location location = graphExtension..Identifier.GetLocation();

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1016_NoIsActiveMethodForDacExtension, location),
				pxContext.CodeAnalysisSettings);
		}
	}
}