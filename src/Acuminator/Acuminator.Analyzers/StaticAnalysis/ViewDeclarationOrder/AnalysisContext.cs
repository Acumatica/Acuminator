using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
		private class AnalysisContext
		{
			private readonly SymbolAnalysisContext _symbolContext;

			public PXGraphSemanticModel GraphSemanticModel { get; }

			public Dictionary<ITypeSymbol, AnalysisDacInfo> AnalysisPassInfoByDacType { get; }

			public List<DataViewInfo> ViewsInBaseGraphs { get; }

			public List<DataViewInfo> ViewsInGraphNotMarkedOnForwardPass { get; }

			public AnalysisContext(SymbolAnalysisContext symbolContext, PXGraphSemanticModel graphSemanticModel)
			{
				_symbolContext = symbolContext;
				GraphSemanticModel = graphSemanticModel;
				ViewsInBaseGraphs = new List<DataViewInfo>(capacity: graphSemanticModel.Views.Length / 2);
				ViewsInGraphNotMarkedOnForwardPass = new List<DataViewInfo>(capacity: graphSemanticModel.Views.Length / 2);
				AnalysisPassInfoByDacType = new Dictionary<ITypeSymbol, AnalysisDacInfo>(capacity: graphSemanticModel.Views.Length);
			}

			public IEnumerable<DataViewInfo> GetViewsToAnalyze()
			{
				foreach (DataViewInfo view in GraphSemanticModel.Views)
				{
					if (view.Type.TypeArguments.IsEmpty || view.Symbol.Locations.IsEmpty)
						continue;

					int countOfDACsInHierarchy = CountOfDACsInTypeHierarchy(view);  

					if (countOfDACsInHierarchy == 1 || countOfDACsInHierarchy == 2)  //Exclude rare corner case when there is a view for a deeply derived DAC (more than 2 DACs in hierarchy)
					{
						yield return view;
					}
				}

				//---------------------------------Local functions-----------------------------------------------------------------------
				int CountOfDACsInTypeHierarchy(DataViewInfo view)
				{
					var baseTypes = view.ViewDac?.GetBaseTypesAndThis();

					if (baseTypes.IsNullOrEmpty())
						return 0;

					int countOfDACsInHierarchy = 0;

					foreach (ITypeSymbol type in baseTypes)
					{
						if (!type.IsDAC())
							break;

						countOfDACsInHierarchy++;
					}

					return countOfDACsInHierarchy;
				}
			}

			public IEnumerable<INamedTypeSymbol> GetVisitedBaseDacs(ITypeSymbol viewDacType) =>
				viewDacType.GetBaseTypes()
						   .Where(baseDac => AnalysisPassInfoByDacType.ContainsKey(baseDac));
			

			public void ReportDiagnostic(Diagnostic diagnostic) => _symbolContext.ReportDiagnostic(diagnostic);

			public void ReportDiagnosticForBaseDACs(IEnumerable<ITypeSymbol> baseDacs, ITypeSymbol viewDac, DiagnosticDescriptor descriptor, 
													Location location)
			{
				foreach (var dac in baseDacs)
				{
					_symbolContext.ReportDiagnostic(
						Diagnostic.Create(descriptor, location, viewDac.Name, dac.Name));
				}
			}
		}
	}
}