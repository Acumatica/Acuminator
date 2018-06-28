using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;
using Acuminator.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;

namespace Acuminator.Tests
{
	public class PXActionOnNonPrimaryViewTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"PXAction\Diagnostics\PX1012\GraphWithNonPrimaryDacView.cs")] 
		public virtual void Test_Action_Declared_For_Not_Primary_View(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 23, column: 10, actionName: "Release1", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 25, column: 10, actionName: "Release2", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 34, column: 10, actionName: "Action1", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 38, column: 10, actionName: "Action3", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 47, column: 10, actionName: "Release1", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 49, column: 10, actionName: "Release2", mainDacName: "SOOrder"));

		[Theory]
		[EmbeddedFileData(@"PXAction\Diagnostics\PX1012\GraphWithNonPrimaryDacView.cs",
						  @"PXAction\CodeFixes\PX1012\GraphWithNonPrimaryDacViewExpected.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXActionOnNonPrimaryViewAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXActionOnNonPrimaryViewCodeFix();

		private DiagnosticResult CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(int line, int column, string actionName, 
																					string mainDacName)
		{
			string format = Descriptors.PX1012_PXActionOnNonPrimaryView.Title.ToString();
			string expectedMessage = string.Format(format, actionName, mainDacName);

			return new DiagnosticResult
			{
				Id = Descriptors.PX1012_PXActionOnNonPrimaryView.Id,
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
