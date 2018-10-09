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
                .WithIsvSpecificAnalyzersEnabled(),
                new DatabaseQueriesInPXGraphInitializationAnalyzer());

        [Theory]
        [EmbeddedFileData(@"Initializers\PXGraphConstructor.cs")]
        public async Task PXGraphConstructor(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\CreateInstanceDelegate.cs")]
        public async Task CreateInstanceDelegate(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\BQLSelect.cs")]
        public async Task BQLSelect(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\BQLSearch.cs")]
        public async Task BQLSearch(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\DataView.cs")]
        public async Task DataView(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\PXView.cs")]
        public async Task PXView(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\PXSelector.cs")]
        public async Task PXSelector(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\PXDatabase.cs")]
        public async Task PXDatabase(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\ExternalMethod.cs")]
        public async Task ExternalMethod(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));

        [Theory]
        [EmbeddedFileData(@"Initializers\NonDbCrudOperations.cs")]
        public async Task NonDbCrudOperations(string actual) =>
            await VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1085_DatabaseQueriesInPXGraphInitialization.CreateFor(1, 1));
    }
}
