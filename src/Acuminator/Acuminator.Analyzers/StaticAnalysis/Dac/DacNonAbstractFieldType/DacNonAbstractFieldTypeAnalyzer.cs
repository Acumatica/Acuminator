using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;


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
			foreach (DacFieldInfo dacFieldInfo in dac.DeclaredFields)
			{
				AnalyzeDacField(dacFieldInfo, context, pxContext);
			}
		}

		private static void AnalyzeDacField(DacFieldInfo dacFieldInfo, SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (dacFieldInfo.Symbol.IsAbstract)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1024_DacNonAbstractFieldType, dacFieldInfo.Node.Identifier.GetLocation()),
				pxContext.CodeAnalysisSettings);
		}
	}
}