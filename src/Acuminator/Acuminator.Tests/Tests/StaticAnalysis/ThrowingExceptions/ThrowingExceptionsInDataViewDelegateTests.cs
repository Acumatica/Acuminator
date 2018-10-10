using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions
{
    public class ThrowingExceptionsInDataViewDelegateTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new ThrowingExceptionsInDataViewDelegateAnalyzer());

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\DataViewDelegateFromGraph.cs")]
        public async Task DataViewDelegateFromGraph_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(13, 13));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\.cs")]
        public async Task DataViewDelegateFromGraphExtension_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\.cs")]
        public async Task DataViewDelegateWithParameter_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\.cs")]
        public async Task StaticDataViewDelegate_ReportsDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\ExternalMethod.cs")]
        public async Task ExternalMethod_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"ViewDelegates\OtherExceptions.cs")]
        public async Task OtherExceptions_DoesntCauseDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual);
    }
}
