using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
    public class PXActionExecutionInPXGraphInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new PXActionExecutionInPXGraphInitializerAnalyzer());

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressInGraph.cs")]
        public Task Press_PXGraphInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressInGraphExtension.cs")]
        public Task Press_PXGraphExtensionInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressOnDerivedTypeInGraph.cs")]
        public Task PressOnDerivedType_PXGraphInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressOnDerivedTypeInGraphExtension.cs")]
        public Task PressOnDerivedType_PXGraphExtensionInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithAdapterInGraph.cs")]
        public Task PressWithAdapter_PXGraphInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithAdapterInGraphExtension.cs")]
        public Task PressWithAdapter_PXGraphExtensionInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithExternalMethodInGraph.cs")]
        public Task PressWithExternalMethod_PXGraphInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithExternalMethodInGraphExtension.cs")]
        public Task PressWithExternalMethod_PXGraphExtensionInitialization(string actual) =>
            VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));
    }
}
