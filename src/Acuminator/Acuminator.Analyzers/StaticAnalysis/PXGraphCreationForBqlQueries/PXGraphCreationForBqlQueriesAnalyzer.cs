using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class PXGraphCreationForBqlQueriesAnalyzer : PXDiagnosticAnalyzer
	{
		public const string IdentifierNamePropertyPrefix = "IdentifierName";
		public const string IsGraphExtensionPropertyPrefix = "IsGraphExtension";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1072_PXGraphCreationForBqlQueries);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterCodeBlockAction(c => AnalyzeCodeBlock(c, pxContext));
		}

		private void AnalyzeCodeBlock(CodeBlockAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			// Get body from a method or property
			CSharpSyntaxNode body = context.CodeBlock?.GetBody();
			if (body == null) return;

			// Collect all PXGraph-typed method parameters passed to BQL queries
			var walker = new BqlGraphArgWalker(context.SemanticModel, pxContext);
			body.Accept(walker);
			if (walker.GraphArguments.IsEmpty) return;

			// Collect all available PXGraph instances (@this, method parameters, local variables)
			var existingGraphs = GetExistingGraphInstances(body, context.SemanticModel, pxContext);
			if (existingGraphs.IsEmpty) return;

			// Determine if available PXGraph instance is used outside of BQL queries
			var usedGraphs = GetSymbolUsages(body, existingGraphs, context.SemanticModel, walker.GraphArguments)
				.ToImmutableHashSet();
			var availableGraphs = existingGraphs.Except(usedGraphs).ToArray();

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
						CreateDiagnosticProperties(availableGraphs, pxContext)));
				}
				else if (availableGraphs.Length > 0 && context.SemanticModel.GetSymbolInfo(graphArgSyntax).Symbol is ILocalSymbol localVar 
				                                    && !usedGraphs.Contains(localVar))
				{
					context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries,
						graphArgSyntax.GetLocation(),
						CreateDiagnosticProperties(availableGraphs.Where(g => !Equals(g, localVar)), pxContext)));
				}
			}
		}

		private ImmutableArray<ISymbol> GetExistingGraphInstances(SyntaxNode body, SemanticModel semanticModel, 
			PXContext pxContext)
		{
			var dataFlow = semanticModel.AnalyzeDataFlow(body);

			if (!dataFlow.Succeeded) 
				return ImmutableArray<ISymbol>.Empty;

			// this
			var thisGraph = dataFlow.WrittenOutside.OfType<IParameterSymbol>()
				.FirstOrDefault(t => t.IsThis && t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));
			// Method parameter
			var parGraph = dataFlow.WrittenOutside.OfType<IParameterSymbol>()
				.FirstOrDefault(t => !t.IsThis && t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));
			// Local variable
			var localVarGraphs = dataFlow.WrittenInside.OfType<ILocalSymbol>()
				.Where(t => t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));

			// ReSharper disable once ImpureMethodCallOnReadonlyValueField
			var builder = ImmutableArray<ISymbol>.Empty.ToBuilder();

			if (thisGraph != null) builder.Add(thisGraph);
			if (parGraph != null) builder.Add(parGraph);
			builder.AddRange(localVarGraphs);

			return builder.ToImmutable();
		}

		private IEnumerable<ISymbol> GetSymbolUsages(CSharpSyntaxNode node, 
			IEnumerable<ISymbol> symbols, SemanticModel semanticModel, IEnumerable<SyntaxNode> nodesToSkip)
		{
			var symbolsSet = (symbols as IImmutableSet<ISymbol>) ?? symbols.ToImmutableHashSet();
			var nodesToSkipSet = (nodesToSkip as IImmutableSet<SyntaxNode>) ?? nodesToSkip.ToImmutableHashSet();

			foreach (var subNode in node.DescendantNodesAndSelf()
				.Where(n => !nodesToSkipSet.Contains(n)))
			{
				var symbol = semanticModel.GetSymbolInfo(subNode).Symbol;

				if (symbol != null && symbolsSet.Contains(symbol))
					yield return symbol;
			}
		}

		private ImmutableDictionary<string, string> CreateDiagnosticProperties(IEnumerable<ISymbol> availableGraphs, 
			PXContext pxContext)
		{
			var builder = ImmutableDictionary.CreateBuilder<string, string>();

			int i = 0;
			foreach (var graph in availableGraphs)
			{
				ITypeSymbol type = null;
				switch (graph)
				{
					case IParameterSymbol property:
						type = property.Type;
						break;
					case ILocalSymbol local:
						type = local.Type;
						break;
				}

				builder.Add(IdentifierNamePropertyPrefix + i, graph.Name);
				
				if (type != null && type.IsPXGraphExtension(pxContext))
					builder.Add(IsGraphExtensionPropertyPrefix + i, true.ToString());

				i++;
			}

			return builder.ToImmutable();
		}
	}
}
