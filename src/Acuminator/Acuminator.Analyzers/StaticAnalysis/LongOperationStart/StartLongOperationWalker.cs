using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
    public class StartLongOperationWalker : NestedInvocationWalker
    {
        private readonly PXContext _pxContext;
        private readonly Action<Diagnostic> _reportDiagnostic;
        private readonly DiagnosticDescriptor _descriptor;

        public StartLongOperationWalker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
            : base(context.Compilation, context.CancellationToken)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            pxContext.ThrowOnNull(nameof(pxContext));
            descriptor.ThrowOnNull(nameof(descriptor));

            _pxContext = pxContext;
            _reportDiagnostic = context.ReportDiagnostic;
            _descriptor = descriptor;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            ThrowIfCancellationRequested();

            IMethodSymbol methodSymbol = GetSymbol<IMethodSymbol>(node);

            if (_pxContext.StartOperation.Contains(methodSymbol))
            {
                ReportDiagnostic(_reportDiagnostic, _descriptor, node);
            }
            else
            {
                base.VisitInvocationExpression(node);
            }
        }
    }
}
