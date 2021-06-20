using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension
{
	public class ConstructorInGraphExtensionAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type == GraphType.PXGraphExtension &&
			!graph.Symbol.InstanceConstructors.IsDefaultOrEmpty && !graph.Symbol.IsGraphExtensionBaseType();

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var constructorLocations = pxGraph.Symbol.InstanceConstructors.Where(constructor => !constructor.IsImplicitlyDeclared)
																		  .SelectMany(constructor => constructor.Locations);
			foreach (Location location in constructorLocations)
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1040_ConstructorInGraphExtension, location), 
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}
