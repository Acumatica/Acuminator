#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
			Descriptors.PX1072_PXGraphCreationForBqlQueries);

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

			// Collect all PXGraph-typed method parameters passed to BQL queries
			var walker = new BqlGraphArgWalker(context.SemanticModel, pxContext);
			body.Accept(walker);
			if (walker.GraphArguments.IsEmpty) return;

			// Collect all available PXGraph instances (@this, method parameters, local variables)
			var availableGraphs = GetExistingGraphInstances(body, context.SemanticModel, pxContext);
			if (availableGraphs.Count == 0) 
				return;

			// Determine if available PXGraph instance is used outside of BQL queries
			var usedGraphs = GetGraphSymbolUsages(body, availableGraphs, context.SemanticModel, walker.GraphArguments, context.CancellationToken)
								.ToHashSet();

			// Remove graphs used somewhere outside of BQL statements
			if (usedGraphs.Count > 0)
			{
			foreach (var graph in usedGraphs)
			{
				availableGraphs.Remove(graph);
			}
			}

			// Analyze each PXGraph-typed parameter in BQL queries
			foreach (var graphArgSyntax in walker.GraphArguments)
			{
				var instantiationType = graphArgSyntax.GetGraphInstantiationType(context.SemanticModel, pxContext);

				// New PXGraph() / new TGraph() / PXGraph.CreateInstance<TGraph> are reported at all times
				// All other usages are reported only if:
				// 1. There is at least one existing PXGraph instance available
				// 2. PXGraph parameter is not used in any way because its modifications might affect the BQL query results
				if (instantiationType != GraphInstantiationType.None)
				{
					context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries,
						graphArgSyntax.GetLocation(),
						CreateDiagnosticProperties(availableGraphs, pxContext)),
						pxContext.CodeAnalysisSettings);
				}
				else if (availableGraphs.Count > 0 && 
						 context.SemanticModel.GetSymbolInfo(graphArgSyntax).Symbol is ILocalSymbol localVar && !usedGraphs.Contains(localVar))
				{
					context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries,
						graphArgSyntax.GetLocation(),
						CreateDiagnosticProperties(availableGraphs.Where(g => !Equals(g, localVar)), pxContext)),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

		private List<ISymbol> GetExistingGraphInstances(SyntaxNode body, SemanticModel semanticModel, PXContext pxContext)
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

		private IEnumerable<ISymbol> GetGraphSymbolUsages(CSharpSyntaxNode node, List<ISymbol> existingGraphs, SemanticModel semanticModel, 
														  ImmutableArray<ExpressionSyntax> bqlSelectGraphArgNodesToSkip, CancellationToken cancellation)
		{
			var nodesToVisit = node.DescendantNodesAndSelf()
								   .Where(n => n is not ExpressionSyntax expressionNode || 
											  !bqlSelectGraphArgNodesToSkip.Contains(expressionNode));

			foreach (var subNode in nodesToVisit)
			{
				var symbolInfo = semanticModel.GetSymbolInfo(subNode, cancellation);
				var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

				if (symbol != null)
				{
					if (existingGraphs.Contains(symbol))
					yield return symbol;
			}
				else
				{
					var declaredSymbol = semanticModel.GetDeclaredSymbol(subNode, cancellation);

					if (declaredSymbol != null && existingGraphs.Contains(declaredSymbol))
						yield return declaredSymbol;
				}				
			}
		}
		}
		}

		private ImmutableDictionary<string, string> CreateDiagnosticProperties(IEnumerable<ISymbol> availableGraphs, PXContext pxContext)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string>();
			int i = 0;

			foreach (var graph in availableGraphs)
			{
				ITypeSymbol? type = graph switch
				{
					IParameterSymbol property => property.Type,
					ILocalSymbol local => local.Type,
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
