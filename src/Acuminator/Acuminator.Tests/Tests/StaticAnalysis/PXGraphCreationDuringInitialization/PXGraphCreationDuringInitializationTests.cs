using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationDuringInitialization
{
    public class PXGraphCreationDuringInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new PXGraphCreationDuringInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData("PXGraphWithCreateInstanceInInstanceConstructor.cs")]
        public void PXGraphCreationDuringInitialization_GraphInstanceConstructor_CreateInstanceUsage(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(15, 41));
        }

        [Theory]
        [EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitMethod.cs")]
        public void PXGraphCreationDuringInitialization_GraphExtensionInitialize_CreateInstanceUsage(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(14, 41));
        }

        [Theory]
        [EmbeddedFileData("PXGrapWithCreateInstanceInInitDelegate.cs")]
        public void PXGraphCreationDuringInitialization_GraphInitDelegate_CreateInstanceUsage(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(20, 14));
        }

        [Theory]
        [EmbeddedFileData("PXGraphWithCreateInstanceOutsideOfInitialization.cs")]
        public void PXGraph_OutsideOfInitialization_CreateInstanceUsage(string source)
        {
            VerifyCSharpDiagnostic(source);
        }
    }
}
