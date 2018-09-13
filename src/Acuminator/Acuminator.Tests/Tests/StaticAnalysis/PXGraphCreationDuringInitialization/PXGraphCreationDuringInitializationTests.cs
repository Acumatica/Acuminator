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
        public void PXGraph_Instance_Constructor_Cannot_Invoke_CreateInstance_Method(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1099_PXGraphCreationDuringInitialization.CreateFor(15, 41));
        }

        [Theory]
        [EmbeddedFileData("PXGraphWithCreateInstanceInStaticConstructor.cs")]
        public void PXGraph_Static_Constructor_Cannot_Invoke_CreateInstance_Method(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1099_PXGraphCreationDuringInitialization.CreateFor(15, 41));
        }

        [Theory]
        [EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitMethod.cs")]
        public void PXGraphExtension_Initialize_Method_Cannot_Invoke_CreateInstance_Method(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1099_PXGraphCreationDuringInitialization.CreateFor(14, 41));
        }
    }
}
