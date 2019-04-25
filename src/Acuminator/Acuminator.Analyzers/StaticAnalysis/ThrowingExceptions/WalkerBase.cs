using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
    public abstract class WalkerBase : NestedInvocationWalker
    {
        protected readonly SymbolAnalysisContext _context;
        protected readonly PXContext _pxContext;

        protected WalkerBase(SymbolAnalysisContext context, PXContext pxContext)
            : base(context.Compilation, context.CancellationToken, pxContext.CodeAnalysisSettings)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            _context = context;
            _pxContext = pxContext;
        }

        protected bool IsPXSetupNotEnteredException(ThrowStatementSyntax node)
        {
            ThrowIfCancellationRequested();

            if (node?.Expression?.SyntaxTree == null)
            {
                return false;
            }

            var semanticModel = GetSemanticModel(node.Expression.SyntaxTree);
            if (semanticModel == null)
            {
                return false;
            }

            var typeInfo = semanticModel.GetTypeInfo(node.Expression);
            if (typeInfo.Type == null)
            {
                return false;
            }

            var isSetupNotEntered = typeInfo.Type.InheritsFromOrEquals(_pxContext.Exceptions.PXSetupNotEnteredException);
            return isSetupNotEntered;
        }
    }
}
