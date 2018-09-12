using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    //Initializers
    //Data views and delegates
    //Actions and handlers

    public class PXGraphSemanticModel
    {
        public ImmutableArray<GraphInitializerInfo> Initializers { get; private set; }

        private PXGraphSemanticModel()
        {
        }

        public static PXGraphSemanticModel GetModel(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol typeSymbol)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

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
                Initializers = GetInitializers(context, pxContext, typeSymbol, graphType)
            };

            return pxGraph;
        }

        private static ImmutableArray<GraphInitializerInfo> GetInitializers(SymbolAnalysisContext context, PXContext pxContext, INamedTypeSymbol typeSymbol, GraphType graphType)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            List<GraphInitializerInfo> initializers = typeSymbol.GetDeclaredInstanceConstructors(context.CancellationToken)
                                                      .Select(ctr => new GraphInitializerInfo(GraphInitializerType.InstanceCtr, ctr.Item1, ctr.Item2))
                                                      .ToList();
            Tuple<MethodDeclarationSyntax, IMethodSymbol> staticCtrInfo = typeSymbol.GetDeclaredStaticConstructor(context.CancellationToken);

            if (staticCtrInfo != null)
            {
                initializers.Add(new GraphInitializerInfo(GraphInitializerType.StaticCtr, staticCtrInfo.Item1, staticCtrInfo.Item2));
            }

            if (graphType == GraphType.PXGraphExtension)
            {
                Tuple<MethodDeclarationSyntax, IMethodSymbol> initialization = typeSymbol.GetGraphExtensionInitialization(pxContext, context.CancellationToken);

                if (initialization != null)
                {
                    initializers.Add(new GraphInitializerInfo(GraphInitializerType.InitializeMethod, initialization.Item1, initialization.Item2));
                }
            }

            return initializers.ToImmutableArray();
        }
    }
}
