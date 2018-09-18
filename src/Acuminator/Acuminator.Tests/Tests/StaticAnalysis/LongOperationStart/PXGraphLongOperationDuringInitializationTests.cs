using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.LongOperationStart;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationStart
{
    public class PXGraphLongOperationDuringInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer () =>
            new PXGraphAnalyzer(new PXGraphLongOperationDuringInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphStartsLongOperationInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(17, 17));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphExtensionStartsLongOperationInInitializationMethod.cs")]
        public void GraphExtensionInitializationMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(15, 17));
        }

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphDoesntStartLongOperationInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_DoesntReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source);
        }
    }
}
