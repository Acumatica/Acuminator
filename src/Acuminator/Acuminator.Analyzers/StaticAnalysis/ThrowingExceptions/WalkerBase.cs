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
            : base(context.Compilation, context.CancellationToken)
        {
            pxContext.ThrowOnNull(nameof(pxContext));

            _context = context;
            _pxContext = pxContext;
        }

        protected bool IsPXSetupNotEnteredException(ThrowStatementSyntax node)
        {
            ThrowIfCancellationRequested();

            if (node == null || node.Expression == null)
            {
                return false;
            }

            var exceptionType = GetExceptionType(node.Expression);
            if (exceptionType == null)
            {
                return false;
            }

            var isSetupNotEntered = exceptionType.InheritsFromOrEquals(_pxContext.Exceptions.PXSetupNotEnteredException);
            return isSetupNotEntered;
        }

        private ITypeSymbol GetExceptionType(ExpressionSyntax node)
        {
            var symbol = GetSymbol<ISymbol>(node);

            switch (symbol)
            {
                case IMethodSymbol method:
                    return method.MethodKind == MethodKind.Constructor
                        ? method.ContainingType
                        : method.ReturnType;
                case ILocalSymbol local:
                    return local.Type;
                default:
                    return null;
            }
        }
    }
}
