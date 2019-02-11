using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.InvalidViewUsageInProcessingDelegate;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidViewUsageInProcessingDelegate
{
    public class InvalidViewUsageInProcessingDelegateTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled(),
                new InvalidViewUsageInProcessingDelegateAnalyzer());

        [Theory]
        [EmbeddedFileData("ParametersDelegate_Bad.cs")]
        public async Task ParametersDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(32, 46),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(41, 46),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(45, 92),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(53, 42));

        [Theory]
        [EmbeddedFileData("ProcessDelegate_Bad.cs")]
        public async Task ProcessDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(33, 33),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(42, 33),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(45, 96),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(54, 33),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(63, 33),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(66, 106),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(75, 29),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(85, 29));

        [Theory]
        [EmbeddedFileData("FinallyProcessDelegate_Bad.cs")]
        public async Task FinallyProcessDelegate_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source,
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(30, 22),
				Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(34, 39),
				Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(39, 27),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(45, 27),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(50, 27),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(53, 59),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(54, 47),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(59, 19),
                Descriptors.PX1088_InvalidViewUsageInProcessingDelegate.CreateFor(65, 19));

        [Theory]
        [EmbeddedFileData("ParametersDelegate_Good.cs")]
        public async Task ParametersDelegate_DoesnotReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData("ProcessDelegate_Good.cs")]
        public async Task ProcessDelegate_DoesnotReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData("FinallyProcessDelegate_Good.cs")]
        public async Task FinallyProcessDelegate_DoesnotReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);
    }
}
