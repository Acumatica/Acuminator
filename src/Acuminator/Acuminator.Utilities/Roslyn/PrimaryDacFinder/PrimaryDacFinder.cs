using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder
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

		private readonly Dictionary<ISymbol, (ITypeSymbol DAC, Score Score)> viewsWithDacAndScores;
		private readonly ILookup<ITypeSymbol, ISymbol> dacWithViewsLookup;

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
			viewsWithDacAndScores = new Dictionary<ISymbol, (ITypeSymbol DAC, Score Score)>(GraphViewSymbolsWithTypes.Length);
			
			foreach (var viewWithType in GraphViewSymbolsWithTypes)
			{
				var dac = viewWithType.ViewType.GetDacFromView(PxContext);

				if (dac == null)
					continue;

				viewsWithDacAndScores.Add(viewWithType.View, (dac, new Score(0.0)));
			}

			dacWithViewsLookup = viewsWithDacAndScores.ToLookup(viewAndDac => viewAndDac.Value.DAC, 
																viewAndDac => viewAndDac.Key);

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
			if (dacWithViewsLookup.Count == 0)
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

			var maxScoredViews = viewsWithDacAndScores.ItemsWithMaxValues(viewWithDacAndScore => viewWithDacAndScore.Value.Score.Value);
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
					viewCandidates = viewWithTypeCandidates?.Select(viewWithType => viewWithType.View);
					dacCandidates = viewWithTypeCandidates?.Select(viewWithType => viewWithType.ViewType.GetDacFromView(PxContext));
					break;
				case PrimaryDacRuleKind.Dac:
					var dacWithViewCandidates = GetCandidatesFromDacRule(rule as DacRuleBase);
					dacCandidates = dacWithViewCandidates?.Select(group => group.Key);
					viewCandidates = dacWithViewCandidates?.SelectMany(group => group);			
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

			viewCandidates = viewCandidates ?? dacCandidatesList.SelectMany(dac => dacWithViewsLookup[dac]);
			ScoreRuleForViewCandidates(viewCandidates, rule);
			return primaryDac;
		}

		private List<(ISymbol View, INamedTypeSymbol ViewType)> GetViewCandidatesFromViewRule(ViewRuleBase viewRule)
		{
			if (viewRule == null || GraphViewSymbolsWithTypes.Length == 0 || CancellationToken.IsCancellationRequested)
				return null;

			return GraphViewSymbolsWithTypes.Where(viewWithType => viewRule.SatisfyRule(this, viewWithType.View, viewWithType.ViewType))
											.ToList();
		}

		private List<IGrouping<ITypeSymbol, ISymbol>> GetCandidatesFromDacRule(DacRuleBase dacRule)
		{
			if (dacRule == null || CancellationToken.IsCancellationRequested)
				return null;

			var candidates = dacWithViewsLookup.TakeWhile(dacWithViews => !CancellationToken.IsCancellationRequested)
												  .Where(dacWithViews => dacRule.SatisfyRule(this, dacWithViews.Key));
							
			return !CancellationToken.IsCancellationRequested
				? candidates.ToList()
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

		private void ScoreRuleForViewCandidates(IEnumerable<ISymbol> viewCandidates, PrimaryDacRuleBase rule)
		{
			if (rule.Weight == 0)
				return;

			foreach (ISymbol candidate in viewCandidates)
			{
				if (!viewsWithDacAndScores.TryGetValue(candidate, out var dacWithScore))
					continue;

				Score score = dacWithScore.Score;

				if (rule.Weight > 0)
				{
					if (score.Value <= double.MaxValue - rule.Weight)
						score.Value += rule.Weight;
					else
						score.Value = double.MaxValue;
				}
				else
				{
					if (score.Value >= double.MinValue - rule.Weight)
						score.Value += rule.Weight;
					else
						score.Value = double.MinValue;
				}			
			}
		}
	}
}