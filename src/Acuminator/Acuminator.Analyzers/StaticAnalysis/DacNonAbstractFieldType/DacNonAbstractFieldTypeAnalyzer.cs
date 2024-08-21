
using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType
{
	public class DacNonAbstractFieldTypeAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1024_DacNonAbstractFieldType
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (DacBqlFieldInfo dacFieldInfo in dac.DeclaredBqlFields)
			{
				AnalyzeDacField(dacFieldInfo, context, pxContext);
			}
		}

		private static void AnalyzeDacField(DacBqlFieldInfo dacFieldInfo, SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (dacFieldInfo.Symbol.IsAbstract)
				return;

			// Node is not null here because aggregated DAC analysis is executed only for DACs declared in the source code,
			// and this analyzer is executed only for DAC BQL fields declared in the DAC.
			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1024_DacNonAbstractFieldType, dacFieldInfo.Node!.Identifier.GetLocation()),
				pxContext.CodeAnalysisSettings);
		}
	}
}