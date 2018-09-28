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
		private enum AnalysisPassDirection 
		{
			Forward,
			Backward
		}

		private class AnalysisContext
		{
			private readonly SymbolAnalysisContext _symbolContext;

			public CancellationToken CancellationToken => _symbolContext.CancellationToken;

			public PXGraphSemanticModel GraphSemanticModel { get; }

			public Dictionary<ITypeSymbol, AnalysisPassInfo> AnalysisPassInfoByDacType { get; }

			public AnalysisPassDirection PassDirection { get; }

			public AnalysisContext(SymbolAnalysisContext symbolContext, PXGraphSemanticModel graphSemanticModel, AnalysisPassDirection passDirection)
			{
				_symbolContext = symbolContext;
				GraphSemanticModel = graphSemanticModel;
				PassDirection = passDirection;
				AnalysisPassInfoByDacType = new Dictionary<ITypeSymbol, AnalysisPassInfo>(capacity: graphSemanticModel.Views.Length);
			}

			public IEnumerable<DataViewInfo> GetViewsToAnalyze()
			{
				var viewsToAnalyze = PassDirection == AnalysisPassDirection.Forward
										? GraphSemanticModel.Views
										: GraphSemanticModel.Views.Reverse();

				return viewsToAnalyze.Where(viewInfo => viewInfo.Type.TypeArguments.Any() && viewInfo.Symbol.Locations.Any());
			}

			public IEnumerable<INamedTypeSymbol> GetVisitedBaseDacs(ITypeSymbol viewDacType) =>
				viewDacType.GetBaseTypes()
						   .Where(baseDac => AnalysisPassInfoByDacType.ContainsKey(baseDac));
			

			public void ReportDiagnostic(Diagnostic diagnostic) => _symbolContext.ReportDiagnostic(diagnostic);
		}
	}
}