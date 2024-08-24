﻿
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder
{
	/// <summary>
	/// An analyzer for the order of view declaration in graph/graph extension.
	/// This diagnostic works only for simple class hierarchy where the depth of the inheritance is equal to 2: object -> DAC -> DerivedDAC.
	/// </summary>
	public class ViewDeclarationOrderAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1004_ViewDeclarationOrder, Descriptors.PX1006_ViewDeclarationOrder);

		/// <summary>
		/// Starting from the Acumatica 2018R2 version a new method is used to initialize caches with explicit ordering of caches.
		/// </summary>
		/// <returns/>
		public override bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && pxContext.PXGraph.InitCacheMapping == null && 
			graph.ViewsByNames.Count > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel graphSemanticModel)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var viewsGroupedByDAC = GetViewsUsedInAnalysis(graphSemanticModel).Where(view => view.DAC != null)
																			  .ToLookup(view => view.DAC!);
			if (viewsGroupedByDAC.Count == 0)
				return;

			foreach (IGrouping<ITypeSymbol, DataViewInfo> dacViews in viewsGroupedByDAC)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				AnalyzeDacViewsForNumberOfCaches(graphSemanticModel, symbolContext, pxContext, dacViews, viewsGroupedByDAC);
			}
		}

		private static IEnumerable<DataViewInfo> GetViewsUsedInAnalysis(PXGraphEventSemanticModel graphSemanticModel)
		{
			foreach (DataViewInfo view in graphSemanticModel.Views)
			{
				if (view.Type.TypeArguments.IsEmpty || view.Symbol.Locations.IsEmpty)
					continue;

				var baseTypes = view.DAC?.GetBaseTypesAndThis();
				int countOfDACsInHierarchy = baseTypes.IsNullOrEmpty()
												? 0
												: baseTypes.TakeWhile(t => t.IsDAC()).Count();

				if (countOfDACsInHierarchy == 1 || countOfDACsInHierarchy == 2)  //Exclude rare corner case when there is a view for a deeply derived DAC (more than 2 DACs in hierarchy)
				{
					yield return view;
				}
			}
		}

		private static void AnalyzeDacViewsForNumberOfCaches(PXGraphEventSemanticModel graphSemanticModel, SymbolAnalysisContext symbolContext,
															 PXContext pxContext, IGrouping<ITypeSymbol, DataViewInfo> dacViews,
															 ILookup<ITypeSymbol, DataViewInfo> viewsGroupedByDAC)
		{
			var dacViewsDeclaredInGraph = dacViews.Where(view => GraphContainsViewDeclaration(graphSemanticModel, view));
			ITypeSymbol dac = dacViews.Key;
			int dacViewDeclarationOrder = dacViews.Min(view => view.DeclarationOrder);
			var baseDacs = dac.GetBaseTypes()
							  .Where(t => t.IsDAC() && viewsGroupedByDAC.Contains(t))
							  .ToList();

			if (baseDacs.Count != 1)
				return;

			ITypeSymbol baseDac = baseDacs[0];
			int baseDacViewOrder = viewsGroupedByDAC[baseDac].Min(baseDacView => baseDacView.DeclarationOrder);

			DiagnosticDescriptor descriptor = dacViewDeclarationOrder > baseDacViewOrder  
					? Descriptors.PX1004_ViewDeclarationOrder                                //the first declared DAC view goes after the first declared base DAC view and two caches will be created
					: Descriptors.PX1006_ViewDeclarationOrder;                               //the first declared DAC view goes before the first declared base DAC view and one cache will be created

			var baseDacViewsDeclaredInGraph = viewsGroupedByDAC[baseDac].Where(view => GraphContainsViewDeclaration(graphSemanticModel, view));
			var viewsToShowDiagnostic = dacViewsDeclaredInGraph.Concat(baseDacViewsDeclaredInGraph);

			ReportDiagnostic(descriptor, symbolContext, pxContext, viewsToShowDiagnostic, dac, baseDac);
		}	

		private static bool GraphContainsViewDeclaration(PXGraphEventSemanticModel graphSemanticModel, DataViewInfo viewInfo) =>
			graphSemanticModel.Symbol.OriginalDefinition?.Equals(viewInfo.Symbol.ContainingType?.OriginalDefinition) ?? false;

		private static void ReportDiagnostic(DiagnosticDescriptor descriptor, SymbolAnalysisContext symbolContext, PXContext pxContext,
											 IEnumerable<DataViewInfo> viewsToShowDiagnostic, ITypeSymbol dac, ITypeSymbol baseDac)
		{
			foreach (DataViewInfo view in viewsToShowDiagnostic)
			{
				Location? viewLocation = view.Symbol.Locations.FirstOrDefault();

				if (viewLocation == null)
					continue;

				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(descriptor, viewLocation, dac.Name, baseDac.Name), pxContext.CodeAnalysisSettings);
			}		
		}
	}
}