using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacUiAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacUiAttributes
{
	public class DacUiAttributesTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacUiAttributesAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacUiAttributesFix();

		[Theory]
		[EmbeddedFileData("Dac_Bad.cs")]
		public async Task Graph_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1094_DacShouldHaveUiAttribute.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData("Dac_Good_Hidden.cs")]
		public async Task GraphWithPXHiddenAttribute_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Dac_Good_CacheName.cs")]
		public async Task GraphWithPXCacheNameAttribute_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("PXMappedCacheExtension.cs")]
		public async Task PXMappedCacheExtension_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(
			"Dac_Bad.cs",
			"Dac_Good_Hidden.cs")]
		public async Task AddPXHiddenAttribute_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 0);

		[Theory]
		[EmbeddedFileData(
			"Dac_Bad.cs",
			"Dac_Good_CacheName.cs")]
		public async Task AddPXCacheNameAttribute_VerifyCodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, 1);
	}
}
