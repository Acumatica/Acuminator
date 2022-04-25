#nullable enable

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces
{
	public class PXGraphCreationInGraphSemanticModelAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				Descriptors.PX1056_PXGraphCreationInIsActiveMethod,
				Descriptors.PX1056_PXGraphCreationInIsActiveForGraphMethod,

				Descriptors.PX1057_PXGraphCreationDuringInitialization,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV,

				Descriptors.PX1084_GraphCreationInDataViewDelegate);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphOrGraphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var graphInitializerWalker = new PXGraphCreateInstanceWalker(
				context,
				pxContext,
				pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
					? Descriptors.PX1057_PXGraphCreationDuringInitialization
					: Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV);

			foreach (GraphInitializerInfo initializer in graphOrGraphExtension.Initializers)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				graphInitializerWalker.Visit(initializer.Node);
			}

			var graphViewDelegateWalker = new PXGraphCreateInstanceWalker(context, pxContext,
				Descriptors.PX1084_GraphCreationInDataViewDelegate);

			foreach (DataViewDelegateInfo del in graphOrGraphExtension.ViewDelegates)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				graphViewDelegateWalker.Visit(del.Node);
			}	
		}

		private class PXGraphCreateInstanceWalker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly DiagnosticDescriptor _descriptor;

			public PXGraphCreateInstanceWalker(SymbolAnalysisContext context, PXContext pxContext,
				DiagnosticDescriptor descriptor)
				: base(context.Compilation, context.CancellationToken, pxContext.CodeAnalysisSettings)
			{
				_context = context;
				_pxContext = pxContext;
				_descriptor = descriptor;
			}

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				IMethodSymbol symbol = GetSymbol<IMethodSymbol>(node);

				if (symbol != null && _pxContext.PXGraph.CreateInstance.Contains(symbol.ConstructedFrom))
				{
					ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
				}
				else
				{
					base.VisitMemberAccessExpression(node);
				}
			}
		}
	}
}
