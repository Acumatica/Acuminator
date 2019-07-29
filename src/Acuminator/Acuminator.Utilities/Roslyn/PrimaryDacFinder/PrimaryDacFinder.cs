using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
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

		public PXGraphSemanticModel GraphSemanticModel { get; }

		public ImmutableArray<ActionInfo> GraphActions { get; }

		public ImmutableArray<DataViewInfo> GraphViews { get; }

		private readonly Dictionary<ISymbol, (ITypeSymbol DAC, Score Score)> _viewsWithDacAndScores;
		private readonly ILookup<ITypeSymbol, ISymbol> _dacWithViewsLookup;

		private readonly List<PrimaryDacRuleBase> _absoluteRules;
		private readonly List<PrimaryDacRuleBase> _heuristicRules;

		private PrimaryDacFinder(PXContext pxContext, PXGraphSemanticModel graphSemanticModel, ImmutableArray<PrimaryDacRuleBase> rules,
								 CancellationToken cancellationToken, IEnumerable<DataViewInfo> graphViewInfos, IEnumerable<ActionInfo> actionInfos)
		{
			PxContext = pxContext;
			GraphSemanticModel = graphSemanticModel;
			CancellationToken = cancellationToken;
			GraphActions = actionInfos.ToImmutableArray();
			GraphViews = graphViewInfos.ToImmutableArray();
			_viewsWithDacAndScores = new Dictionary<ISymbol, (ITypeSymbol DAC, Score Score)>(GraphViews.Length);

			foreach (DataViewInfo viewInfo in GraphViews)
			{
				var dac = viewInfo.Type.GetDacFromView(PxContext);

				if (dac == null)
					continue;

				_viewsWithDacAndScores.Add(viewInfo.Symbol, (dac, new Score(0.0)));
			}

			_dacWithViewsLookup = _viewsWithDacAndScores.ToLookup(viewAndDac => viewAndDac.Value.DAC,
																  viewAndDac => viewAndDac.Key);
			_absoluteRules = rules.Where(rule => rule.IsAbsolute).ToList();
			_heuristicRules = rules.Where(rule => !rule.IsAbsolute).ToList();
		}

		public static PrimaryDacFinder Create(PXContext pxContext, INamedTypeSymbol graphOrGraphExtension, CancellationToken cancellationToken,
											  IRulesProvider rulesProvider = null)
		{
			if (pxContext == null || graphOrGraphExtension == null || cancellationToken.IsCancellationRequested)
				return null;

			bool isGraph = graphOrGraphExtension.InheritsFrom(pxContext.PXGraph.Type);

			if (!isGraph && !graphOrGraphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return null;

			PXGraphSemanticModel graphSemanticModel = PXGraphSemanticModel.InferModels(pxContext, graphOrGraphExtension, cancellationToken)
																		 ?.FirstOrDefault();
			return Create(pxContext, graphSemanticModel, cancellationToken, rulesProvider);
		}

		public static PrimaryDacFinder Create(PXContext pxContext, PXGraphSemanticModel graphSemanticModel, CancellationToken cancellationToken,
											  IRulesProvider rulesProvider = null)
		{
			if (pxContext == null || graphSemanticModel?.GraphSymbol == null || cancellationToken.IsCancellationRequested)
				return null;

			rulesProvider = rulesProvider ?? new DefaultRulesProvider(pxContext);
			var rules = rulesProvider.GetRules();

			if (rules.Length == 0 || cancellationToken.IsCancellationRequested)
				return null;

			var allGraphActionInfos = graphSemanticModel.Actions
														.Where(action => action.Symbol.ContainingType.IsPXGraph(pxContext))
														.OrderBy(action => action.DeclarationOrder);
			var allViewInfos = graphSemanticModel.Views
												 .Where(view => view.Symbol.ContainingType.IsPXGraph(pxContext))
												 .OrderBy(view => view.DeclarationOrder);

			return cancellationToken.IsCancellationRequested
				? null
				: new PrimaryDacFinder(pxContext, graphSemanticModel, rules, cancellationToken, allViewInfos, allGraphActionInfos);
		}

		public ITypeSymbol FindPrimaryDAC()
		{
			if (_dacWithViewsLookup.Count == 0)
				return null;

			foreach (PrimaryDacRuleBase rule in _absoluteRules)
			{
				ITypeSymbol primaryDAC = ApplyRule(rule);

				if (CancellationToken.IsCancellationRequested)
					return null;
				else if (primaryDAC != null)
					return primaryDAC;
			}

			if (CancellationToken.IsCancellationRequested)
				return null;

			foreach (PrimaryDacRuleBase rule in _heuristicRules)
			{
				ApplyRule(rule);

				if (CancellationToken.IsCancellationRequested)
					return null;
			}

			if (CancellationToken.IsCancellationRequested)
				return null;

			var maxScoredViews = _viewsWithDacAndScores.ItemsWithMaxValues(viewWithDacAndScore => viewWithDacAndScore.Value.Score.Value);
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
					viewCandidates = viewWithTypeCandidates?.Select(view => view.Symbol);
					dacCandidates = viewWithTypeCandidates?.Select(view => view.Type.GetDacFromView(PxContext));
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

			viewCandidates = viewCandidates ?? dacCandidatesList.SelectMany(dac => _dacWithViewsLookup[dac]);
			ScoreRuleForViewCandidates(viewCandidates, rule);
			return primaryDac;
		}

		private List<DataViewInfo> GetViewCandidatesFromViewRule(ViewRuleBase viewRule)
		{
			if (viewRule == null || GraphViews.Length == 0 || CancellationToken.IsCancellationRequested)
				return null;

			return GraphViews.Where(view => viewRule.SatisfyRule(this, view.Symbol, view.Type)).ToList();
		}

		private List<IGrouping<ITypeSymbol, ISymbol>> GetCandidatesFromDacRule(DacRuleBase dacRule)
		{
			if (dacRule == null || CancellationToken.IsCancellationRequested)
				return null;

			var candidates = _dacWithViewsLookup.TakeWhile(dacWithViews => !CancellationToken.IsCancellationRequested)
												  .Where(dacWithViews => dacRule.SatisfyRule(this, dacWithViews.Key));

			return !CancellationToken.IsCancellationRequested
				? candidates.ToList()
				: null;
		}

		private IEnumerable<ITypeSymbol> GetCandidatesFromActionRule(ActionRuleBase actionRule)
		{
			if (actionRule == null || GraphActions.Length == 0 || CancellationToken.IsCancellationRequested)
				return null;

			var dacCandidates = from action in GraphActions
								where actionRule.SatisfyRule(this, action.Symbol, action.Type)
								select action.Type.GetDacFromAction();

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
				if (!_viewsWithDacAndScores.TryGetValue(candidate, out var dacWithScore))
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