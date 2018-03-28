using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Acuminator.Tests
{
    public class MissingTypeListAttributeTests : CodeFixVerifier
    {
	    private DiagnosticResult CreateDiagnosticResult(int line, int column)
	    {
			var diagnostic = new DiagnosticResult
			{
				Id = Descriptors.PX1002_MissingTypeListAttributeAnalyzer.Id,
				Message = Descriptors.PX1002_MissingTypeListAttributeAnalyzer.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[] {
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};

		    return diagnostic;
	    }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeGood.cs")]
        public void TestDiagnostic_Good(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeInheritedList.cs")]
        public void TestDiagnostic_InheritedList_Good(string actual)
        {
            VerifyCSharpDiagnostic(actual);
        }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeBad.cs")]
        public void TestDiagnostic_Bad(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreateDiagnosticResult(14, 23));
        }

        [Theory]
        [EmbeddedFileData("MissingTypeListAttributeBad.cs", "MissingTypeListAttributeBad_Expected.cs")]
        public void TestCodeFix(string actual, string expected)
        {
            VerifyCSharpFix(actual, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new MissingTypeListAttributeAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new MissingTypeListAttributeFix();
        }
    }
}
