using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphLongOperationDuringInitialization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphLongOperationDuringInitialization
{
    public class PXGraphLongOperationDuringInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer () =>
            new PXGraphAnalyzer(new PXGraphLongOperationDuringInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData("PXGraphStartsLongOperationInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(17, 17));
        }

        [Theory]
        [EmbeddedFileData("PXGraphExtensionStartsLongOperationInInitializationMethod.cs")]
        public void GraphExtensionInitializationMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1054_PXGraphLongRunOperationDuringInitialization.CreateFor(15, 17));
        }

        [Theory]
        [EmbeddedFileData("PXGraphDoesntStartLongOperationInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_DoesntReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source);
        }
    }
}
