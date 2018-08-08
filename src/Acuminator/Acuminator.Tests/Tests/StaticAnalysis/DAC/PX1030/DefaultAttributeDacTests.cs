using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Acuminator.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace Acuminator.Tests
{
	public class DefaultAttributeInDacTests : CodeFixVerifier
	{
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacExtensionDefaultAttributeAnalyzer();

        [Theory]
        [EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacExtensionWithBoundFields.cs")]
        public virtual void TestDacExtensionWithDefaultAttribute(string source) =>
            VerifyCSharpDiagnostic(source,
                CreatePX1030DiagnosticResult(line: 23, column: 4),
				CreatePX1030DiagnosticResult(line: 30, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacExtensionWithUnboundFields.cs")]
		public virtual void TestDacExtensionWithUnboundFields(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 23, column: 4),
				CreatePX1030DiagnosticResult(line: 30, column: 4));
        private DiagnosticResult CreatePX1030DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Id,
                Message = Descriptors.PX1030_DefaultAttibuteToExisitingRecords.Title.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", line, column)
                    }
            };
        }

        //        protected override CodeFixProvider GetCSharpCodeFixProvider() => new UnderscoresInDacCodeFix();

        /*
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new UnderscoresInDacCodeFix();

		*/
    }
}
