using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ActionHandlerReturnType;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ActionHandlerReturnType
{
    public class ActionHandlerReturnTypeTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new PXGraphAnalyzer(CodeAnalysisSettings.Default, new ActionHandlerReturnTypeAnalyzer());

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ActionHandlerReturnTypeFix();

        [Theory]
        [EmbeddedFileData("Handlers_Bad.cs")]
        public async Task Handler_ReportsDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source, Descriptors.PX1013_PXActionHandlerInvalidReturnType.CreateFor(15, 21));

        [Theory]
        [EmbeddedFileData("Handlers_Good.cs")]
        public async Task Handler_DoesntReportDiagnostic(string source) =>
            await VerifyCSharpDiagnosticAsync(source);

        [Theory]
        [EmbeddedFileData(
            "Handlers_Bad.cs",
            "Handlers_Good.cs")]
        public async Task CodeFix(string actual, string expected) =>
            await VerifyCSharpFixAsync(actual, expected);
    }
}
