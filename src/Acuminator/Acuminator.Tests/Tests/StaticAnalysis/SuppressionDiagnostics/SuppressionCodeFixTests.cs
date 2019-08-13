using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionCommentCodeFix
{
	public class SuppressionCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default
				.WithIsvSpecificAnalyzersEnabled(),
				new ForbiddenFieldsInDacAnalyzer(),
				new ConstructorInDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressCommentFix();

		[Theory]
		[EmbeddedFileData("DacForbiddenFields.cs",
			"DacForbiddenFieldsSuppressComment_Expected.cs")]
		public virtual void DacWithForbidden_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacWithConstructor.cs",
			"DacWithConstructorSuppressComment_Expected.cs")]
		public virtual void DacWithConstructor_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}