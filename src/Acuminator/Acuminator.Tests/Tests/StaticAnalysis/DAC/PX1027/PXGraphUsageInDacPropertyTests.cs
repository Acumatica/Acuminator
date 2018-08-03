using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class PXGraphUsageInDacPropertyTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacPropertyGraphUsageAnalyzer();

        private DiagnosticResult CreatePX1027DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1027_PXGraphUsageInDacProperty.Id,
                Message = Descriptors.PX1027_PXGraphUsageInDacProperty.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacWithGraphUsageInProperty.cs")]
        public void Test_PXGraph_Usage_Inside_Dac_Property(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1027DiagnosticResult(13, 17),
                CreatePX1027DiagnosticResult(13, 50),
                CreatePX1027DiagnosticResult(15, 24),
                CreatePX1027DiagnosticResult(25, 24),
                CreatePX1027DiagnosticResult(25, 42),
                CreatePX1027DiagnosticResult(35, 24));
        }

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacExtensionWithGraphUsageInProperty.cs")]
        public void Test_PXGraph_Usage_Inside_CacheExtension_Property(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1027DiagnosticResult(13, 17),
                CreatePX1027DiagnosticResult(13, 50),
                CreatePX1027DiagnosticResult(15, 24),
                CreatePX1027DiagnosticResult(25, 24),
                CreatePX1027DiagnosticResult(25, 42),
                CreatePX1027DiagnosticResult(35, 24));
        }
    }
}
