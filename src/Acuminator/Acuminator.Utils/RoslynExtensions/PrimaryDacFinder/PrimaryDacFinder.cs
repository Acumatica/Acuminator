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
		private readonly ClassDeclarationSyntax graphDeclaration;
		

		private readonly Dictionary<string, ISymbol> graphViews;

		private PrimaryDacFinder(PXContext pxContext, SemanticModel semanticModel, INamedTypeSymbol graph, ClassDeclarationSyntax graphNode,
								 ImmutableArray<PrimaryDacRuleBase> rules, CancellationToken cancellationToken)
		{
			SemanticModel = semanticModel;
			PxContext = pxContext;
			graphSymbol = graph;
			graphDeclaration = graphNode;
			CancellationToken = cancellationToken;
			Rules = rules;
		}

		public static async Task<PrimaryDacFinder> CreateAsync(PXContext pxContext, SemanticModel semanticModel, INamedTypeSymbol graph,
															   CancellationToken cancellationToken, IRulesProvider rulesProvider = null)
		{
			if (pxContext == null || semanticModel == null || graph == null || !graph.InheritsFrom(pxContext.PXGraphType) || 
				cancellationToken.IsCancellationRequested)
			{
				return null;
			}

			rulesProvider = rulesProvider ?? new DefaultRulesProvider();
			var rules = rulesProvider.GetRules();

			if (rules.Length == 0)
				return null;

			ClassDeclarationSyntax graphNode = await graph.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as ClassDeclarationSyntax;

			if (graphNode == null || cancellationToken.IsCancellationRequested)
				return null;
			
			return new PrimaryDacFinder(pxContext, semanticModel, graph, graphNode, rules, cancellationToken);
		}

		public INamedTypeSymbol FindPrimaryDAC()
		{

		}
	}
}