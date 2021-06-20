using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphDeclarationTypeParameter;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphDeclarationTypeParameter
{
	public class PXGraphDeclarationTypeParameterTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new PXGraphDeclarationTypeParameterAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXGraphDeclarationTypeParameterFix();

		[Theory]
		[EmbeddedFileData("Graph_Bad.cs")]
		public async Task Graph_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1093_GraphDeclarationViolation.CreateFor(11, 35),
				Descriptors.PX1093_GraphDeclarationViolation.CreateFor(15, 39),
				Descriptors.PX1093_GraphDeclarationViolation.CreateFor(19, 33));

		[Theory]
		[EmbeddedFileData("Graph_Good.cs")]
		public async Task Graph_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(
			"Graph_Bad.cs",
			"Graph_Good.cs")]
		public async Task CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
