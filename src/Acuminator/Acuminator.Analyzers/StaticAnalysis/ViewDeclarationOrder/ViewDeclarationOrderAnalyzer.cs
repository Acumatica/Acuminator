﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder
{
	/// <summary>
	/// An analyzer for the order of view declaration in graph/graph extension.
	/// </summary>
	public partial class ViewDeclarationOrderAnalyzer : IPXGraphAnalyzer
	{
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1004_ViewDeclarationOrder, Descriptors.PX1006_ViewDeclarationOrder);

		
		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphSemanticModel)
		{
			if (graphSemanticModel.Views.Length == 0)
				return;

			CheckGraphViewsOnForwardPass(context, graphSemanticModel);
					
			//backward pass
			CheckGraphViews(context, graphSemanticModel, Descriptors.PX1006_ViewDeclarationOrder, isForwardPass: false);
		}

		private static void CheckGraphViewsOnForwardPass(SymbolAnalysisContext symbolContext, PXGraphSemanticModel graphSemanticModel)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			AnalysisContext analysisContext = new AnalysisContext(symbolContext, graphSemanticModel, AnalysisPassDirection.Forward);

			foreach (DataViewInfo viewInfo in analysisContext.GetViewsToAnalyze())
			{
				ITypeSymbol viewDacType = viewInfo.Type.TypeArguments[0];

				if (!viewDacType.IsDAC())
					continue;

				AnalyzeGraphViewOnForwardPass(analysisContext, viewInfo, viewDacType);

				if (analysisContext.AnalysisPassInfoByDacType.TryGetValue(viewDacType, out AnalysisPassInfo analysisPassInfo))
				{
					analysisPassInfo.VisitedViews.Add(viewInfo);
				}
				else
				{
					analysisContext.AnalysisPassInfoByDacType[viewDacType] = new AnalysisPassInfo(viewDacType, viewInfo);
				}
			}
		}

		private static void AnalyzeGraphViewOnForwardPass(AnalysisContext analysisContext, DataViewInfo viewInfo, ITypeSymbol viewDacType)
		{
			if (!GraphContainsViewDeclaration(analysisContext.GraphSemanticModel, viewInfo))
				return;

			Location viewLocation = viewInfo.Symbol.Locations[0];
			var visitedBaseDACs = analysisContext.GetVisitedBaseDacs(viewDacType);

			if (analysisContext.AnalysisPassInfoByDacType.TryGetValue(viewDacType, out var analysisPassInfo))
			{
				if (analysisPassInfo.HasDiagnostic)
				{
					visitedBaseDACs.ForEach(baseDac => analysisContext.ReportDiagnostic(
											Diagnostic.Create(analysisPassInfo.DiagnosticDescriptor, viewLocation, viewDacType.Name, baseDac.Name)));
				}


			}

			
			
			visitedBaseDACs.ForEach(baseDac => analysisContext.ReportDiagnostic(
											Diagnostic.Create(Descriptors.PX1004_ViewDeclarationOrder, viewLocation, viewDacType.Name, baseDac.Name)));
					
		}

		private static void CheckGraphViews(SymbolAnalysisContext context, PXGraphSemanticModel graphSemanticModel, 
											DiagnosticDescriptor diagnosticDescriptor, bool isForwardPass)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var visitedViewDacTypesWithViews = new Dictionary<ITypeSymbol, List<DataViewInfo>>();
			var graphViews = isForwardPass 
				? graphSemanticModel.Views
				: graphSemanticModel.Views.Reverse();
			
			foreach (DataViewInfo viewInfo in graphViews.Where(vInfo => vInfo.Type.TypeArguments.Any() && vInfo.Symbol.Locations.Any()))
			{
				ITypeSymbol viewDacType = viewInfo.Type.TypeArguments[0];

				if (!viewDacType.IsDAC())
					continue;

				CheckGraphView(viewInfo, viewDacType);

				if (visitedViewDacTypesWithViews.TryGetValue(viewDacType, out var visitedDataViews))
				{
					visitedDataViews.Add(viewInfo);
				}
				else
				{
					visitedViewDacTypesWithViews[viewDacType] = new List<DataViewInfo> { viewInfo };
				}
			}

			//---------------------------------------------------Local Functions-----------------------------------------------
			void CheckGraphView(DataViewInfo viewInfo, ITypeSymbol viewDacType)
			{		
				var visitedBaseDACs = viewDacType.GetBaseTypes()
												 .Where(baseDac => visitedViewDacTypesWithViews.ContainsKey(baseDac));

				if (GraphContainsViewDeclaration(graphSemanticModel, viewInfo))
				{
					Location viewLocation = viewInfo.Symbol.Locations[0];
					visitedBaseDACs.ForEach(baseDac => context.ReportDiagnostic(
															Diagnostic.Create(diagnosticDescriptor, viewLocation, viewDacType.Name, baseDac.Name)));
				}
				else
				{
					foreach (INamedTypeSymbol baseDac in visitedBaseDACs)
					{
						visitedViewDacTypesWithViews[baseDac]
							.Where(visitedView => GraphContainsViewDeclaration(graphSemanticModel, visitedView))
							.ForEach(visitedView => context.ReportDiagnostic(
														Diagnostic.Create(diagnosticDescriptor, visitedView.Symbol.Locations[0],
																		  viewDacType.Name, baseDac.Name)));
					}
				}
			}
		} 

		private static bool GraphContainsViewDeclaration(PXGraphSemanticModel graphSemanticModel, DataViewInfo viewInfo) =>
			graphSemanticModel.Symbol.OriginalDefinition?.Equals(viewInfo.Symbol.ContainingType?.OriginalDefinition) ?? false;
	}
}