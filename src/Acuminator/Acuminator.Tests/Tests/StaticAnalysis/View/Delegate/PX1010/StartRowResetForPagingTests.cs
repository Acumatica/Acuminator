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
    public class StartRowResetForPagingTests : CodeFixVerifier
    {
	    private DiagnosticResult[] CreateDiagnosticResults()
	    {
		    return new DiagnosticResult[] 
            {
                new DiagnosticResult
                {
                    Id = Descriptors.PX1010_StartRowResetForPaging.Id,
                    Message = Descriptors.PX1010_StartRowResetForPaging.Title.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 17, 44) }
                }
            };
	    }

        [Theory]
        [EmbeddedFileData(@"View\Delegate\PX1010\Diagnostics\StartRowResetForPaging.cs")] 
		public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreateDiagnosticResults());
        }

        [Theory]
        [EmbeddedFileData(@"View\Delegate\PX1010\Diagnostics\StartRowResetForPaging.cs",
						  @"View\Delegate\PX1010\CodeFixes\StartRowResetForPaging_Expected.cs")]
        public void TestCodeFix(string actual, string expected)
        {
            VerifyCSharpFix(actual, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new StartRowResetForPagingAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new StartRowResetForPagingFix();
        }
    }
}
