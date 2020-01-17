using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	public class SuppressionOnMissingXmlCommentCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PublicClassXmlCommentAnalyzer(
				CodeAnalysisSettings.Default
									.WithIsvSpecificAnalyzersEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismEnabled()
									.WithPX1007DocumentationDiagnosticEnabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressDiagnosticTestCodeFix();

		[Theory]
		[EmbeddedFileData(@"Dac\DAC_Without_XML_Description.cs", @"Dac\DAC_Without_XML_Description_Expected.cs")]
		public virtual void DacWithoutXMLDescription_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}