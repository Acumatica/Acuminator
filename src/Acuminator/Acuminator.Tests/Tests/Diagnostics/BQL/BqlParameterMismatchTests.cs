using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	public class BqlParameterMismatchTests : CodeFixVerifier
	{
        private DiagnosticResult CreatePX1015DiagnosticResult(int line, int column)
        {
            var diagnostic = new DiagnosticResult
            {
                Id = Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.Id,
                Message = Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", line, column)
                    }
            };

            return diagnostic;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new BqlParameterMismatchAnalyzer();
       
        [Theory]
        [EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\StaticCall.cs")]
        public virtual void FormatDocument(string actual) =>
            VerifyCSharpDiagnostic(actual, CreatePX1015DiagnosticResult(line: 20, column: 6));     
    }
}
