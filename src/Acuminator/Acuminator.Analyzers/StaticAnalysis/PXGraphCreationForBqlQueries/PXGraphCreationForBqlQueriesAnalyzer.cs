using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
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
	public class PXGraphCreationForBqlQueriesAnalyzer : PXDiagnosticAnalyzer
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

			var walker = new Walker(context.SemanticModel, pxContext);
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

		private class Walker : CSharpSyntaxWalker
		{
			private const string SelectMethodName = "Select";
			private const string SearchMethodName = "Search";

			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;

			private readonly List<ExpressionSyntax> _graphArguments = new List<ExpressionSyntax>();
			public ImmutableArray<ExpressionSyntax> GraphArguments => _graphArguments.ToImmutableArray();

			public Walker(SemanticModel semanticModel, PXContext pxContext)
			{
				semanticModel.ThrowOnNull(nameof (semanticModel));
				pxContext.ThrowOnNull(nameof (pxContext));

				_semanticModel = semanticModel;
				_pxContext = pxContext;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				var symbolInfo = _semanticModel.GetSymbolInfo(node);
				var methodSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;

				// Check BQL Select / Search methods by name because
				// variations of these methods are declared in different PXSelectBase-derived classes
				var declaringType = methodSymbol?.ContainingType?.OriginalDefinition;

				if (declaringType != null && declaringType.IsBqlCommand(_pxContext) 
					&& !methodSymbol.Parameters.IsEmpty && methodSymbol.Parameters[0].Type.IsPXGraph(_pxContext) 
					&& node.ArgumentList.Arguments.Count > 0 &&
				    (methodSymbol.Name.StartsWith(SelectMethodName, StringComparison.Ordinal) ||
				     methodSymbol.Name.StartsWith(SearchMethodName, StringComparison.Ordinal)))
				{
					var graphArg = node.ArgumentList.Arguments[0].Expression;
					_graphArguments.Add(graphArg);
				}
			}
		}
	}
}
