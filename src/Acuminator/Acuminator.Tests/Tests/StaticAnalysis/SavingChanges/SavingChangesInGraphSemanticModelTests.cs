using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.SavingChanges;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SavingChanges
{
    public class SavingChangesInGraphSemanticModelTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(new SavingChangesInGraphSemanticModelAnalyzer());

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphIsSavingChangesInInstanceConstructor.cs")]
        public async Task GraphInstanceConstructor_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(17, 17));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphExtensionIsSavingChangesInInitializationMethod.cs")]
        public async Task GraphExtensionInitializationMethod_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1058_PXGraphSavingChangesDuringInitialization.CreateFor(15, 17));

        [Theory]
        [EmbeddedFileData(@"PXGraph\PXGraphIsntSavingChangesInInstanceConstructor.cs")]
        public async Task GraphInstanceConstructor_DoesntReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData(@"PXGraph\ViewDelegate.cs")]
        public async Task ViewDelegate(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(13, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\ViewDelegateWithParameter.cs")]
        public async Task ViewDelegateWithParameter(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(13, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionOwnView.cs")]
        public async Task ViewDelegateInGraphExtensionOwnView(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(12, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionBaseView.cs")]
        public async Task ViewDelegateInGraphExtensionBaseView(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(10, 13));

        [Theory]
        [EmbeddedFileData(@"PXGraph\ViewDelegateInGraphExtensionOverride.cs")]
        public async Task ViewDelegateInGraphExtensionOverride(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1083_SavingChangesInDataViewDelegate.CreateFor(10, 13));
    }
}
