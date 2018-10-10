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
    public class ThrowingExceptionsInDataViewDelegateTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new ThrowingExceptionsInDataViewDelegateAnalyzer());

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraph_View.cs")]
        public async Task PXGraph_View_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(13, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraphExtension_OwnView.cs")]
        public async Task PXGraphExtension_OwnView_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(12, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraphExtension_BaseView.cs")]
        public async Task PXGraphExtension_BaseView_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(10, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraphExtension_Override.cs")]
        public async Task PXGraphExtension_Override_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(10, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraph_DelegateWithParameter.cs")]
        public async Task PXGraph_DelegateWithParameter_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(13, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraphExtension_StaticDelegate.cs")]
        public async Task PXGraphExtension_StaticDelegate_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(16, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraphExtension_ExternalMethod.cs")]
        public async Task PXGraphException_ExternalMethod_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(16, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\PXGraph_OtherExceptions.cs")]
        public async Task PXGraph_OtherExceptions_DoesntReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual);
    }
}
