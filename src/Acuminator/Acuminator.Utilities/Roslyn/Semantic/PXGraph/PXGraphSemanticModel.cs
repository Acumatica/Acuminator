using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class PXGraphSemanticModel
    {
        public ImmutableArray<GraphInitializerInfo> Initializers { get; private set; }

        private PXGraphSemanticModel()
        {
        }

        public static PXGraphSemanticModel GetModel(PXContext pxContext, INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
        {
            cancellation.ThrowIfCancellationRequested();
            pxContext.ThrowOnNull(nameof(pxContext));
            typeSymbol.ThrowOnNull(nameof(typeSymbol));

            GraphType graphType = GraphType.PXGraph;

            if (typeSymbol.IsPXGraphExtension(pxContext))
            {
                graphType = GraphType.PXGraphExtension;
            }
            else if (!typeSymbol.IsPXGraph(pxContext))
            {
                return null;
            }

            PXGraphSemanticModel pxGraph = new PXGraphSemanticModel
            {
                Initializers = GetInitializers(cancellation, pxContext, typeSymbol, graphType)
            };

            return pxGraph;
        }

        private static ImmutableArray<GraphInitializerInfo> GetInitializers(CancellationToken cancellation, PXContext pxContext, INamedTypeSymbol typeSymbol, GraphType graphType)
        {
            cancellation.ThrowIfCancellationRequested();

            List<GraphInitializerInfo> initializers = typeSymbol.GetDeclaredInstanceConstructors(cancellation)
                                                      .Select(ctr => new GraphInitializerInfo(GraphInitializerType.InstanceCtr, ctr.Item1, ctr.Item2))
                                                      .ToList();
            Tuple<ConstructorDeclarationSyntax, IMethodSymbol> staticCtrInfo = typeSymbol.GetDeclaredStaticConstructor(cancellation);

            if (staticCtrInfo != null)
            {
                initializers.Add(new GraphInitializerInfo(GraphInitializerType.StaticCtr, staticCtrInfo.Item1, staticCtrInfo.Item2));
            }

            if (graphType == GraphType.PXGraphExtension)
            {
                Tuple<MethodDeclarationSyntax, IMethodSymbol> initialization = typeSymbol.GetGraphExtensionInitialization(pxContext, cancellation);

                if (initialization != null)
                {
                    initializers.Add(new GraphInitializerInfo(GraphInitializerType.InitializeMethod, initialization.Item1, initialization.Item2));
                }
            }

            return initializers.ToImmutableArray();
        }
    }
}
