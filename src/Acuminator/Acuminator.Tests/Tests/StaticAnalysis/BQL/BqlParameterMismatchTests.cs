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
	public class BqlParameterMismatchTests : DiagnosticVerifier
	{
		[Theory]
		[EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\StaticCall.cs")]
		public virtual void Test_Static_Calls(string source) =>
			VerifyCSharpDiagnostic(source, 
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 20, column: 6, expectedMethodName: "SelectSingleBound", expectedArgsCount: 2),
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 33, column: 6, expectedMethodName: "SelectSingleBound", expectedArgsCount: 2),
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 47, column: 6, expectedMethodName: "SelectSingleBound", expectedArgsCount: 2),
				CreatePX1015RequiredAndOptionalArgsDiagnosticResult(line: 62, column: 6, expectedMethodName: "SelectSingleBound",
																	minExpectedArgsCount: 1, maxExpectedArgsCount: 2));

		[Theory]
		[EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\StaticCallWithCustomPredicate.cs")]
		public virtual void Test_Static_Call_With_Custom_Predicates(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 28, column: 6, expectedMethodName: "SelectSingleBound", expectedArgsCount: 4));

		[Theory]
		[EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\InheritanceCall.cs")]
		public virtual void Test_Inheritance_Calls_Instance_And_Static(string source) => VerifyCSharpDiagnostic(source,
			CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 28, column: 31, expectedMethodName: "Select", expectedArgsCount: 2));

		[Theory]
		[EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\FieldInstanceCall.cs", @"Dac\SOOrder.cs")]
		public virtual void Test_Field_Instance_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 20, column: 24, expectedMethodName: "SelectSingle", expectedArgsCount: 2));

		[Theory]
		[EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\SearchCall.cs", @"Dac\SOOrder.cs")]
		public virtual void Test_Search_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 22, column: 24, expectedMethodName: "Search", expectedArgsCount: 1),
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 24, column: 24, expectedMethodName: "Search", expectedArgsCount: 3),
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 34, column: 7, expectedMethodName: "Search", expectedArgsCount: 1),
				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 46, column: 6, expectedMethodName: "Search", expectedArgsCount: 3));

		[Theory]
		[EmbeddedFileData(@"BQL\Diagnostics\ArgumentsMismatch\VariableInstanceCall.cs", @"Dac\SOOrder.cs")]
		public virtual void Test_Variable_Instance_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				CreatePX1015RequiredAndOptionalArgsDiagnosticResult(line: 24, column: 27, expectedMethodName: "Select",
																	minExpectedArgsCount: 1, maxExpectedArgsCount: 2),

				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 38, column: 27, expectedMethodName: "Select", expectedArgsCount: 1),

				CreatePX1015RequiredArgsOnlyDiagnosticResult(line: 57, column: 54, expectedMethodName: "Select", expectedArgsCount: 2));


		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new BqlParameterMismatchAnalyzer();

		private DiagnosticResult CreatePX1015RequiredArgsOnlyDiagnosticResult(int line, int column, string expectedMethodName,
																			  int expectedArgsCount)
		{
			string format = Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.Title.ToString();
			string expectedMessage = string.Format(format, expectedMethodName, expectedArgsCount);
			return CreatePX1015DiagnosticResult(line, column, expectedMessage);
		}

		private DiagnosticResult CreatePX1015RequiredAndOptionalArgsDiagnosticResult(int line, int column, string expectedMethodName,
																					 int minExpectedArgsCount, int maxExpectedArgsCount)
		{
			string format = Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.Title.ToString();
			string expectedMessage = string.Format(format, expectedMethodName, minExpectedArgsCount, maxExpectedArgsCount);
			return CreatePX1015DiagnosticResult(line, column, expectedMessage);
		}

		private DiagnosticResult CreatePX1015DiagnosticResult(int line, int column, string expectedMessage)
		{
			return new DiagnosticResult
			{
				Id = Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.Id,
				Message = expectedMessage,
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
