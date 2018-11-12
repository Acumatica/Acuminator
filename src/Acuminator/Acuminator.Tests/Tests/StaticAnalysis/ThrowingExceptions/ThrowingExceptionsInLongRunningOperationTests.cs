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
    public class ThrowingExceptionsInLongRunningOperationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new ThrowingExceptionsInLongRunningOperationAnalyzer());

        [Theory]
        [EmbeddedFileData(@"LongOperations\LongOperationStart_Bad.cs")]
        public async Task LongOperationStart_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(21, 13));

        [Theory]
        [EmbeddedFileData(@"LongOperations\ParametersDelegate_Bad.cs")]
        public async Task ParametersDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(36, 21),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(50, 21),
                // With the current version of Roslyn packages 1.x we cannot analyze this construction
                //Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(55, 53),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(68, 17));

        [Theory]
        [EmbeddedFileData(@"LongOperations\ProcessDelegate_Bad.cs")]
        public async Task ProcessDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(29, 17),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(33, 17),
                // With the current version of Roslyn packages 1.x we cannot analyze this construction
                //Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(35, 57),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(40, 17),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(44, 17),
                // With the current version of Roslyn packages 1.x we cannot analyze this construction
                //Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(46, 67),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(51, 13),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(56, 13));

        [Theory]
        [EmbeddedFileData(@"LongOperations\FinallyProcessDelegate_Bad.cs")]
        public async Task FinallyProcessDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(30, 21),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(34, 21),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(40, 21),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(44, 21),
                // With the current version of Roslyn packages 1.x we cannot analyze this construction
                //Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(47, 53),
                // With the current version of Roslyn packages 1.x we cannot analyze this construction
                //Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(48, 41),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(53, 13),
                Descriptors.PX1086_ThrowingSetupNotEnteredExceptionInLongRunningOperation.CreateFor(58, 13));

        [Theory]
        [EmbeddedFileData(@"LongOperations\LongOperationStart_Good.cs")]
        public async Task LongOperationStart_WithoutPXSetupNotEnteredException_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData(@"LongOperations\ParametersDelegate_Good.cs")]
        public async Task ParametersDelegate_WithoutPXSetupNotEnteredException_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData(@"LongOperations\ProcessDelegate_Good.cs")]
        public async Task ProcessDelegate_WithoutPXSetupNotEnteredException_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData(@"LongOperations\FinallyProcessDelegate_Good.cs")]
        public async Task FinallyProcessDelegate_WithoutPXSetupNotEnteredException_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);
    }
}
