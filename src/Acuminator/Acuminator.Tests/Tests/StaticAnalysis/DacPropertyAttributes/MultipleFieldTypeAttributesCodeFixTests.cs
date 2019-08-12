using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacPropertyAttributes
{
	public class MultipleFieldTypeAttributesCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacPropertyAttributesAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MultipleFieldTypeAttributesOnDacPropertyFix();

		[Theory]
		[EmbeddedFileData("DacWithMultipleFieldTypeAttributes.cs",
						  "DacWithMultipleFieldTypeAttributes_Expected.cs")]
		public void DacProperty_CodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
