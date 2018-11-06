using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities;

namespace Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder
{
	/// <summary>
	/// An analyzer for the order of view declaration in graph/graph extension.
	/// </summary>
	public class ViewDeclarationOrderAnalyzer : IPXGraphAnalyzer
	{
		private const string InitCacheMappingMethodName = "InitCacheMapping";

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1004_ViewDeclarationOrder, Descriptors.PX1006_ViewDeclarationOrder);	

		public void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, CodeAnalysisSettings settings, 
							PXGraphSemanticModel graphSemanticModel)
		{
			if (graphSemanticModel.ViewsByNames.Count == 0 || IsNewMethodUsedToInitCaches(pxContext))
				return;

			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			RunAnalysisOnGraphViewsToFindTwoCacheCases(pxContext, graphSemanticModel, symbolContext);

			symbolContext.CancellationToken.ThrowIfCancellationRequested();	
		}

		/// <summary>
		/// Starting from the Acumatica 2018R2 version a new method is used to initialize caches with explicit ordering of caches.
		/// </summary>
		/// <returns/>
		private static bool IsNewMethodUsedToInitCaches(PXContext pxContext)
		{
			IMethodSymbol initCachesNewMethod = pxContext.PXGraph.Type
																 .GetMembers(InitCacheMappingMethodName)
																 .OfType<IMethodSymbol>()
																 .FirstOrDefault(method => method.ReturnsVoid && method.Parameters.Length == 1);
			return initCachesNewMethod != null;
		}

		private static void RunAnalysisOnGraphViewsToFindTwoCacheCases(PXContext pxContext, PXGraphSemanticModel graphSemanticModel, 
																	   SymbolAnalysisContext symbolContext)
		{
			var viewsGroupedByDAC = GetViewsToAnalyze(graphSemanticModel)
										.Where(view => view.ViewDAC != null)
										.ToLookup(view => view.ViewDAC);

			if (viewsGroupedByDAC.Count == 0)
				return;

			foreach (IGrouping<ITypeSymbol, DataViewInfo> dacViews in viewsGroupedByDAC)
			{
				ITypeSymbol dac = dacViews.Key;
				int minDacViewDeclarationOrder = dacViews.Min(view => view.DeclarationOrder);
				var baseDACsWithViews = dac.GetBaseTypes()
										   .Where(t => t.IsDAC() && viewsGroupedByDAC.Contains(t));

				foreach (INamedTypeSymbol baseDac in baseDACsWithViews)
				{
					var minBaseDacViewDeclarationOrder = viewsGroupedByDAC[baseDac].Min(view => view.DeclarationOrder);

					if (minBaseDacViewDeclarationOrder > minDacViewDeclarationOrder)
					{

					}
					else
					{

					}
				}
			}
			 






			
		}

		private static bool GraphContainsViewDeclaration(PXGraphSemanticModel graphSemanticModel, DataViewInfo viewInfo) =>
			graphSemanticModel.Symbol.OriginalDefinition?.Equals(viewInfo.Symbol.ContainingType?.OriginalDefinition) ?? false;

		private static IEnumerable<DataViewInfo> GetViewsToAnalyze(PXGraphSemanticModel graphSemanticModel)
		{
			foreach (DataViewInfo view in graphSemanticModel.Views)
			{
				if (view.Type.TypeArguments.IsEmpty || view.Symbol.Locations.IsEmpty)
					continue;

				var baseTypes = view.ViewDAC?.GetBaseTypesAndThis();
				int countOfDACsInHierarchy = baseTypes.IsNullOrEmpty()
												? 0
												: baseTypes.TakeWhile(t => t.IsDAC()).Count();

				if (countOfDACsInHierarchy == 1 || countOfDACsInHierarchy == 2)  //Exclude rare corner case when there is a view for a deeply derived DAC (more than 2 DACs in hierarchy)
				{
					yield return view;
				}
			}
		}	
	}
}