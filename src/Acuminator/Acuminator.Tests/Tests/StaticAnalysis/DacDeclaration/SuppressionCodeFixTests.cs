using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacDeclaration;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacDeclaration
{
	public class SuppressionCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacDeclarationAnalyzer(CodeAnalysisSettings.Default
				.WithIsvSpecificAnalyzersEnabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXCodeFixProvider();

		[Theory]
		[EmbeddedFileData("DacForbiddenFields.cs",
			"DacForbiddenFieldsSuppressComment_Expected.cs")]
		public virtual void TestFixForDacWithForbidden_SuppressComment(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithConstructor.cs",
			"DacWithConstructorSuppressComment_Expected.cs")]
		public virtual void DacWithConstructorSuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}