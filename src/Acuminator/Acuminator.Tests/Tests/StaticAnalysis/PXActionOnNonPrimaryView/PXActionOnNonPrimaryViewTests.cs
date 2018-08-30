using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXActionOnNonPrimaryView;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionOnNonPrimaryView
{
	public class PXActionOnNonPrimaryViewTests : Verification.CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("GraphWithNonPrimaryDacView.cs")] 
		public virtual void Test_Diagnostic_For_Graph_And_Graph_Extension(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 23, column: 10, actionName: "Release1", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 25, column: 10, actionName: "Release2", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 34, column: 10, actionName: "Action1", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 38, column: 10, actionName: "Action3", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 47, column: 10, actionName: "Release1", mainDacName: "SOOrder"),
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 49, column: 10, actionName: "Release2", mainDacName: "SOOrder"));

		[Theory]
		[EmbeddedFileData("DerivedGraphWithBaseGraphPrimaryDac.cs")]
		public virtual void Test_Diagnostic_For_Derived_Graph(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1012ActionOnNonPrimaryViewDiagnosticResult(line: 26, column: 10, actionName: "Release1", mainDacName: "SOOrder"));

		[Theory]
		[EmbeddedFileData("GraphWithNonPrimaryDacView.cs",
						  "GraphWithNonPrimaryDacView_Expected.cs")]
		public void Test_Code_Fix_For_Graph_And_Graph_Extension(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("DerivedGraphWithBaseGraphPrimaryDac.cs",
						  "DerivedGraphWithBaseGraphPrimaryDac_Expected.cs")]
		public void Test_Code_Fix_For_Derived_Graph(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXActionOnNonPrimaryViewAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXActionOnNonPrimaryViewFix();

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
