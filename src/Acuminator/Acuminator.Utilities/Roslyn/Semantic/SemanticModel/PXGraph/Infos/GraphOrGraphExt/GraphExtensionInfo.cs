using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphExtensionInfo : GraphOrGraphExtInfoBase<GraphExtensionInfo>
	{
		public GraphInfo? Graph { get; }

		protected GraphExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol graphExtension, GraphInfo? graph,
									 int declarationOrder, GraphExtensionInfo baseInfo) :
								this(node, graphExtension, graph, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		protected GraphExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol graphExtension, GraphInfo? graph, int declarationOrder) :
								base(node, graphExtension, declarationOrder)
		{
			Graph = graph;
		}

		public static GraphExtensionInfo? Create(INamedTypeSymbol? graphExtension, ClassDeclarationSyntax? graphExtensionNode, ITypeSymbol? graph,
												 PXContext pxContext, int graphExtDeclarationOrder, CancellationToken cancellation)
		{
			var graphNode = graph.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var graphInfo = GraphInfo.Create(graph as INamedTypeSymbol, graphNode, pxContext, graphDeclarationOrder: 0, cancellation);

			return Create(graphExtension, graphExtensionNode, graphInfo, pxContext, graphExtDeclarationOrder, cancellation);
		}

		public static GraphExtensionInfo? Create(INamedTypeSymbol? graphExtension, ClassDeclarationSyntax? graphExtensionNode, GraphInfo? graphInfo,
												 PXContext pxContext, int graphExtDeclarationOrder, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (graphExtension == null)
				return null;

			INamedTypeSymbol? extensionBaseType = graphExtension.GetBaseTypesAndThis()
																.FirstOrDefault(type => type.IsGraphExtensionBaseType()) as INamedTypeSymbol;
			if (extensionBaseType == null)
				return null;

			bool isInSource = graphExtensionNode != null;
			var extensionFromPreviousLevels = GetAggregatedExtensionFromPreviouslLevels(extensionBaseType, pxContext, graphInfo, cancellation);
			GraphExtensionInfo? aggregatedBaseGraphExtInfo = !SymbolEqualityComparer.Default.Equals(graphExtension.BaseType, extensionBaseType)
				? GetAggregatedBaseExtensions(graphExtension, graphInfo, extensionFromPreviousLevels, isInSource, cancellation)
				: null;

			var graphExtensionInfo = aggregatedBaseGraphExtInfo != null
				? new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, graphExtDeclarationOrder, aggregatedBaseGraphExtInfo)
				: extensionFromPreviousLevels != null
					? new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, graphExtDeclarationOrder, extensionFromPreviousLevels)
					: new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, graphExtDeclarationOrder);

			return graphExtensionInfo;
		}

		private static GraphExtensionInfo? GetAggregatedExtensionFromPreviouslLevels(INamedTypeSymbol extensionBaseType, PXContext pxContext,
																					 GraphInfo? graphInfo, CancellationToken cancellation)
		{
			if (!extensionBaseType.IsGenericType)
				return null;

			var typeArguments = extensionBaseType.TypeArguments;

			if (typeArguments.Length <= 1)
				return null;

			if (typeArguments[0] is not INamedTypeSymbol previousLevelExtensionType || !previousLevelExtensionType.IsPXGraphExtension(pxContext))
				return null;

			var prevLevelExtensionNode = previousLevelExtensionType.GetSyntax(cancellation) as ClassDeclarationSyntax;
			var aggregatedPrevLevelGraphExtensionInfo = 
				Create(previousLevelExtensionType, prevLevelExtensionNode, graphInfo, pxContext, graphExtDeclarationOrder: 0, cancellation);

			return aggregatedPrevLevelGraphExtensionInfo;
		}

		private static GraphExtensionInfo? GetAggregatedBaseExtensions(INamedTypeSymbol graphExtension, GraphInfo? graphInfo,
																	   GraphExtensionInfo? aggregatedExtensionFromBaseLevels,
																	   bool isInSource, CancellationToken cancellation)
		{
			var graphExtensionsBaseTypesFromBaseToDerived = graphExtension.GetGraphExtensionBaseTypes()
																		  .OfType<INamedTypeSymbol>()
																		  .Reverse();

			GraphExtensionInfo? aggregatedBaseGraphExtensionInfo = null; 
			GraphExtensionInfo? prevGraphExtensionInfo = aggregatedExtensionFromBaseLevels;

			foreach (INamedTypeSymbol baseType in graphExtensionsBaseTypesFromBaseToDerived)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseGraphExtensionNode = isInSource
					? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
					: null;

				isInSource = baseGraphExtensionNode != null;
				aggregatedBaseGraphExtensionInfo = prevGraphExtensionInfo != null
					? new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, declarationOrder: 0, prevGraphExtensionInfo)
					: new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, declarationOrder: 0);

				prevGraphExtensionInfo = aggregatedBaseGraphExtensionInfo;
			}

			return aggregatedBaseGraphExtensionInfo;
		}
	}
}