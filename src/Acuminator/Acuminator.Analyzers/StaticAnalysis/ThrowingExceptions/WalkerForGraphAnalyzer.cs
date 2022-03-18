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

        public WalkerForGraphAnalyzer(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor) : 
								 base(context, pxContext)
        {
            descriptor.ThrowOnNull(nameof(descriptor));

            _descriptor = descriptor;
        }

		public override void VisitThrowExpression(ThrowExpressionSyntax throwExpression)
		{
			ThrowIfCancellationRequested();

			if (IsPXSetupNotEnteredException(throwExpression.Expression))
			{
				ReportDiagnostic(_context.ReportDiagnostic, _descriptor, throwExpression);
			}
			else
			{
				base.VisitThrowExpression(throwExpression);
			}
		}

		public override void VisitThrowStatement(ThrowStatementSyntax throwStatement)
        {
            ThrowIfCancellationRequested();

            if (IsPXSetupNotEnteredException(throwStatement.Expression))
            {
                ReportDiagnostic(_context.ReportDiagnostic, _descriptor, throwStatement);
            }
            else
            {
                base.VisitThrowStatement(throwStatement);
            }
        } 
    }
}
