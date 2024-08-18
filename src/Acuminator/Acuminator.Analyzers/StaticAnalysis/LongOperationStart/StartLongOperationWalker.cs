#nullable enable

using System;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationStart
{
	public class StartLongOperationWalker : NestedInvocationWalker
    {
        private readonly Action<Diagnostic> _reportDiagnostic;
        private readonly DiagnosticDescriptor _descriptor;

        public StartLongOperationWalker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
            : base(pxContext, context.CancellationToken)
        {
            _reportDiagnostic = context.ReportDiagnostic;
            _descriptor = descriptor.CheckIfNull();
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            ThrowIfCancellationRequested();

            IMethodSymbol? methodSymbol = GetSymbol<IMethodSymbol>(node);

			if (methodSymbol == null)
			{
				base.VisitInvocationExpression(node);
				return;
			}

            if (PxContext.StartOperation.Contains(methodSymbol) || 
				(!methodSymbol.IsDefinition && methodSymbol.OriginalDefinition != null && PxContext.StartOperation.Contains(methodSymbol.OriginalDefinition)))
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
