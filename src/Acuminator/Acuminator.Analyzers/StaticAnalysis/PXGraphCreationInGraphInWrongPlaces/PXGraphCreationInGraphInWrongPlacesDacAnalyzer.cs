#nullable enable

using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Walkers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces
{
	public class PXGraphCreationInGraphInWrongPlacesDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod);

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dacExtension) => 
			base.ShouldAnalyze(pxContext, dacExtension) &&
			dacExtension.DacType == DacType.DacExtension &&
			dacExtension.IsActiveMethodInfo != null;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var graphIsActiveMethodWalker = new PXGraphCreateInstanceWalker(context, pxContext, Descriptors.PX1056_PXGraphCreationInIsActiveMethod);
			graphIsActiveMethodWalker.Visit(dacExtension.IsActiveMethodInfo.Node);
		}
	}
}
