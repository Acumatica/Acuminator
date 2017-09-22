using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Analyzers.Analyzers;
using PX.Analyzers.FixProviders;
using PX.Analyzers.Test.Helpers;
using TestHelper;
using Xunit;

namespace PX.Analyzers.Test
{
    public class DACCreateInstanceTests : CodeFixVerifier
    {
	    private DiagnosticResult CreatePX1007DiagnosticResult(int line, int column)
	    {
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1007_DACCreateInstance.Id,
				Message = String.Format(Descriptors.PX1007_DACCreateInstance.MessageFormat.ToString(), "APInvoice"),
				Severity = Descriptors.PX1007_DACCreateInstance.DefaultSeverity,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

		    return diagnostic;
	    }

		[Theory]
        [EmbeddedFileData("DACCreateInstance.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreatePX1007DiagnosticResult(14, 24));
        }
		
	    [Theory]
	    [EmbeddedFileData("DACCreateInstance.cs", "DACCreateInstance_Expected.cs")]
	    public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new DACCreateInstanceFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new DACCreateInstanceAnalyzer();
        }
    }
}
