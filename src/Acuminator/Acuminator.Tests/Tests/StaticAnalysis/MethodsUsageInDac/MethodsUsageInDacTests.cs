using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MethodsUsageInDac
{
    public class MethodsUsageInDacTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new MethodsUsageInDacAnalyzer();

        private DiagnosticResult CreatePX1031DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1031_DacCannotContainInstanceMethods.Id,
                Message = Descriptors.PX1031_DacCannotContainInstanceMethods.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        private DiagnosticResult CreatePX1032DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.Id,
                Message = Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData("DacWithMethodsUsage.cs")]
        public void TestDiagnostic_Dac(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1032DiagnosticResult(23, 17),
                CreatePX1032DiagnosticResult(24, 17),
                CreatePX1032DiagnosticResult(26, 26),
                CreatePX1031DiagnosticResult(47, 32));
        }

        [Theory]
        [EmbeddedFileData("DacExtensionWithMethodsUsage.cs")]
        public void TestDiagnostic_CacheExtension(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1032DiagnosticResult(27, 17),
                CreatePX1032DiagnosticResult(28, 17),
                CreatePX1032DiagnosticResult(30, 26),
                CreatePX1031DiagnosticResult(51, 32));
        }

	    [Theory]
	    [EmbeddedFileData("DacWithNestedTypes.cs")]
	    public void TestDiagnostic_DacWithNestedTypes_ShouldNotShowDiagnostic(string source)
	    {
		    VerifyCSharpDiagnostic(source);
	    }
	}
}
