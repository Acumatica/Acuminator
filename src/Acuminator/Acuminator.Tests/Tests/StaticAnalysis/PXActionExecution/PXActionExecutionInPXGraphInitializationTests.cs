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
        public async Task Press_PXGraphInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressInGraphExtension.cs")]
        public async Task Press_PXGraphExtensionInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressOnDerivedTypeInGraph.cs")]
        public async Task PressOnDerivedType_PXGraphInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressOnDerivedTypeInGraphExtension.cs")]
        public async Task PressOnDerivedType_PXGraphExtensionInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithAdapterInGraph.cs")]
        public async Task PressWithAdapter_PXGraphInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithAdapterInGraphExtension.cs")]
        public async Task PressWithAdapter_PXGraphExtensionInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithExternalMethodInGraph.cs")]
        public async Task PressWithExternalMethod_PXGraphInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(11, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PressWithExternalMethodInGraphExtension.cs")]
        public async Task PressWithExternalMethod_PXGraphExtensionInitialization(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1081_PXGraphExecutesActionDuringInitialization.CreateFor(9, 13));
    }
}
