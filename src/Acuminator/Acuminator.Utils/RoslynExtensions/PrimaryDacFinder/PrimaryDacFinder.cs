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

		private readonly List<(ISymbol View, INamedTypeSymbol ViewType)> graphViews;
		private readonly Dictionary<string, INamedTypeSymbol> graphViewsByName;

		public ImmutableDictionary<string, INamedTypeSymbol> GraphActionsByName { get; }

		private readonly Dictionary<ITypeSymbol, double> dacWithScores;

		private readonly List<PrimaryDacRuleBase> absoluteRules;

		private readonly List<PrimaryDacRuleBase> heuristicRules;

		private PrimaryDacFinder(PXContext pxContext, SemanticModel semanticModel, INamedTypeSymbol graph,
								 ImmutableArray<PrimaryDacRuleBase> rules, CancellationToken cancellationToken)
		{
			SemanticModel = semanticModel;
			PxContext = pxContext;
			Graph = graph;
			CancellationToken = cancellationToken;

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

			rulesProvider = rulesProvider ?? new DefaultRulesProvider();
			var rules = rulesProvider.GetRules();

			if (rules.Length == 0 || cancellationToken.IsCancellationRequested)
				return null;

			var allActions = GetAllActionsFromGraphOrGraphExtension(graphOrGraphExtension, pxContext, isGraph, cancellationToken);

			if (allActions == null || cancellationToken.IsCancellationRequested)
				return null;

			return new PrimaryDacFinder(pxContext, semanticModel, graph, rules, cancellationToken);
		}

		private static IEnumerable<INamedTypeSymbol> GetAllActionsFromGraphOrGraphExtension(INamedTypeSymbol graphOrGraphExtension, PXContext pxContext,
																							bool isGraph, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return null;

			var actions = isGraph
				? graphOrGraphExtension.GetPXActionsFromGraphOrGraphExtension(pxContext)
				: graphOrGraphExtension.GetPXActionsFromGraphExtensionAndItsBaseGraph(pxContext);

			return actions.Where(action => action.IsGenericType);
		}

		public ITypeSymbol FindPrimaryDAC()
		{
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

			var maxScoredDACs = dacWithScores.ItemsWithMaxValues(dacWithScore => dacWithScore.Value);

			return maxScoredDACs.Count == 1 
				? maxScoredDACs[0].Key
				: null;
		}

		private ITypeSymbol ApplyRule(PrimaryDacRuleBase rule)
		{	
			IEnumerable<ITypeSymbol> dacCandidates = null;

			switch (rule)
			{
				case GraphRuleBase graphRule:
					dacCandidates = graphRule.GetCandidatesFromGraphRule(this, Graph);				
					break;
				case ViewRuleBase viewRule:
					dacCandidates = GetCandidatesFromViewRule(viewRule);
					break;

				case DacRuleBase dacRule:
					dacCandidates = GetCandidatesFromDacRule(dacRule);
					break;
			}

			if (dacCandidates == null)
				return null;

			ITypeSymbol primaryDac = null;

			if (rule.IsAbsolute && dacCandidates.IsSingle())
				primaryDac = dacCandidates.Single();

			ScoreRuleForCandidates(dacCandidates, rule);
			return primaryDac;
		}

		private IEnumerable<ITypeSymbol> GetCandidatesFromViewRule(ViewRuleBase viewRule)
		{
			if (graphViews.Count == 0 || CancellationToken.IsCancellationRequested)
				return null;

			var dacCandidates = from viewWithType in graphViews.TakeWhile(v => !CancellationToken.IsCancellationRequested)
								where viewRule.SatisfyRule(this, viewWithType.View, viewWithType.ViewType)
								select viewWithType.ViewType.GetDacFromView(PxContext);

			return !CancellationToken.IsCancellationRequested
				? dacCandidates
				: null;
		}

		private IEnumerable<ITypeSymbol> GetCandidatesFromDacRule(DacRuleBase dacRule)
		{
			if (dacWithScores.Count == 0 || CancellationToken.IsCancellationRequested)
				return null;

			var dacCandidates = dacWithScores.Keys.TakeWhile(v => !CancellationToken.IsCancellationRequested)
												  .Where(dac => dacRule.SatisfyRule(this, dac));
							
			return !CancellationToken.IsCancellationRequested
				? dacCandidates
				: null;
		}

		private void ScoreRuleForCandidates(IEnumerable<ITypeSymbol> dacCandidates, PrimaryDacRuleBase rule)
		{
			if (rule.Weight == 0)
				return;

			foreach (ITypeSymbol candidate in dacCandidates)
			{
				if (!dacWithScores.TryGetValue(candidate, out double score))
					continue;

				dacWithScores[candidate] = score + rule.Weight;
			}
		}
	}
}