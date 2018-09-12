using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class GraphInitializerInfo
    {
        public GraphInitializerType Type { get; private set; }
        public MethodDeclarationSyntax Declaration { get; private set; }
        public IMethodSymbol Symbol { get; private set; }

        public GraphInitializerInfo(GraphInitializerType type, MethodDeclarationSyntax declaration, IMethodSymbol symbol)
        {
            declaration.ThrowOnNull(nameof(declaration));
            symbol.ThrowOnNull(nameof(symbol));

            Type = type;
            Declaration = declaration;
            Symbol = symbol;
        }
    }
}
