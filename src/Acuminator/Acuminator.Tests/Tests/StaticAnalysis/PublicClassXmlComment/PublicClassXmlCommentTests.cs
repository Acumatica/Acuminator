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
										.WithSuppressionMechanismDisabled()
										.WithPX1007DocumentationDiagnosticEnabled());

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
		[EmbeddedFileData("DAC_Excluded.cs")]
		public async Task Excluded_PublicClass_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_ExcludedWithNested.cs")]
		public async Task ExcludedWithNested_PublicClasses_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(18, 15, messageArgs: nameof(Resources.PX1007Class).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(27, 16, messageArgs: nameof(Resources.PX1007Class).GetLocalized()));

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
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(22, 17, messageArgs: nameof(Resources.PX1007DacProperty).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(32, 17, messageArgs: nameof(Resources.PX1007DacProperty).GetLocalized()),
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(43, 17, messageArgs: nameof(Resources.PX1007DacProperty).GetLocalized()));

		[Theory]
		[EmbeddedFileData("DAC_AddDescription.cs")]
		public async Task PublicDac_WithDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_Hidden_Obsolete_InternalUse.cs")]
		public async Task PublicDACs_With_Hidden_Obsolete_PXInternalUseOnly_Attributes_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_System_Fields.cs")]
		public async Task PublicDac_WithSystemFields_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacExtension_System_Fields.cs")]
		public async Task PublicDacExtension_WithSystemFields_DoesntReportDiagnostic(string source) =>
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

		#region Partial class tests		
		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs")]
		public async Task PublicPartialHelper_SingleWithoutDescription(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(10, 23, messageArgs: nameof(Resources.PX1007Class).GetLocalized()));

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithBadComment.cs")]
		public async Task PublicPartialHelper_BadCommentOnOtherDeclaration(string source, string badCommentSource) =>
			await VerifyCSharpDiagnosticAsync(
				source, badCommentSource,
				Descriptors.PX1007_PublicClassXmlComment.CreateFor(10, 23, messageArgs: nameof(Resources.PX1007Class).GetLocalized()));

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithComment.cs")]
		public async Task PublicPartialHelper_CommentOnOtherDeclaration_DoesntReportDiagnostic(string checkedSource, string partialDeclaration) =>
			await VerifyCSharpDiagnosticAsync(checkedSource, partialDeclaration);

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithExcludeComment.cs")]
		public async Task PublicPartialHelper_ExcludeOnOtherDeclaration_DoesntReportDiagnostic(string checkedSource, string partialDeclaration) =>
			await VerifyCSharpDiagnosticAsync(checkedSource, partialDeclaration);

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithComment.cs", @"Partial\WithExcludeComment.cs")]
		public async Task PublicPartialHelper_WithCommenAndExcludeOnOtherDeclarations_DoesntReportDiagnostic(string checkedSource, 
																											 string sourceWithComment, string sourceWithExclude) =>
			await VerifyCSharpDiagnosticAsync(checkedSource, sourceWithComment, sourceWithExclude);

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithBadComment.cs", @"Partial\WithExcludeComment.cs")]
		public async Task PublicPartialHelper_WithBadComment_ExcludeOnAnotherDeclaration_DoesntReportDiagnostic(string checkedSource, 
																												string badCommentSource, string sourceWithExclude) =>
			await VerifyCSharpDiagnosticAsync(checkedSource, badCommentSource, sourceWithExclude);

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithBadComment.cs", @"Partial\WithExcludeComment.cs")]
		public async Task PublicPartialHelper_WithBadComment_CommantOnAnotherDeclaration_DoesntReportDiagnostic(string checkedSource,
																												string badCommentSource, string sourceWithComment) =>
			await VerifyCSharpDiagnosticAsync(checkedSource, badCommentSource, sourceWithComment);
		#endregion
	}
}
