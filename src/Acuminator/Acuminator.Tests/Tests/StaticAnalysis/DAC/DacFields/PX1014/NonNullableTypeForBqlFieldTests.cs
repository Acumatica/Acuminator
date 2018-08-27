using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class NonNullableTypeForBqlFieldTests : CodeFixVerifier
    {
	    [Theory]
	    [EmbeddedFileData(@"Dac\PX1014\Diagnostics\NonNullableTypeForBqlField.cs")] 
		public void TestDiagnostic(string actual)
	    {
		    VerifyCSharpDiagnostic(actual, CreatePX1014DiagnosticResult(16, 14));
	    }

		[Theory]
        [EmbeddedFileData(@"Dac\PX1014\CodeFixes\NonNullableTypeForBqlField_Expected.cs")]
        public void TestDiagnostic_ShouldNotShowDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

	    [Theory]
	    [EmbeddedFileData(@"Dac\PX1014\Diagnostics\NonNullableTypeForBqlField_Valid.cs")]
	    public void TestDiagnostic_ShouldNotShowDiagnostic2(string actual)
	    {
		    VerifyCSharpDiagnostic(actual);
	    }

		[Theory]
	    [EmbeddedFileData(@"Dac\PX1014\Diagnostics\NonNullableTypeForBqlField.cs",
						  @"Dac\PX1014\CodeFixes\NonNullableTypeForBqlField_Expected.cs")]
	    public void TestCodeFix(string actual, string expected)
	    {
		    VerifyCSharpFix(actual, expected);
	    }

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new NonNullableTypeForBqlFieldFix();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NonNullableTypeForBqlFieldAnalyzer();
        }

		private DiagnosticResult CreatePX1014DiagnosticResult(int line, int column)
		{
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1014_NonNullableTypeForBqlField.Id,
				Message = Descriptors.PX1014_NonNullableTypeForBqlField.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

			return diagnostic;
		}
	}
}
