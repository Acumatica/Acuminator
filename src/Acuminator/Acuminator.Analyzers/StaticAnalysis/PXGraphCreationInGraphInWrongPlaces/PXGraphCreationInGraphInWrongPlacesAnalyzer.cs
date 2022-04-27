#nullable enable

using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Walkers;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces
{
	public class PXGraphCreationInGraphInWrongPlacesAnalyzer : PXGraphAggregatedAnalyzerBase
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

			if (graphOrGraphExtension.Type == GraphType.PXGraphExtension)
			{
				CheckIsActiveMethod(context, pxContext, Descriptors.PX1056_PXGraphCreationInIsActiveMethod, graphOrGraphExtension.IsActiveMethodInfo);
				CheckIsActiveMethod(context, pxContext, Descriptors.PX1056_PXGraphCreationInIsActiveForGraphMethod, 
									graphOrGraphExtension.IsActiveForGraphMethodInfo);
			}
		}

		private void CheckIsActiveMethod(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor,
										 NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>? IsActiveMethodInfo)
		{
			if (IsActiveMethodInfo != null)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var graphIsActiveMethodWalker = new PXGraphCreateInstanceWalker(context, pxContext, descriptor);
				graphIsActiveMethodWalker.Visit(IsActiveMethodInfo.Node);
			}
		}
	}
}
