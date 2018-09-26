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

namespace Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder
{
	/// <summary>
	/// An analyzer for the order of view declaration in graph/graph extension.
	/// </summary>
	public class ViewDeclarationOrderAnalyzer : IPXGraphAnalyzer
	{
        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1004_ViewDeclarationOrder, Descriptors.PX1006_ViewDeclarationOrder);

		
		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphSemanticModel)
		{
			if (graphSemanticModel.Views.Length == 0)
				return;
			
			//forward pass
			CheckGraphViews(context, graphSemanticModel.Views, Descriptors.PX1004_ViewDeclarationOrder);
					
			//backward pass
			CheckGraphViews(context, graphSemanticModel.Views.Reverse(), Descriptors.PX1006_ViewDeclarationOrder);
		}

		private static void CheckGraphViews(SymbolAnalysisContext context, ImmutableArray<DataViewInfo> graphViews, 
											DiagnosticDescriptor diagnosticDescriptor)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var visitedViewDacTypes = new HashSet<ITypeSymbol>();

			foreach (DataViewInfo viewInfo in graphViews.Where(vInfo => vInfo.Type.TypeArguments.Any() && vInfo.Symbol.Locations.Any()))
			{
				ITypeSymbol viewDacType = viewInfo.Type.TypeArguments[0];

				if (!viewDacType.IsDAC())
					continue;
	
				Location viewLocation = viewInfo.Symbol.Locations[0];
				viewDacType.GetBaseTypes()
						   .Where(baseDac => visitedViewDacTypes.Contains(baseDac))
						   .ForEach(baseDac => context.ReportDiagnostic(
														Diagnostic.Create(diagnosticDescriptor, viewLocation, viewDacType.Name, baseDac.Name)));
				visitedViewDacTypes.Add(viewDacType);
			}
		}
	}
}