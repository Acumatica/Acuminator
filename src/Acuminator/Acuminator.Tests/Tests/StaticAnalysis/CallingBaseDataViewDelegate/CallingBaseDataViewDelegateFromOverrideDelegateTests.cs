using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseDataViewDelegate;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallingBaseDataViewDelegate
{
    public class CallingBaseDataViewDelegateFromOverrideDelegateTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new CallingBaseDataViewDelegateFromOverrideDelegateAnalyzer());

        [Theory]
        [EmbeddedFileData("BaseViewInvocation_ThroughView.cs")]
        public async Task BaseViewInvocation_ThroughView_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1087_PossibleStackOverflowExceptionInBaseViewDelegateInvocation.CreateFor(13, 26));

        [Theory]
        [EmbeddedFileData("BaseViewInvocation_ThroughSelectBase.cs")]
        public async Task BaseViewDelegateInvocation_ThroughSelectBase_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1087_PossibleStackOverflowExceptionInBaseViewDelegateInvocation.CreateFor(10, 26));

        [Theory]
        [EmbeddedFileData("BaseViewInvocation_ExternalMethod.cs")]
        public async Task BaseViewDelegateInvocation_ExternalMethod_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1087_PossibleStackOverflowExceptionInBaseViewDelegateInvocation.CreateFor(10, 20),
                Descriptors.PX1087_PossibleStackOverflowExceptionInBaseViewDelegateInvocation.CreateFor(26, 26));

        [Theory]
        [EmbeddedFileData("BaseViewInvocation_WithRedeclaredView.cs")]
        public async Task BaseViewDelegateInvocation_WithRedeclaredView_DoesnotReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);
    }
}
