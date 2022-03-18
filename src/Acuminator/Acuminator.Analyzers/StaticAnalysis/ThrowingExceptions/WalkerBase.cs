#nullable enable

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

        protected bool IsPXSetupNotEnteredException(ExpressionSyntax? expressionAfterThrowkeyword)
        {
            ThrowIfCancellationRequested();

            if (expressionAfterThrowkeyword?.SyntaxTree == null)
            {
                return false;
            }

			SemanticModel? semanticModel = GetSemanticModel(expressionAfterThrowkeyword.SyntaxTree);
			ITypeSymbol? exceptionType = semanticModel?.GetTypeInfo(expressionAfterThrowkeyword).Type;

            if (exceptionType == null)
            {
                return false;
            }

            bool isSetupNotEntered = exceptionType.InheritsFromOrEquals(_pxContext.Exceptions.PXSetupNotEnteredException);
            return isSetupNotEntered;
        }
    }
}
