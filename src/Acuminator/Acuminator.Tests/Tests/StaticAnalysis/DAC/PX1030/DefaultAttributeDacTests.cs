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
using Acuminator.Utilities;

namespace Acuminator.Tests
{
	public class DefaultAttributeInDacTests : CodeFixVerifier
	{
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacExtensionDefaultAttributeAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacExtensionDefaultAttributeFix();

		[Theory]
        [EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacExtensionWithBoundFields.cs")]
        public virtual void TestDacExtensionWithBoundAttribute(string source) =>
            VerifyCSharpDiagnostic(source,
                CreatePX1030DiagnosticResult(line: 23, column: 4),
				CreatePX1030DiagnosticResult(line: 30, column: 4),
				CreatePX1030DiagnosticResult(line: 44, column: 4),
				CreatePX1030DiagnosticResult(line: 50, column: 13),
				CreatePX1030DiagnosticResult(line: 56, column: 13),
				CreatePX1030DiagnosticResult(line: 62, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacExtensionWithUnboundFields.cs")]
		public virtual void TestDacExtensionWithUnboundFields(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 23, column: 4),
				CreatePX1030DiagnosticResult(line: 30, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacWithBoundAndUnboundFields.cs")]
		public virtual void TestDacWithBoundAndUnboundAttribute(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\AggregateAttributeFields.cs")]
		public virtual void TestDacWithAggregateAttributeFields(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1030DiagnosticResult(line: 36, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\AggregateAttributeFields.cs",
							@"Dac\PX1030\CodeFixes\AggregateAttributeFields_Expected.cs")]
		public virtual void TestCodeFixDacWithAggregateAttributeFields(string actual,string expected) =>
			VerifyCSharpFix(actual,expected);

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacExtensionWithBoundFields.cs",
							@"Dac\PX1030\CodeFixes\DacExtensionWithBoundFields_Expected.cs")]
		public virtual void TestCodeFixDacExtensionWithBoundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Dac\PX1030\Diagnostics\DacExtensionWithUnboundFields.cs",
							@"Dac\PX1030\CodeFixes\DacExtensionWithUnboundFields_Expected.cs")]
		public virtual void TestCodeFixDacExtensionWithUnboundAttribute(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
		
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
    }
}
