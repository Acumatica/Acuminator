using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using TestHelper;
using Microsoft.CodeAnalysis;
using Xunit;
using Acuminator.Tests.Helpers;

namespace Acuminator.Tests
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
        [EmbeddedFileData(@"Dac\PX1031 and PX1032\Diagnostics\DacWithMethodsUsage.cs")]
        public void TestDiagnostic_Dac(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1032DiagnosticResult(23, 17),
                CreatePX1032DiagnosticResult(24, 17),
                CreatePX1032DiagnosticResult(26, 26),
                CreatePX1031DiagnosticResult(47, 32));
        }

        [Theory]
        [EmbeddedFileData(@"Dac\PX1031 and PX1032\Diagnostics\DacExtensionWithMethodsUsage.cs")]
        public void TestDiagnostic_CacheExtension(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1032DiagnosticResult(27, 17),
                CreatePX1032DiagnosticResult(28, 17),
                CreatePX1032DiagnosticResult(30, 26),
                CreatePX1031DiagnosticResult(51, 32));
        }

	    [Theory]
	    [EmbeddedFileData(@"Dac\PX1031 and PX1032\Diagnostics\DacWithNestedTypes.cs")]
	    public void TestDiagnostic_DacWithNestedTypes_ShouldNotShowDiagnostic(string source)
	    {
		    VerifyCSharpDiagnostic(source);
	    }
	}
}
