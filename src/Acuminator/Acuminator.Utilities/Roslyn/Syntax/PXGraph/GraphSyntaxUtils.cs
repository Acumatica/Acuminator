using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax.PXGraph
{
	public static class GraphSyntaxUtils
	{
		/// <summary>
		/// Determines PXGraph instantiation type for the syntax node (e.g. new PXGraph(), PXGraph.CreateInstance, etc.)
		/// </summary>
		public static GraphInstantiationType GetGraphInstantiationType(this SyntaxNode node, SemanticModel semanticModel, 
			PXContext pxContext)
		{
			node.ThrowOnNull(nameof (node));
			semanticModel.ThrowOnNull(nameof (semanticModel));
			pxContext.ThrowOnNull(nameof (pxContext));

			// new PXGraph()
			if (node is ObjectCreationExpressionSyntax objCreationSyntax && objCreationSyntax.Type != null
			                                                             && semanticModel
				                                                             .GetSymbolInfo(objCreationSyntax.Type)
				                                                             .Symbol is ITypeSymbol typeSymbol
			                                                             && typeSymbol.IsPXGraph(pxContext))
			{
				return typeSymbol.Equals(pxContext.PXGraph.Type)
					? GraphInstantiationType.ConstructorOfBaseType
					: GraphInstantiationType.ConstructorOfSpecificType;
			}

			// PXGraph.CreateInstance
			if (node is InvocationExpressionSyntax invocationSyntax)
			{
				var symbolInfo = semanticModel.GetSymbolInfo(invocationSyntax);
				var methodSymbol = (symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
				methodSymbol = methodSymbol?.OverriddenMethod?.OriginalDefinition ?? methodSymbol?.OriginalDefinition;

				if (methodSymbol != null && pxContext.PXGraph.CreateInstance.Contains(methodSymbol))
				{
					return GraphInstantiationType.CreateInstance;
				}
			}

			return GraphInstantiationType.None;
		}

		public static IEnumerable<(ITypeSymbol GraphSymbol, SyntaxNode GraphNode)> GetDeclaredGraphs(this SyntaxNode root, SemanticModel semanticModel,
																									 PXContext context, CancellationToken cancellationToken = default)
		{
			root.ThrowOnNull(nameof(root));
			context.ThrowOnNull(nameof(context));
			semanticModel.ThrowOnNull(nameof(semanticModel));
			cancellationToken.ThrowIfCancellationRequested();

			return GetDeclaredGraphsImpl();


			IEnumerable<(ITypeSymbol GraphSymbol, SyntaxNode GraphNode)> GetDeclaredGraphsImpl()
			{
				var declaredClasses = root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>();

				foreach (ClassDeclarationSyntax classNode in declaredClasses)
				{
					ITypeSymbol classTypeSymbol = classNode.GetTypeSymbolFromClassDeclaration(semanticModel, cancellationToken);

					if (classTypeSymbol != null && classTypeSymbol.IsPXGraph(context))
					{
						yield return (classTypeSymbol, classNode);
					}
				}
			}
		}

		public static ITypeSymbol GetTypeSymbolFromClassDeclaration(this ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel,
																	CancellationToken cancellationToken = default)
		{
			classDeclaration.ThrowOnNull(nameof(classDeclaration));
			semanticModel.ThrowOnNull(nameof(semanticModel));
			cancellationToken.ThrowIfCancellationRequested();

			SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(classDeclaration, cancellationToken);
			var typeSymbol = symbolInfo.Symbol as ITypeSymbol;

			if (typeSymbol == null && symbolInfo.CandidateSymbols.Length == 1)
			{
				typeSymbol = symbolInfo.CandidateSymbols[0] as ITypeSymbol;
			}

			return typeSymbol;
		}
	}
}
