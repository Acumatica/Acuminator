using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

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
		public async Task PublicClass_WithoutDescription_ReportsDiagnistic(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(9, 15),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(13, 23),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(15, 16),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(17, 19),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(19, 14));

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs")]
		public async Task PublicClass_WithoutSummary_ReportsDiagnistic(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(10, 15),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(15, 23),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(18, 16),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(21, 19),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(24, 14));

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs")]
		public async Task PublicClass_WithEmptySummary_ReportsDiagnistic(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(12, 15),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(19, 23),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(24, 16),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(29, 19),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(34, 14));

		[Theory]
		[EmbeddedFileData("NonPublic.cs")]
		public async Task Non_PublicClass_WithoutDescription_DoesntReportDiagnistic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NonPX.cs")]
		public async Task NonPX_PublicClass_WithoutDescription_DoesntReportDiagnistic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Excluded.cs")]
		public async Task Excluded_PublicClass_WithoutDescription_DoesntReportDiagnistic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

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
	}
}
