#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
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

		public PXGraphSemanticModel GraphSemanticModel { get; }

		public ImmutableArray<ActionInfo> GraphActions { get; }

		public ImmutableArray<DataViewInfo> GraphViews { get; }

		private readonly Dictionary<ISymbol, (DataViewInfo View, Score Score)> _viewsWithScores;
		private readonly ILookup<ITypeSymbol, DataViewInfo> _dacWithViewsLookup;

		private readonly List<PrimaryDacRuleBase> _absoluteRules;
		private readonly List<PrimaryDacRuleBase> _heuristicRules;

		private PrimaryDacFinder(PXContext pxContext, PXGraphSemanticModel graphSemanticModel, ImmutableArray<PrimaryDacRuleBase> rules,
								 CancellationToken cancellationToken, IEnumerable<DataViewInfo> graphViewInfos, IEnumerable<ActionInfo> actionInfos)
		{
			PxContext 		   = pxContext;
			GraphSemanticModel = graphSemanticModel;
			CancellationToken  = cancellationToken;
			GraphActions 	   = actionInfos.ToImmutableArray();
			GraphViews 		   = graphViewInfos.ToImmutableArray();

			_viewsWithScores = GraphViews.Where(viewInfo => viewInfo.DAC != null)
										 .ToDictionary(keySelector: viewInfo => viewInfo.Symbol, 
													   elementSelector: viewInfo => (viewInfo, new Score(0.0)));

			_dacWithViewsLookup = _viewsWithScores.Values.ToLookup(viewAndScore => viewAndScore.View.DAC!,
																   viewAndScore => viewAndScore.View);

			_absoluteRules  = rules.Where(rule => rule.IsAbsolute).ToList(capacity: 4);
			_heuristicRules = rules.Where(rule => !rule.IsAbsolute).ToList(capacity: 16);
		}

		public static PrimaryDacFinder? Create(PXContext pxContext, INamedTypeSymbol graphOrGraphExtension, CancellationToken cancellationToken,
											   IRulesProvider? customRulesProvider = null)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!graphOrGraphExtension.IsPXGraphOrExtension(pxContext))
				return null;

			PXGraphSemanticModel? graphSemanticModel = PXGraphSemanticModel.InferModels(pxContext, graphOrGraphExtension, 
																					   GraphSemanticModelCreationOptions.CollectGeneralGraphInfo, 
																					   cancellationToken)
																		  ?.FirstOrDefault();
			return graphSemanticModel != null 
				? Create(pxContext, graphSemanticModel, cancellationToken, customRulesProvider)
				: null;
		}

		public static PrimaryDacFinder? Create(PXContext pxContext, PXGraphSemanticModel graphSemanticModel, CancellationToken cancellationToken,
											   IRulesProvider? customRulesProvider = null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			pxContext.ThrowOnNull();

			if (graphSemanticModel.CheckIfNull().GraphSymbol == null)
				return null;

			IRulesProvider rulesProvider = customRulesProvider ?? new DefaultRulesProvider(pxContext);
			ImmutableArray<PrimaryDacRuleBase> rules = rulesProvider.GetRules();

			if (rules.Length == 0)
				return null;

			cancellationToken.ThrowIfCancellationRequested();

			var allGraphActionInfos = graphSemanticModel.Actions
														.Where(action => action.Symbol.ContainingType.IsPXGraph(pxContext))
														.OrderBy(action => action.DeclarationOrder);
			var allViewInfos = graphSemanticModel.Views
												 .Where(view => view.Symbol.ContainingType.IsPXGraph(pxContext))
												 .OrderBy(view => view.DeclarationOrder);

			cancellationToken.ThrowIfCancellationRequested();

			return new PrimaryDacFinder(pxContext, graphSemanticModel, rules, cancellationToken, allViewInfos, allGraphActionInfos);
		}

		public ITypeSymbol? FindPrimaryDAC()
		{
			if (_viewsWithScores.Count == 0)
				return null;

			foreach (PrimaryDacRuleBase rule in _absoluteRules)
			{
				CancellationToken.ThrowIfCancellationRequested();

				ITypeSymbol? primaryDAC = ApplyRule(rule);

				if (primaryDAC != null)
					return primaryDAC;
			}

			CancellationToken.ThrowIfCancellationRequested();

			foreach (PrimaryDacRuleBase rule in _heuristicRules)
			{
				CancellationToken.ThrowIfCancellationRequested();
				ApplyRule(rule);
			}

			var maxScoredViews = _viewsWithScores.ItemsWithMaxValues(viewWithDacAndScore => viewWithDacAndScore.Value.Score.Value);
			var maxScoredDACs = maxScoredViews.Select(viewWithDacAndScore => viewWithDacAndScore.Value.View.DAC!)
											  .ToHashSet();
			return maxScoredDACs.Count == 1
				? maxScoredDACs.FirstOrDefault()
				: null;
		}

		private ITypeSymbol? ApplyRule(PrimaryDacRuleBase rule)
		{
			var (dacCandidates, viewCandidates) = GetDacAndViewCandidates(rule);
			List<ITypeSymbol>? dacCandidatesList = dacCandidates?.Where(dac => dac != null).Distinct().ToList()!;

			if (dacCandidatesList?.Count is null or 0)
				return null;
			
			ITypeSymbol? primaryDac = null;

			if (rule.IsAbsolute && dacCandidatesList.Count == 1)
				primaryDac = dacCandidatesList[0];

			viewCandidates ??= dacCandidatesList.SelectMany(dac => _dacWithViewsLookup[dac]);
			ScoreRuleForViewCandidates(viewCandidates, rule);
			return primaryDac;
		}

		private (IEnumerable<ITypeSymbol?>? DacCandidates, IEnumerable<DataViewInfo>? ViewCandidates) GetDacAndViewCandidates(PrimaryDacRuleBase rule)
		{
			switch (rule)
			{
				case GraphRuleBase graphRule:
					return (DacCandidates: graphRule.GetCandidatesFromGraphRule(this), ViewCandidates: null);

				case ViewRuleBase viewRule:
				{
					var viewCandidates = GetViewCandidatesFromViewRule(viewRule);
					var dacCandidates  = viewCandidates?.Select(view => view.DAC);

					return (dacCandidates, viewCandidates);
				}
				case DacRuleBase dacRule:
				{
					var dacWithViewCandidates = GetDacCandidatesWithViewsFromDacRule(dacRule);
					var dacCandidates = dacWithViewCandidates?.Select(dacWithViews => dacWithViews.Key);
					var viewCandidates = dacWithViewCandidates?.SelectMany(dacWithViews => dacWithViews);

					return (dacCandidates, viewCandidates);
				}
				case ActionRuleBase actionRule:
					return (DacCandidates: GetCandidatesFromActionRule(actionRule), ViewCandidates: null);

				default:
					return (DacCandidates: null, ViewCandidates: null);
			}
		}

		private List<DataViewInfo>? GetViewCandidatesFromViewRule(ViewRuleBase viewRule) =>
			GraphViews.Length > 0
				? GraphViews.Where(view => viewRule.SatisfyRule(this, view)).ToList()
				: null;

		private List<IGrouping<ITypeSymbol, DataViewInfo>>? GetDacCandidatesWithViewsFromDacRule(DacRuleBase dacRule) =>
			_dacWithViewsLookup.Count > 0
				? _dacWithViewsLookup.Where(dacWithViews => dacRule.SatisfyRule(this, dacWithViews.Key)).ToList()
				: null;

		private IEnumerable<ITypeSymbol>? GetCandidatesFromActionRule(ActionRuleBase actionRule)
		{
			if (GraphActions.Length == 0)
				return null;

			var dacCandidates = from action in GraphActions
								where actionRule.SatisfyRule(this, action.Symbol, action.Type)
								select action.Type.GetDacFromAction();
			return dacCandidates;
		}

		private void ScoreRuleForViewCandidates(IEnumerable<DataViewInfo> viewCandidates, PrimaryDacRuleBase rule)
		{
			if (rule.Weight == 0)
				return;

			foreach (var viewInfo in viewCandidates)
			{
				if (!_viewsWithScores.TryGetValue(viewInfo.Symbol, out var viewWithScore))
					continue;

				Score score = viewWithScore.Score;

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