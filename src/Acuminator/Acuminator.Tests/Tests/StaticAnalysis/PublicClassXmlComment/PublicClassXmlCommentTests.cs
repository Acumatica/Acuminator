using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

using Resources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.PublicClassXmlComment
{
	public class PublicClassXmlCommentTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PublicClassXmlCommentAnalyzer(
				CodeAnalysisSettings.Default
				.WithStaticAnalysisEnabled()
				.WithSuppressionMechanismDisabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PublicClassXmlCommentFix();

		[Theory]
		[EmbeddedFileData("WithoutDescription.cs")]
		public async Task PublicClass_WithoutDescription(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(9, 15, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(13, 23, messageArgs: nameof(Resources.PX1007Delegate).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(15, 16, messageArgs: nameof(Resources.PX1007Struct).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(17, 19, messageArgs: nameof(Resources.PX1007Interface).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(19, 14, messageArgs: nameof(Resources.PX1007Enum).GetLocalized()));

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs")]
		public async Task PublicClass_WithoutSummary(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(10, 15, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(15, 23, messageArgs: nameof(Resources.PX1007Delegate).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(18, 16, messageArgs: nameof(Resources.PX1007Struct).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(21, 19, messageArgs: nameof(Resources.PX1007Interface).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(24, 14, messageArgs: nameof(Resources.PX1007Enum).GetLocalized()));

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs")]
		public async Task PublicClass_WithEmptySummary(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(12, 15, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(19, 23, messageArgs: nameof(Resources.PX1007Delegate).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(24, 16, messageArgs: nameof(Resources.PX1007Struct).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(29, 19, messageArgs: nameof(Resources.PX1007Interface).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(34, 14, messageArgs: nameof(Resources.PX1007Enum).GetLocalized()));

		[Theory]
		[EmbeddedFileData("NonPublic.cs")]
		public async Task Non_PublicClass_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Excluded.cs")]
		public async Task Excluded_PublicClass_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("ExcludedWithNested.cs")]
		public async Task ExcludedWithNested_PublicClasses_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(27, 15, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(35, 16, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(37, 20, messageArgs: nameof(Resources.PX1007Interface).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(42, 17, messageArgs: nameof(Resources.PX1007Struct).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(45, 15, messageArgs: nameof(Resources.PX1007Enum).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(47, 24, messageArgs: nameof(Resources.PX1007Delegate).GetLocalized()));

		[Theory]
		[EmbeddedFileData("WithoutDescription.cs", "WithoutDescription_AddDescription.cs")]
		public async Task NoXmlComment_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs", "WithoutSummary_AddDescription.cs")]
		public async Task NoSummaryTag_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs", "WithEmptySummary_AddDescription.cs")]
		public async Task EmptySummaryTag_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("WithoutDescription.cs", "WithoutDescription_Exclude.cs")]
		public async Task NoXmlComment_Exclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs", "WithoutSummary_Exclude.cs")]
		public async Task NoSummaryTag_Exclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs", "WithEmptySummary_Exclude.cs")]
		public async Task EmptySummaryTag_Exclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		[Theory]
		[EmbeddedFileData("DAC.cs")]
		public async Task PublicDac_WithoutDescription(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(17, 25, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(22, 17, messageArgs: nameof(Resources.PX1007DacProperty).GetLocalized()),

				Descriptors.PX1007_PublicClassXmlComment.CreateFor(26, 25, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(32, 17, messageArgs: nameof(Resources.PX1007DacProperty).GetLocalized()),

				Descriptors.PX1007_PublicClassXmlComment.CreateFor(36, 25, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(43, 17, messageArgs: nameof(Resources.PX1007DacProperty).GetLocalized()));

		[Theory]
		[EmbeddedFileData("DAC_AddDescription.cs")]
		public async Task PublicDac_WithDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_AddExclude.cs")]
		public async Task PublicDac_WithExclude_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC.cs", "DAC_AddDescription.cs")]
		public async Task Dac_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("DAC.cs", "DAC_AddExclude.cs")]
		public async Task Dac_AddExclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);
	}
}
