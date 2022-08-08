#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	public partial class LongOperationDelegateClosuresAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1008_LongOperationDelegateClosures);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type != GraphType.None &&
			!graph.Symbol.DeclaringSyntaxReferences.IsDefaultOrEmpty;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graph)
		{
			var longOperationsChecker = new LongOperationsChecker(context, pxContext, graph);

			foreach (SyntaxReference graphSyntaxReference in graph.Symbol.DeclaringSyntaxReferences)
			{
				var graphNode = graphSyntaxReference.GetSyntax(context.CancellationToken) as ClassDeclarationSyntax;

				if (graphNode != null)
					longOperationsChecker.CheckForCapturedGraphReferencesInDelegateClosures(graphNode);
			}			
		}
	}
}