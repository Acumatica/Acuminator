﻿
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class PXGraphCreationForBqlQueriesAnalyzer : PXDiagnosticAnalyzer
	{
		public static readonly string IdentifierNamePropertyPrefix = "IdentifierName";
		public static readonly string IsGraphExtensionPropertyPrefix = "IsGraphExtension";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable,
			Descriptors.PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable);

		public PXGraphCreationForBqlQueriesAnalyzer() : this(null)
		{ }

		public PXGraphCreationForBqlQueriesAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterCodeBlockAction(c => AnalyzeCodeBlock(c, pxContext));
		}

		private void AnalyzeCodeBlock(CodeBlockAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			// Get body from a method or property
			CSharpSyntaxNode? body = context.CodeBlock?.GetBody();
			if (body == null) return;

			// Collect all PXGraph-typed method parameters passed to BQL queries except graph-typed fields and properties
			var walker = new BqlGraphArgWalker(context.SemanticModel, pxContext, context.CancellationToken);
			body.Accept(walker);
			if (walker.GraphArguments.IsEmpty) return;

			// Collect all available PXGraph instances (@this, method parameters, local variables)
			var availableGraphs = GetExistingGraphInstances(body, context.SemanticModel, pxContext, context.CancellationToken);

			if (availableGraphs.Count == 0)
			{
				AnalyseCaseWithNoAvailableExistingGraphs(context, pxContext, walker.GraphArguments);
				return;
			}

			// Determine usage locations of available PXGraph instance outside of BQL queries
			var availableGraphUsages = GetGraphSymbolsUsages(body, availableGraphs, context.SemanticModel, walker.GraphArguments, context.CancellationToken);

			// Analyze each PXGraph-typed parameter in BQL queries
			foreach (ExpressionSyntax graphArgSyntax in walker.GraphArguments)
			{
				AnalyzeGraphArgumentOfBqlQuery(context, pxContext, graphArgSyntax, availableGraphs, availableGraphUsages);
			}
		}

		private List<ISymbol> GetExistingGraphInstances(SyntaxNode body, SemanticModel semanticModel, PXContext pxContext, CancellationToken cancellation)
		{
			var dataFlow = semanticModel.TryAnalyzeDataFlow(body);

			if (dataFlow == null) 
				return new List<ISymbol>();

			// this
			var containingCodeElement = body.Parent<MemberDeclarationSyntax>();
			bool isInsideStaticCodeElement = containingCodeElement?.IsStatic() ?? false;
			var thisGraph = isInsideStaticCodeElement 
				? null
				: dataFlow.WrittenOutside.OfType<IParameterSymbol>()
						  .FirstOrDefault(t => t.IsThis && t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));
			// Method parameter
			var parGraph = dataFlow.WrittenOutside.OfType<IParameterSymbol>()
				.FirstOrDefault(t => !t.IsThis && t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));

			// Local variable
			var localVarGraphs = dataFlow.WrittenInside.OfType<ILocalSymbol>()
				.Where(t => t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));

			// ReSharper disable once ImpureMethodCallOnReadonlyValueField
			var existingGraphs = new List<ISymbol>();

			if (thisGraph != null)
				existingGraphs.Add(thisGraph);

			if (parGraph != null)
				existingGraphs.Add(parGraph);

			existingGraphs.AddRange(localVarGraphs);				

			return existingGraphs;
		}

		private void AnalyseCaseWithNoAvailableExistingGraphs(CodeBlockAnalysisContext context, PXContext pxContext, ImmutableArray<ExpressionSyntax> bqlSelectGraphArgNodes)
		{
			if (bqlSelectGraphArgNodes.Length == 1)		//Do not report a case with a single BQL query with graph creation in argument
				return;

			// Do not report case when several BQL queries use the same symbol
			var graphArgsSymbols = GetDifferentSymbolsFromCallArgs(context.SemanticModel, bqlSelectGraphArgNodes, context.CancellationToken);

			if (graphArgsSymbols.Count <= 1)
				return;

			// Report a warning to create a shared graph variable to the user
			foreach (var graphArgSyntax in bqlSelectGraphArgNodes)
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable, graphArgSyntax.GetLocation()),
					pxContext.CodeAnalysisSettings);
			}		
		}

		private HashSet<ISymbol> GetDifferentSymbolsFromCallArgs(SemanticModel semanticModel, ImmutableArray<ExpressionSyntax> bqlSelectGraphArgNodes, 
																 CancellationToken cancellation) =>
			bqlSelectGraphArgNodes.Select(graphArgSyntax => semanticModel.GetSymbolOrFirstCandidate(graphArgSyntax, cancellation))
								  .Where(graphArgSymbol => graphArgSymbol != null && (graphArgSymbol is not IPropertySymbol or IFieldSymbol))
								  .ToHashSet()!;

		private Dictionary<ISymbol, List<SyntaxNode>> GetGraphSymbolsUsages(CSharpSyntaxNode body, List<ISymbol> existingGraphs, SemanticModel semanticModel,
																			ImmutableArray<ExpressionSyntax> bqlSelectGraphArgNodesToSkip, 
																			CancellationToken cancellation)
		{
			var nodesToVisit = body.DescendantNodesAndSelf()
								   .Where(n => n is not ExpressionSyntax expressionNode || 
											  !bqlSelectGraphArgNodesToSkip.Contains(expressionNode));
			var graphSymbolsUsages = new Dictionary<ISymbol, List<SyntaxNode>>(SymbolEqualityComparer.Default);

			foreach (var subNode in nodesToVisit)
			{
				var symbol = semanticModel.GetSymbolOrFirstCandidate(subNode, cancellation);

				if (symbol != null && existingGraphs.Contains(symbol, SymbolEqualityComparer.Default))
				{
					if (graphSymbolsUsages.TryGetValue(symbol, out List<SyntaxNode> usageNodes))
						usageNodes.Add(subNode);
					else
						graphSymbolsUsages.Add(symbol, new List<SyntaxNode> { subNode });
				}
			}

			return graphSymbolsUsages;
		}

		private void AnalyzeGraphArgumentOfBqlQuery(CodeBlockAnalysisContext context, PXContext pxContext, ExpressionSyntax graphArgSyntax,
													List<ISymbol> availableGraphs, Dictionary<ISymbol, List<SyntaxNode>> availableGraphUsages)
		{
			var instantiationType = graphArgSyntax.GetGraphInstantiationType(context.SemanticModel, pxContext);

			// New PXGraph() / new TGraph() / PXGraph.CreateInstance<TGraph> are reported at all times
			// All other usages are reported only if:
			// 1. There is at least one existing PXGraph instance available
			// 2. PXGraph parameter is not used in any way because its modifications might affect the BQL query results
			if (instantiationType != GraphInstantiationType.None)
			{
				var diagnosticPropertiesForGraphCreatedInArgumentExpression = CreateDiagnosticProperties(availableGraphs, pxContext);
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable, graphArgSyntax.GetLocation(),
									  diagnosticPropertiesForGraphCreatedInArgumentExpression),
					pxContext.CodeAnalysisSettings);
				return;
			}

			var localVar = context.SemanticModel.GetSymbolOrFirstCandidate(graphArgSyntax, context.CancellationToken) as ILocalSymbol;

			// Do not report and do not suggest to change the graph if it is used somewhere else to avoid disruptive side effects in the business logic
			// This includes fields and properties that store graph or a graph extension. The analysis of type members that store graphs
			//  goes outside of method boundaries. We can't predict side effects in the business logic cuased by the change of a graph stored as type member
			if (localVar == null || availableGraphUsages.ContainsKey(localVar))  
				return;

			var availableGraphsNotUsedBeforeBqlQuery = 
				availableGraphs.Where(graph => IsAvailableGraphNotUsedBeforeBqlQuery(graph, graphArgSyntax, availableGraphUsages))
							   .ToList();

			if (availableGraphsNotUsedBeforeBqlQuery.Count == 0)
				return;			

			// If there is only a single local variable available then there are no other graphs in the context to use other than this local variable.
			// This is the case of static graph methods used by processing views where a graph instance is created and used inside the static method. 
			// We allow such cases.
			if (availableGraphsNotUsedBeforeBqlQuery.Count == 1 && localVar.Equals(availableGraphsNotUsedBeforeBqlQuery[0], SymbolEqualityComparer.Default))
				return;

			var diagnosticProperties = CreateDiagnosticProperties(
											availableGraphsNotUsedBeforeBqlQuery.Where(graph => !localVar.Equals(graph, SymbolEqualityComparer.Default)), 
											pxContext);
			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable, graphArgSyntax.GetLocation(), diagnosticProperties),
				pxContext.CodeAnalysisSettings);
		}

		private bool IsAvailableGraphNotUsedBeforeBqlQuery(ISymbol availableGraph, ExpressionSyntax graphArgSyntax, 
														   Dictionary<ISymbol, List<SyntaxNode>> availableGraphsUsages)
		{
			if (!availableGraphsUsages.TryGetValue(availableGraph, out List<SyntaxNode> graphUsages))
				return true;

			int graphArgStart = graphArgSyntax.SpanStart;

			foreach (SyntaxNode graphUsage in graphUsages)
			{
				if (graphUsage.Span.End < graphArgStart)
					return false;
			}

			return true;
		}

		private ImmutableDictionary<string, string?> CreateDiagnosticProperties(IEnumerable<ISymbol> availableGraphs, PXContext pxContext)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string?>();
			int i = 0;

			foreach (var graph in availableGraphs)
			{
				ITypeSymbol? type = graph switch
				{
					IParameterSymbol property => property.Type,
					ILocalSymbol local        => local.Type,
					IFieldSymbol field        => field.Type,
					IPropertySymbol property  => property.Type,
					_ => null
				};

				builder.Add(IdentifierNamePropertyPrefix + i, graph.Name);
				
				if (type != null && type.IsPXGraphExtension(pxContext))
					builder.Add(IsGraphExtensionPropertyPrefix + i, true.ToString());

				i++;
			}

			return builder.ToImmutable();
		}
	}
}
