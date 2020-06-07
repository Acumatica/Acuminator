using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	public class SuppressionOnMissingIsActiveMethodCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
					CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
												.WithSuppressionMechanismEnabled(),
					new NoIsActiveMethodForExtensionAnalyzer());
		
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressDiagnosticTestCodeFix();

		[Theory]
		[EmbeddedFileData(@"Dac\DacExtension_WithoutIsActive.cs", @"Dac\DacExtension_WithoutIsActive_Expected.cs")]
		public virtual void DacExtension_WithoutIsActiveMethod_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}