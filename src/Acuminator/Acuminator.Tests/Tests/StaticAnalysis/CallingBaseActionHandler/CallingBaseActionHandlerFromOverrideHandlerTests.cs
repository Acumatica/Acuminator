using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseActionHandler;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallingBaseActionHandler
{
    public class CallingBaseActionHandlerFromOverrideHandlerTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new CallingBaseActionHandlerFromOverrideHandlerAnalyzer());

        [Theory]
        [EmbeddedFileData("BaseActionInvocation_Bad.cs")]
        public async Task BaseActionInvocation_ThroughAction_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation.CreateFor(14, 20));

        [Theory]
        [EmbeddedFileData("BaseActionHandlerInvocation_Bad.cs")]
        public async Task BaseActionInvocation_ThroughHandler_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation.CreateFor(17, 20));

        [Theory]
        [EmbeddedFileData("BaseActionInvocation_Good.cs")]
        public async Task BaseActionInvocation_ThroughAction_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData("BaseActionHandlerInvocation_Good.cs")]
        public async Task BaseActionInvocation_ThroughHandler_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);
    }
}
