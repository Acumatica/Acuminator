using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	}
}
