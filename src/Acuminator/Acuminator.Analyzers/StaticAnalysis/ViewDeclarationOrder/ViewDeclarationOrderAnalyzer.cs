using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder
{
	/// <summary>
	/// An analyzer for the order of view declaration in graph/graph extension.
	/// </summary>
	public class ViewDeclarationOrderAnalyzer : IPXGraphAnalyzer
	{
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1004_ViewDeclarationOrder, Descriptors.PX1006_ViewDeclarationOrder);

		
		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
		{
			INamedTypeSymbol graph = (INamedTypeSymbol)context.Symbol;

			if (graph == null || context.CancellationToken.IsCancellationRequested ||
			   !graph.InheritsFromOrEquals(pxContext.PXGraphType) && !graph.InheritsFromOrEquals(pxContext.PXGraphExtensionType))
			{
				return;
			}

            graph.GetViewsWithSymbolsFromPXGraph(pxContext);
            var graphViews = graph.GetMembers()
								  .OfType<IFieldSymbol>()
								  .Select(field => field.Type as INamedTypeSymbol)
								  .Where(fieldType => fieldType != null &&
													   fieldType.InheritsFrom(pxContext.PXSelectBaseType) && 
													   fieldType.IsGenericType &&
													   fieldType.TypeArguments.Length > 0).
								   ToImmutableList();

			Location graphLocation = graph.Locations.FirstOrDefault();

			if (graphLocation == null || context.CancellationToken.IsCancellationRequested)
				return;

			//forward pass
			CheckGraphViews(context, graphLocation, graphViews, Descriptors.PX1004_ViewDeclarationOrder);

			if (context.CancellationToken.IsCancellationRequested)
				return;

			//backward pass
			CheckGraphViews(context, graphLocation, graphViews.Reverse(), Descriptors.PX1006_ViewDeclarationOrder);
		}

		private static void CheckGraphViews(SymbolAnalysisContext context, Location graphLocation, ImmutableList<INamedTypeSymbol> graphViews,
											DiagnosticDescriptor diagnosticDescriptor)
		{
			var visitedViews = new HashSet<ITypeSymbol>();

			foreach (INamedTypeSymbol view in graphViews)
			{
				ITypeSymbol viewType = view.TypeArguments[0] as ITypeSymbol;

				if (viewType == null || visitedViews.Contains(viewType))
					continue;

				foreach (INamedTypeSymbol parentType in viewType.GetBaseTypes().Where(pType => visitedViews.Contains(pType)))
				{
					context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, graphLocation, viewType.Name, parentType.Name));
				}

				visitedViews.Add(viewType);
			}
		}
	}
}