using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Analyzers.StaticAnalysis.PXGraphSavingChangesDuringInitialization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphSavingChangesDuringInitialization
{
    public class PXGraphSaveChangesDuringInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new PXGraphSavingChangesDuringInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData("PXGraphIsSavingChangesInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(17, 17));
        }

        [Theory]
        [EmbeddedFileData("PXGraphExtensionIsSavingChangesInInitializationMethod.cs")]
        public void GraphExtensionInitializationMethod_ReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source, Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(15, 17));
        }

        [Theory]
        [EmbeddedFileData("PXGraphIsntSavingChangesInInstanceConstructor.cs")]
        public void GraphInstanceConstructor_DoesntReportsDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source);
        }
    }
}
