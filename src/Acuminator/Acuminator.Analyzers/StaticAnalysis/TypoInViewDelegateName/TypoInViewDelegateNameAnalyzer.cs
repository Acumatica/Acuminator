using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;


namespace Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName
{
    public class TypoInViewDelegateNameAnalyzer : PXGraphAggregatedAnalyzerBase
    {
	    public const string ViewFieldNameProperty = "field";
		private const int MaximumDistance = 2;

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type != GraphType.None;

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
			ImmutableArray.Create(Descriptors.PX1005_TypoInViewDelegateName);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphModel)
		{
			var viewWithoutDelegatesNames = graphModel.Views.Where(view => !graphModel.ViewDelegatesByNames.ContainsKey(view.Symbol.Name))
															.Select(view => view.Symbol.Name)
															.ToList(capacity: graphModel.ViewsByNames.Count);
			if (viewWithoutDelegatesNames.Count == 0)
				return;

			var delegateCandidates = from method in graphModel.Symbol.GetMembers().OfType<IMethodSymbol>()
									 where method.ContainingType.Equals(graphModel.Symbol) && !method.IsOverride &&
										   method.IsValidViewDelegate(pxContext) && !method.IsValidActionHandler(pxContext)
									 select method;

			foreach (IMethodSymbol method in delegateCandidates)
			{
				if (viewWithoutDelegatesNames.Any(viewName => viewName == method.Name))
					continue;

				string nearestViewName = FindNearestView(viewWithoutDelegatesNames, method);

				if (nearestViewName != null && !method.Locations.IsEmpty)
				{
					var properties = ImmutableDictionary.CreateBuilder<string, string>();
					properties.Add(ViewFieldNameProperty, nearestViewName);

					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1005_TypoInViewDelegateName, method.Locations.First(), properties.ToImmutable(), nearestViewName),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

	    private string FindNearestView(List<string> viewCandidatesNames, IMethodSymbol method)
	    {
			string methodName = method.Name.ToLowerInvariant();
		    int minDistance = int.MaxValue;
		    string nearestViewName = null;

		    foreach (var viewName in viewCandidatesNames)
		    {
			    int distance = StringExtensions.LevenshteinDistance(methodName, viewName.ToLowerInvariant());

			    if (distance <= MaximumDistance && distance < minDistance)
			    {
				    minDistance = distance;
				    nearestViewName = viewName;
			    }
		    }

		    return nearestViewName;
	    }
	}
}
