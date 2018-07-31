using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Analyzers;
using Acuminator.Utilities.Extra;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A graph's primary DAC finder.
	/// </summary>
	public class PrimaryDacFinder
	{
		public PXContext PxContext { get; }

		public CancellationToken CancellationToken { get; }

		public SemanticModel SemanticModel { get; }

		public INamedTypeSymbol Graph { get; }

		public ImmutableArray<(ISymbol Action, INamedTypeSymbol ActionType)> GraphActionSymbolsWithTypes { get; }

		public ImmutableArray<(ISymbol View, INamedTypeSymbol ViewType)> GraphViewSymbolsWithTypes { get; }

		private readonly Dictionary<ISymbol, (ITypeSymbol DAC, double Score)> viewsWithDacAndScores;
		private readonly HashSet<ITypeSymbol> viewDacs;

		private readonly List<PrimaryDacRuleBase> absoluteRules;
		private readonly List<PrimaryDacRuleBase> heuristicRules;

		private PrimaryDacFinder(PXContext pxContext, SemanticModel semanticModel, INamedTypeSymbol graph,
								 ImmutableArray<PrimaryDacRuleBase> rules, CancellationToken cancellationToken,
								 IEnumerable<(ISymbol View, INamedTypeSymbol ViewType)> viewSymbolsWithTypes,
								 IEnumerable<(ISymbol Action, INamedTypeSymbol ActionType)> actionSymbolsWithTypes)
		{
			SemanticModel = semanticModel;
			PxContext = pxContext;
			Graph = graph;
			CancellationToken = cancellationToken;
			GraphActionSymbolsWithTypes = actionSymbolsWithTypes.ToImmutableArray();
			GraphViewSymbolsWithTypes = viewSymbolsWithTypes.ToImmutableArray();
			viewsWithDacAndScores = new Dictionary<ISymbol, (ITypeSymbol DAC, double Score)>(GraphViewSymbolsWithTypes.Length);
			viewDacs = new HashSet<ITypeSymbol>();

			foreach (var viewWithType in GraphViewSymbolsWithTypes)
			{
				var dac = viewWithType.ViewType.GetDacFromView(PxContext);

				if (dac == null)
					continue;

				viewsWithDacAndScores.Add(viewWithType.View, (dac, 0.0));
				viewDacs.Add(dac);
			}

			absoluteRules = rules.Where(rule => rule.IsAbsolute).ToList();
			heuristicRules = rules.Where(rule => !rule.IsAbsolute).ToList();
		}

		public static PrimaryDacFinder Create(PXContext pxContext, SemanticModel semanticModel, INamedTypeSymbol graphOrGraphExtension,
											  CancellationToken cancellationToken, IRulesProvider rulesProvider = null)
		{
			if (pxContext == null || semanticModel == null || graphOrGraphExtension == null || cancellationToken.IsCancellationRequested)
				return null;

			bool isGraph = graphOrGraphExtension.InheritsFrom(pxContext.PXGraphType);

			if (!isGraph && !graphOrGraphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return null;

			INamedTypeSymbol graph = isGraph
				? graphOrGraphExtension
				: graphOrGraphExtension.GetGraphFromGraphExtension(pxContext) as INamedTypeSymbol;

			if (graph == null || cancellationToken.IsCancellationRequested)
				return null;

			rulesProvider = rulesProvider ?? new DefaultRulesProvider(pxContext);
			var rules = rulesProvider.GetRules();

			if (rules.Length == 0 || cancellationToken.IsCancellationRequested)
				return null;

			var allGraphActionSymbolsWithTypes = graph.GetPXActionSymbolsWithTypesFromGraph(pxContext);

			if (cancellationToken.IsCancellationRequested)
				return null;

			var allViewSymbolsWithTypes = isGraph 
				? graphOrGraphExtension.GetViewsWithSymbolsFromPXGraph(pxContext) 
				: graphOrGraphExtension.GetViewSymbolsWithTypesFromGraphExtensionAndItsBaseGraph(pxContext);

			if (cancellationToken.IsCancellationRequested)
				return null;

			return new PrimaryDacFinder(pxContext, semanticModel, graph, rules, cancellationToken, allViewSymbolsWithTypes,
										allGraphActionSymbolsWithTypes);
		}

		public ITypeSymbol FindPrimaryDAC()
		{
			if (viewDacs.Count == 0)
				return null;

			foreach (PrimaryDacRuleBase rule in absoluteRules)
			{
				ITypeSymbol primaryDAC = ApplyRule(rule);

				if (CancellationToken.IsCancellationRequested)
					return null;
				else if (primaryDAC != null)
					return primaryDAC;
			}

			if (CancellationToken.IsCancellationRequested)
				return null;

			foreach (PrimaryDacRuleBase rule in heuristicRules)
			{
				ApplyRule(rule);

				if (CancellationToken.IsCancellationRequested)
					return null;
			}

			if (CancellationToken.IsCancellationRequested)
				return null;

			var maxScoredViews = viewsWithDacAndScores.ItemsWithMaxValues(viewWithDacAndScore => viewWithDacAndScore.Value.Score);
			var maxScoredDACs = maxScoredViews.Select(viewWithDacAndScore => viewWithDacAndScore.Value.DAC)
											  .ToHashSet();
			return maxScoredDACs.Count == 1 
				? maxScoredDACs.FirstOrDefault()
				: null;
		}

		private ITypeSymbol ApplyRule(PrimaryDacRuleBase rule)
		{	
			IEnumerable<ITypeSymbol> dacCandidates = null;
			IEnumerable<ISymbol> viewCandidates = null;

			switch (rule.RuleKind)
			{
				case PrimaryDacRuleKind.Graph:
					dacCandidates = (rule as GraphRuleBase)?.GetCandidatesFromGraphRule(this);
					break;
				case PrimaryDacRuleKind.View:
					var viewWithTypeCandidates = GetViewCandidatesFromViewRule(rule as ViewRuleBase);
					viewCandidates = viewWithTypeCandidates.Select(viewWithType => viewWithType.View);
					dacCandidates = viewWithTypeCandidates.Select(viewWithType => viewWithType.ViewType.GetDacFromView(PxContext));
					break;
				case PrimaryDacRuleKind.Dac:
					dacCandidates = GetCandidatesFromDacRule(rule as DacRuleBase);
					break;
				case PrimaryDacRuleKind.Action:
					dacCandidates = GetCandidatesFromActionRule(rule as ActionRuleBase);
					break;
			}

			if (dacCandidates.IsNullOrEmpty())
				return null;

			var dacCandidatesList = dacCandidates.Where(dac => dac != null).Distinct().ToList();
			ITypeSymbol primaryDac = null;

			if (rule.IsAbsolute && dacCandidatesList.Count == 1)
				primaryDac = dacCandidatesList[0];

			viewCandidates = viewCandidates ?? GetViewCandidatesFromDacCandidates(dacCandidatesList);
			ScoreRuleForViewCandidates(viewCandidates, rule);
			return primaryDac;
		}

		private List<(ISymbol View, INamedTypeSymbol ViewType)> GetViewCandidatesFromViewRule(ViewRuleBase viewRule)
		{
			if (viewRule == null || GraphViewSymbolsWithTypes.Length == 0 || CancellationToken.IsCancellationRequested)
				return default;

			return GraphViewSymbolsWithTypes.Where(viewWithType => viewRule.SatisfyRule(this, viewWithType.View, viewWithType.ViewType))
											.ToList();
		}

		private IEnumerable<ITypeSymbol> GetCandidatesFromDacRule(DacRuleBase dacRule)
		{
			if (dacRule == null || viewDacs.Count == 0 || CancellationToken.IsCancellationRequested)
				return null;

			var dacCandidates = viewDacs.TakeWhile(v => !CancellationToken.IsCancellationRequested)
										.Where(dac => dacRule.SatisfyRule(this, dac));
							
			return !CancellationToken.IsCancellationRequested
				? dacCandidates
				: null;
		}

		private IEnumerable<ITypeSymbol> GetCandidatesFromActionRule(ActionRuleBase actionRule)
		{
			if (actionRule == null || GraphViewSymbolsWithTypes.Length == 0 || CancellationToken.IsCancellationRequested)
				return null;

			var dacCandidates = from actionWithType in GraphActionSymbolsWithTypes
								where actionRule.SatisfyRule(this, actionWithType.Action, actionWithType.ActionType)
								select actionWithType.ActionType.GetDacFromAction();

			return !CancellationToken.IsCancellationRequested
				? dacCandidates
				: null;
		}

		private IEnumerable<ISymbol> GetViewCandidatesFromDacCandidates(IEnumerable<ITypeSymbol> dacCandidates)
		{
			var dacsSet = dacCandidates.ToHashSet();
			return from viewWithDacAndScore in viewsWithDacAndScores
				   where dacsSet.Contains(viewWithDacAndScore.Value.DAC)
				   select viewWithDacAndScore.Key;
		}

		private void ScoreRuleForViewCandidates(IEnumerable<ISymbol> viewCandidates, PrimaryDacRuleBase rule)
		{
			if (rule.Weight == 0)
				return;

			foreach (ISymbol candidate in viewCandidates)
			{
				if (!viewsWithDacAndScores.TryGetValue(candidate, out var dacWithScore))
					continue;

				if (rule.Weight > 0)
				{
					if (dacWithScore.Score <= double.MaxValue - rule.Weight)
						viewsWithDacAndScores[candidate] = (dacWithScore.DAC, dacWithScore.Score + rule.Weight);
					else
						viewsWithDacAndScores[candidate] = (dacWithScore.DAC, double.MaxValue);
				}
				else
				{
					if (dacWithScore.Score >= double.MinValue - rule.Weight)
						viewsWithDacAndScores[candidate] = (dacWithScore.DAC, dacWithScore.Score + rule.Weight);
					else
						viewsWithDacAndScores[candidate] = (dacWithScore.DAC, double.MinValue);
				}			
			}
		}
	}
}