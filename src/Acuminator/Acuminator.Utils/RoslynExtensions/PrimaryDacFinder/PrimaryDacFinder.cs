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


namespace Acuminator.Utilities
{
	/// <summary>
	/// A graph's primary DAC finder.
	/// </summary>
	public class PrimaryDacFinder
	{
		private readonly PXContext pxContext;
		private readonly INamedTypeSymbol graphSymbol;
		private readonly ClassDeclarationSyntax graphDeclaration;
		private readonly SemanticModel semanticModel;
		private CancellationToken cancellationToken;

		private readonly Dictionary<string, ISymbol> graphViews;

		private PrimaryDacFinder(PXContext context, SemanticModel semModel, INamedTypeSymbol graph, ClassDeclarationSyntax graphNode,
								 CancellationToken cToken)
		{ 
			semanticModel = semModel;
			pxContext = context;
			graphSymbol = graph;
			graphDeclaration = graphNode;
			cancellationToken = cToken;
		}

		public static async Task<PrimaryDacFinder> CreateAsync(PXContext context, SemanticModel semModel, INamedTypeSymbol graph,
															   CancellationToken cToken)
		{
			if (context == null || semModel == null || graph == null || !graph.InheritsFrom(context.PXGraphType) || 
				cToken.IsCancellationRequested)
			{
				return null;
			}

			ClassDeclarationSyntax graphNode = await graph.GetSyntaxAsync(cToken).ConfigureAwait(false) as ClassDeclarationSyntax;

			if (graphNode == null || cToken.IsCancellationRequested)
				return null;
			
			return new PrimaryDacFinder(context, semModel, graph, graphNode, cToken);
		}

		public INamedTypeSymbol FindPrimaryDAC()
		{

		}
	}
}