using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacUiAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacUiAttributes
{
	public class DacUiAttributesTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacUiAttributesAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacUiAttributesFix();

		[Theory]
		[EmbeddedFileData("Dac_Bad.cs")]
		public async Task Graph_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1094_DacShouldHaveUiAttribute.CreateFor(1, 1));

		[Theory]
		[EmbeddedFileData("Dac_Good.cs")]
		public async Task Graph_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(
			"Dac_Bad.cs",
			"Dac_Good.cs")]
		public async Task CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
