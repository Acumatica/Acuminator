#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	public class SuppressionOnDacCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default
									.WithIsvSpecificAnalyzersEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismEnabled(),

				new ForbiddenFieldsInDacAnalyzer(),
				new ConstructorInDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressDiagnosticTestCodeFix();

		[Theory]
		[EmbeddedFileData(@"Dac\ForbiddenFieldsSuppressComment_Expected.cs")]
		public virtual Task DacForbiddenFields_Suppressed(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Dac\WithConstructor_Unsuppressed.cs")]
		public virtual Task DacWithConstructor_SuppressSomeCases(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 18, column: 10),
				Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 95, column: 10));

		[Theory]
		[EmbeddedFileData(@"Dac\ForbiddenFields.cs",
			@"Dac\ForbiddenFieldsSuppressComment_Expected.cs")]
		public virtual Task DacWithForbidden_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Dac\WithConstructor.cs",
			@"Dac\WithConstructorSuppressComment_Expected.cs")]
		public virtual Task DacWithConstructor_SuppressComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}