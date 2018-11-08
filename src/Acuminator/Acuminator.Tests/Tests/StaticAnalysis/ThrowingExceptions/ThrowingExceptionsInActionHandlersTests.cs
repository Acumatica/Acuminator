using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions
{
    public class ThrowingExceptionsInActionHandlersTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new ThrowingExceptionsInActionHandlersAnalyzer());

        [Theory]
        [EmbeddedFileData(@"ActionHandlers\Handlers_Bad.cs")]
        public async Task Actions_ReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers.CreateFor(19, 13),
                Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers.CreateFor(26, 13),
                Descriptors.PX1090_ThrowingSetupNotEnteredExceptionInActionHandlers.CreateFor(35, 13));

        [Theory]
        [EmbeddedFileData(@"ActionHandlers\Handlers_Good.cs")]
        public async Task Actions_WithoutPXSetupNotEnteredException_DontReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);
    }
}
