using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using CommonServiceLocator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public partial class PXGraphCreationForBqlQueriesAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1072_PXGraphCreationForBqlQueries);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterCodeBlockAction(c => AnalyzeCodeBlock(c, pxContext));
		}

		private void AnalyzeCodeBlock(CodeBlockAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			CSharpSyntaxNode body = GetBody(context.CodeBlock);
			if (body == null) return;

			var walker = new BqlGraphArgWalker(context.SemanticModel, pxContext);
			body.Accept(walker);

			if (walker.GraphArguments.IsEmpty) return;

			var dataFlow = context.SemanticModel.AnalyzeDataFlow(body);

			if (!dataFlow.Succeeded) return;

			var thisGraph = dataFlow.WrittenOutside.OfType<IParameterSymbol>()
				.FirstOrDefault(t => t.IsThis && t.Type != null && t.Type.IsPXGraphOrExtension(pxContext));

			foreach (var graphArgSyntax in walker.GraphArguments)
			{
				var instantiationType = GetGraphInstantiationType(graphArgSyntax, context.SemanticModel, pxContext);

				if (instantiationType != GraphInstantiationType.None && thisGraph != null)
				{
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries,
						graphArgSyntax.GetLocation()));
				}
			}
		}

		private CSharpSyntaxNode GetBody(SyntaxNode codeBlock)
		{
			switch (codeBlock)
			{
				case AccessorDeclarationSyntax accessorSyntax:
					return accessorSyntax.Body;
				case MethodDeclarationSyntax methodSyntax:
					return methodSyntax.Body ?? (CSharpSyntaxNode) methodSyntax.ExpressionBody?.Expression;
				case ConstructorDeclarationSyntax constructorSyntax:
					return constructorSyntax.Body;
				default:
					return null;
			}
		}

		private GraphInstantiationType GetGraphInstantiationType(SyntaxNode node, SemanticModel semanticModel, 
			PXContext pxContext)
		{
			// new PXGraph()
			if (node is ObjectCreationExpressionSyntax objCreationSyntax && objCreationSyntax.Type != null
			                                                             && semanticModel
				                                                             .GetSymbolInfo(objCreationSyntax.Type)
				                                                             .Symbol is ITypeSymbol typeSymbol
			                                                             && typeSymbol.IsPXGraph())
			{
				return GraphInstantiationType.Constructor;
			}

			// PXGraph.CreateInstance
			if (node is InvocationExpressionSyntax invocationSyntax)
			{
				var symbolInfo = semanticModel.GetSymbolInfo(invocationSyntax);
				var methodSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;

				if (methodSymbol != null && pxContext.PXGraphRelatedMethods.CreateInstance.Contains(methodSymbol))
				{
					return GraphInstantiationType.CreateInstance;
				}
			}

			return GraphInstantiationType.None;
		}

		private enum GraphInstantiationType
		{
			None,
			Constructor,
			CreateInstance
		}
	}
}
