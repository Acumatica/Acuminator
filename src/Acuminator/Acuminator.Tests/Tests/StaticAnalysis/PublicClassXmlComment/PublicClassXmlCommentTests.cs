#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment;
using Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

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
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(8, 15),
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(16, 15));

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs")]
		public async Task PublicClass_WithoutSummary(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(9, 15));

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs")]
		public async Task PublicClass_WithEmptySummary(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(15, 15),
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(24, 15));

		[Theory]
		[EmbeddedFileData("NonPublic.cs")]
		public async Task Non_PublicClass_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("ShouldNotShow.cs")]
		public async Task NonDac_Types_DontReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_Excluded.cs")]
		public async Task Excluded_PublicClass_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_ExcludedWithNested.cs")]
		public async Task ExcludedWithNested_PublicClasses_WithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(18, 15),
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(27, 16),
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(39, 16));

		[Theory]
		[EmbeddedFileData("WithoutDescription.cs", "WithoutDescription_AddDescription.cs")]
		public async Task NoXmlComment_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("WithoutDescription.cs", "WithoutDescription_Exclude.cs")]
		public async Task NoXmlComment_Exclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs", "WithoutSummary_AddDescription.cs")]
		public async Task NoSummaryOrInheritdocTag_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("WithoutSummary.cs", "WithoutSummary_Exclude.cs")]
		public async Task NoSummaryOrInheritdocTag_Exclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs", "WithEmptySummary_AddDescription.cs")]
		public async Task EmptySummaryTag_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("WithEmptySummary.cs", "WithEmptySummary_Exclude.cs")]
		public async Task EmptySummaryTag_Exclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		[Theory]
		[EmbeddedFileData("DAC.cs")]
		public async Task PublicDac_WithoutDescription(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(22, 17),
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(32, 17),
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(43, 17));

		[Theory]
		[EmbeddedFileData("DAC_Hidden_Obsolete_InternalUse.cs")]
		public async Task PublicDACs_With_Hidden_Obsolete_PXInternalUseOnly_Attributes_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacExtension_Hidden_Obsolete_InternalUse.cs")]
		public async Task PublicDacExtensions_With_Hidden_Obsolete_PXInternalUseOnly_Attributes(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
					Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(13, 22),
					Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(23, 17));

		[Theory]
		[EmbeddedFileData("DAC_System_Fields.cs")]
		public async Task PublicDac_WithSystemFields_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DacExtension_System_Fields.cs")]
		public async Task PublicDacExtension_WithSystemFields_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC.cs", "DAC_AddDescription.cs")]
		public async Task Dac_AddDescription_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData("DAC.cs", "DAC_AddExclude.cs")]
		public async Task Dac_AddExclude_Works(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 1);

		#region Inheritdoc tests
		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_NoInheritdoc.cs")]
		public async Task ProjectionDac_NoInheritdoc(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(24, 23),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(35, 25),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(46, 23),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(60, 28)
				);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_NoInheritdoc.cs", @"Inheritdoc\ProjectionDAC_NoInheritdoc_AddInheritdoc.cs")]
		public async Task ProjectionDac_NoInheritdoc_AddInheritdocTag(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDacExtension_NoInheritdoc.cs")]
		public async Task ProjectionDacExtension_NoInheritdoc(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(21, 15),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(32, 17),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(43, 15),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(57, 20)
				);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDacExtension_NoInheritdoc.cs",
						  @"Inheritdoc\ProjectionDacExtension_NoInheritdoc_AddInheritdoc.cs")]
		public async Task ProjectionDacExtension_NoInheritdoc_AddInheritdocTag(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_EmptyInheritdoc.cs")]
		public async Task ProjectionDac_EmptyInheritdocTags(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(34, 25));

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_EmptyInheritdoc.cs", @"Inheritdoc\ProjectionDAC_EmptyInheritdoc_FixInheritdoc.cs")]
		public async Task ProjectionDac_EmptyInheritdocTags_FixInheritdocTag(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_InheritdocWithWrongReference.cs")]
		public async Task ProjectionDac_InhertidocWithWrongReference(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(25, 23),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(33, 25),
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.CreateFor(45, 25));

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_InheritdocWithWrongReference.cs",
						  @"Inheritdoc\ProjectionDAC_InheritdocWithWrongReference_FixInheritdoc.cs")]
		public async Task ProjectionDac_InhertidocWithWrongReference_FixInheritdocTag(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected, codeFixIndex: 0);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_NotMappedAndGoodFields.cs")]
		public async Task ProjectionDAC_NotMapped_Described_Excluded_Fields(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
					Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(26, 24));

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_SystemFields.cs")]
		public async Task ProjectionDAC_SystemFields_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\NonProjectionDAC_Inheritdoc.cs")]
		public async Task NonProjectionDAC_WithInhertitdocTags_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion

		#region Partial class tests		
		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs")]
		public async Task PublicPartialHelper_SingleWithoutDescription(string source) =>
			await VerifyCSharpDiagnosticAsync(
				source,
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(10, 23));

		[Theory]
		[EmbeddedFileData(@"Partial\WithoutComment.cs", @"Partial\WithBadComment.cs")]
		public async Task PublicPartialHelper_BadCommentOnOtherDeclaration(string source, string badCommentSource) =>
			await VerifyCSharpDiagnosticAsync(
				source, badCommentSource,
				Descriptors.PX1007_PublicClassNoXmlComment.CreateFor(10, 23));

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

		#region Generated tags tests
		[Theory]
		[EmbeddedFileData("WithoutDescription_AddDescription.cs")]
		public async Task GeneratedDescription_ForApiWithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("WithoutDescription_Exclude.cs")]
		public async Task GeneratedExclude_ForApiWithoutDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("WithEmptySummary_AddDescription.cs")]
		public async Task GeneratedDescription_ForApiWithEmptySummary_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("WithEmptySummary_Exclude.cs")]
		public async Task GeneratedExclude_ForApiWithEmptySummary_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("WithoutSummary_AddDescription.cs")]
		public async Task GeneratedDescription_ForApiWithoutSummaryOrInheritdocTag_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("WithoutSummary_Exclude.cs")]
		public async Task GeneratedExclude_ForApiWithoutSummaryOrInheritdocTag_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_AddExclude.cs")]
		public async Task PublicDac_WithExclude_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("DAC_AddDescription.cs")]
		public async Task PublicDac_WithDescription_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_EmptyInheritdoc_FixInheritdoc.cs")]
		public async Task ProjectionDac_WithEmptyInheritdocs_AfterCodeFix_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_InheritdocWithWrongReference_FixInheritdoc.cs")]
		public async Task ProjectionDac_WithInheritdocWithWrongReference_AfterCodeFix_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
			
		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDAC_NoInheritdoc_AddInheritdoc.cs")]
		public async Task ProjectionDac_NoInheritdoc_AfterCodeFix_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"Inheritdoc\ProjectionDacExtension_NoInheritdoc_AddInheritdoc.cs")]
		public async Task ProjectionDacExtension_NoInheritdoc_AfterCodeFix_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
		#endregion
	}
}
