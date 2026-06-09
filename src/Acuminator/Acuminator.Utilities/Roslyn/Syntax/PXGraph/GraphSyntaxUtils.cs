using System;
using System.Collections.Generic;
using System.Linq;
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
		/// Get PXGraph instantiation type for the syntax node (e.g. new PXGraph(), PXGraph.CreateInstance, etc.)
		/// </summary>
		/// <param name="node">The syntax node to act on.</param>
		/// <param name="semanticModel">The semantic model.</param>
		/// <param name="pxContext">The context.</param>
		/// <param name="cancellation">A token that allows processing to be cancelled.</param>
		/// <returns>
		/// The graph instantiation type.
		/// </returns>
		public static GraphInstantiationType GetGraphInstantiationType(this SyntaxNode node, SemanticModel semanticModel, 
																	   PXContext pxContext, CancellationToken cancellation)
		{
			node.ThrowOnNull();
			semanticModel.ThrowOnNull();
			pxContext.ThrowOnNull();

			switch (node)
			{
				// new PXGraph()
				case ObjectCreationExpressionSyntax objCreationSyntax:
				{
					var typeSymbol = semanticModel.GetSymbolOrBestCandidate(objCreationSyntax.Type, cancellation) as ITypeSymbol;

					if (typeSymbol == null || !typeSymbol.IsPXGraph(pxContext))
						return GraphInstantiationType.None;

					return typeSymbol.Equals(pxContext.PXGraph.Type, SymbolEqualityComparer.Default)
						? GraphInstantiationType.ConstructorOfBaseType
						: GraphInstantiationType.ConstructorOfSpecificType;
				}

				// new()
				case ImplicitObjectCreationExpressionSyntax implicitObjectCreationSyntax:
				{
					var expressionTypeInfo = semanticModel.GetTypeInfo(node, cancellation);
					
					if (expressionTypeInfo.Type is not { } typeSymbol || !typeSymbol.IsPXGraph(pxContext))
						return GraphInstantiationType.None;

					return typeSymbol.Equals(pxContext.PXGraph.Type, SymbolEqualityComparer.Default)
						? GraphInstantiationType.ConstructorOfBaseType
						: GraphInstantiationType.ConstructorOfSpecificType;
				}

				// PXGraph.CreateInstance
				case InvocationExpressionSyntax invocationSyntax:
				{
						var methodSymbol = semanticModel.GetSymbolOrBestCandidate(invocationSyntax, cancellation) as IMethodSymbol;

						if (methodSymbol == null || methodSymbol.MethodKind != MethodKind.Ordinary)
							return GraphInstantiationType.None;

						IMethodSymbol? methodToCheck;

						if (methodSymbol.IsOverride)
						{
							methodToCheck = methodSymbol.GetOverriddenAndThis().LastOrDefault()?.OriginalDefinition ?? 
											methodSymbol?.OriginalDefinition; ;
						}
						else
						{
							methodToCheck = methodSymbol.OriginalDefinition;
						}

						if (methodToCheck != null &&
							pxContext.PXGraph.CreateInstance.Contains<IMethodSymbol>(methodToCheck, SymbolEqualityComparer.Default))
						{
							return GraphInstantiationType.CreateInstance;
						}
						else
							return GraphInstantiationType.None;
				}

				default:
					return GraphInstantiationType.None;
			}
		}

		internal static IEnumerable<(ITypeSymbol GraphSymbol, ClassDeclarationSyntax GraphNode)> GetDeclaredGraphsAndExtensions(
																				this SyntaxNode root, SemanticModel semanticModel,
																				PXContext context, CancellationToken cancellationToken = default)
		{
			root.ThrowOnNull();
			context.ThrowOnNull();
			semanticModel.ThrowOnNull();
			cancellationToken.ThrowIfCancellationRequested();

			return context.IsPlatformReferenced 
				? GetDeclaredGraphsAndExtensionsImpl()
				: [];

			//------------------------------------------------Local Function------------------------------------------------
			IEnumerable<(ITypeSymbol GraphSymbol, ClassDeclarationSyntax GraphNode)> GetDeclaredGraphsAndExtensionsImpl()
			{
				var declaredClasses = root.DescendantNodesAndSelf()
										  .OfType<ClassDeclarationSyntax>();

				foreach (ClassDeclarationSyntax classDeclaration in declaredClasses)
				{
					var classTypeSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) as ITypeSymbol;

					if (classTypeSymbol != null && classTypeSymbol.IsPXGraphOrExtension(context))
					{
						yield return (classTypeSymbol, classDeclaration);
					}
				}
			}
		}
	}
}
