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

		public ImmutableArray<GraphExtensionInfo> GraphExtensionsFromPreviousLevels { get; }

		protected GraphExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol graphExtension, GraphInfo? graph,
									 ImmutableArray<GraphExtensionInfo> graphExtensionsFromPreviousLevels, int declarationOrder, 
									 GraphExtensionInfo baseInfo) :
							  this(node, graphExtension, graph, graphExtensionsFromPreviousLevels, declarationOrder)
		{
			_baseInfo = baseInfo.CheckIfNull();
			CombineWithBaseInfo(baseInfo);
		}

		protected GraphExtensionInfo(ClassDeclarationSyntax? node, INamedTypeSymbol graphExtension, GraphInfo? graph,
									 ImmutableArray<GraphExtensionInfo> graphExtensionsFromPreviousLevels, int declarationOrder) :
								base(node, graphExtension, declarationOrder)
		{
			Graph = graph;
			GraphExtensionsFromPreviousLevels = graphExtensionsFromPreviousLevels;
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
			var extensionsFromPreviousLevels = GetExtensionsFromPreviouslLevels(extensionBaseType, pxContext, graphInfo, cancellation);
			GraphExtensionInfo? aggregatedBaseGraphExtInfo = !SymbolEqualityComparer.Default.Equals(graphExtension.BaseType, extensionBaseType)
				? GetAggregatedBaseExtensions(graphExtension, graphInfo, extensionsFromPreviousLevels, isInSource, cancellation)
				: null;

			var graphExtensionInfo = aggregatedBaseGraphExtInfo != null
				? new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, extensionsFromPreviousLevels, 
										 graphExtDeclarationOrder, aggregatedBaseGraphExtInfo)
				: new GraphExtensionInfo(graphExtensionNode, graphExtension, graphInfo, extensionsFromPreviousLevels, graphExtDeclarationOrder);

			return graphExtensionInfo;
		}

		private static ImmutableArray<GraphExtensionInfo> GetExtensionsFromPreviouslLevels(INamedTypeSymbol extensionBaseType, PXContext pxContext,
																						   GraphInfo? graphInfo, CancellationToken cancellation)
		{
			if (!extensionBaseType.IsGenericType)
				return ImmutableArray<GraphExtensionInfo>.Empty;

			var typeArguments = extensionBaseType.TypeArguments;

			if (typeArguments.Length <= 1)
				return ImmutableArray<GraphExtensionInfo>.Empty;

			int baseExtensionDeclarationOrder = 1;
			int graphIndex = typeArguments.Length - 1;
			var extensionsFromPreviousLevels = ImmutableArray.CreateBuilder<GraphExtensionInfo>();

			for (int i = graphIndex - 1; i >= 0; i--)
			{
				cancellation.ThrowIfCancellationRequested();

				if (extensionBaseType.TypeArguments[i] is not INamedTypeSymbol baseExtension || !baseExtension.IsPXGraphExtension(pxContext))
					continue;

				var baseExtensionNode = baseExtension.GetSyntax(cancellation) as ClassDeclarationSyntax;
				var baseExtensionInfo = Create(baseExtension, baseExtensionNode, graphInfo, pxContext, baseExtensionDeclarationOrder, cancellation);

				if (baseExtensionInfo != null)
					extensionsFromPreviousLevels.Add(baseExtensionInfo);
			}

			return extensionsFromPreviousLevels.ToImmutable();
		}

		private static GraphExtensionInfo? GetAggregatedBaseExtensions(INamedTypeSymbol graphExtension, GraphInfo? graphInfo,
																	   in ImmutableArray<GraphExtensionInfo> extensionsFromBaseLevels,
																	   bool isInSource, CancellationToken cancellation)
		{
			var graphExtensionsBaseTypesFromBaseToDerived = graphExtension.GetGraphExtensionBaseTypes()
																		  .OfType<INamedTypeSymbol>()
																		  .Reverse();

			GraphExtensionInfo? aggregatedBaseGraphExtensionInfo = null, prevGraphExtensionInfo = null;

			foreach (INamedTypeSymbol baseType in graphExtensionsBaseTypesFromBaseToDerived)
			{
				cancellation.ThrowIfCancellationRequested();

				var baseGraphExtensionNode = isInSource
					? baseType.GetSyntax(cancellation) as ClassDeclarationSyntax
					: null;

				isInSource = baseGraphExtensionNode != null;
				aggregatedBaseGraphExtensionInfo = prevGraphExtensionInfo != null
					? new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, extensionsFromBaseLevels, declarationOrder: 0, prevGraphExtensionInfo)
					: new GraphExtensionInfo(baseGraphExtensionNode, baseType, graphInfo, extensionsFromBaseLevels, declarationOrder: 0);

				prevGraphExtensionInfo = aggregatedBaseGraphExtensionInfo;
			}

			return aggregatedBaseGraphExtensionInfo;
		}
	}
}