using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacDeclaration;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class DacWithConstructorTests : CodeFixVerifier
    {
        [Theory]
        [EmbeddedFileData(@"Dac\PX1028\Diagnostics\DacWithConstructor.cs")]
        public virtual void TestDacWithConstructor(string source) =>
            VerifyCSharpDiagnostic(source,
                CreatePX1028DacConstructorDiagnosticResult(line: 13, column: 16),
                CreatePX1028DacConstructorDiagnosticResult(line: 17, column: 16),
                CreatePX1028DacConstructorDiagnosticResult(line: 74, column: 16),
                CreatePX1028DacConstructorDiagnosticResult(line: 88, column: 16),
                CreatePX1028DacConstructorDiagnosticResult(line: 92, column: 16));

        [Theory]
        [EmbeddedFileData(@"Dac\PX1028\Diagnostics\DacWithConstructor.cs",
                          @"Dac\PX1028\CodeFixes\DacWithConstructor_Expected.cs")]
        public virtual void TestCodeFixDacWithConstructor(string actual, string expected) =>
            VerifyCSharpFix(actual,expected);

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConstructorInDacFix();

        private DiagnosticResult CreatePX1028DacConstructorDiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1028_ConstructorInDacDeclaration.Id,
                Message = Descriptors.PX1028_ConstructorInDacDeclaration.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }
    }
}
