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

		public ImmutableArray<PrimaryDacRuleBase> Rules { get; }

		private readonly INamedTypeSymbol graphSymbol;
		
		private readonly Dictionary<string, ISymbol> graphViews;

		private PrimaryDacFinder(PXContext pxContext, SemanticModel semanticModel, INamedTypeSymbol graph,
								 ImmutableArray<PrimaryDacRuleBase> rules, CancellationToken cancellationToken)
		{
			SemanticModel = semanticModel;
			PxContext = pxContext;
			graphSymbol = graph;
			CancellationToken = cancellationToken;
			Rules = rules;
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

		}
	}
}