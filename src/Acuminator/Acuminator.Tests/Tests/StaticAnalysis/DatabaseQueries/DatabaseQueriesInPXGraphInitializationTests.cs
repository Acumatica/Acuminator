using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DatabaseQueries;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DatabaseQueries
{
    public class DatabaseQueriesInPXGraphInitializationTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(
                CodeAnalysisSettings.Default
                .WithRecursiveAnalysisEnabled()
                .WithIsvSpecificAnalyzersEnabled(),
                new DatabaseQueriesInPXGraphInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData(@"Initializers\PXGraphConstructor.cs")]
        public async Task PXGraphConstructor_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\CreateInstanceDelegate.cs")]
        public async Task CreateInstanceDelegate_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(20, 25));

        [Theory]
        [EmbeddedFileData(@"Initializers\BQLSelect.cs")]
        public async Task BQLSelect_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\BQLSearch.cs")]
        public async Task BQLSearch_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\DataView.cs")]
        public async Task DataView_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(19, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\PXView.cs")]
        public async Task PXView_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(19, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\PXSelector.cs")]
        public async Task PXSelector_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 21));

        [Theory]
        [EmbeddedFileData(@"Initializers\PXDatabase.cs")]
        public async Task PXDatabase_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\ExternalMethod.cs")]
        public async Task ExternalMethod_CausesDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(17, 22));

        [Theory]
        [EmbeddedFileData(@"Initializers\NonDbCrudOperations.cs")]
        public async Task NonDbCrudOperations_DoesntCauseDiagnostic(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual);
    }
}
