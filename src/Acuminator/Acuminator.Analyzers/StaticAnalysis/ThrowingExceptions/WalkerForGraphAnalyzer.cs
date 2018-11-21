using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
    internal class WalkerForGraphAnalyzer : WalkerBase
    {
        private readonly DiagnosticDescriptor _descriptor;

        public WalkerForGraphAnalyzer(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
                : base(context, pxContext)
        {
            descriptor.ThrowOnNull(nameof(descriptor));

            _descriptor = descriptor;
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            ThrowIfCancellationRequested();

            if (IsPXSetupNotEnteredException(node))
            {
                ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
            }
            else
            {
                base.VisitThrowStatement(node);
            }
        }
    }
}
