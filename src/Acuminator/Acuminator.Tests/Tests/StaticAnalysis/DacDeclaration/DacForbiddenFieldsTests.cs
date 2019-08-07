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
	public class DacForbiddenFieldsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacDeclarationAnalyzer(CodeAnalysisSettings.Default
				.WithIsvSpecificAnalyzersEnabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new ForbiddenFieldsInDacFix();

		[Theory]
		[EmbeddedFileData("DacForbiddenFields.cs")]
		public virtual void TestDacWithForbiddenFields(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(13, 25, "companyId"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(17, 17, "CompanyID"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(27, 25, "deletedDatabaseRecord"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(30, 17, "DeletedDatabaseRecord"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(39, 25, "companyMask"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(42, 17, "CompanyMask"));

		[Theory]
		[EmbeddedFileData("DacForbiddenFields.cs",
			"DacForbiddenFields_Expected.cs")]
		public virtual void TestFixForDacWithForbiddenFields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacForbiddenFieldsWithoutRegions.cs",
			"DacForbiddenFieldsWithoutRegions_Expected.cs")]
		public virtual void TestFixForDacWithForbiddenFieldWithoutRegions(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacForbiddenFieldsRegions_Case1.cs",
			"DacForbiddenFieldsRegions_Case1_Expected.cs")]
		public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case1(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacForbiddenFieldsRegions_Case2.cs",
			"DacForbiddenFieldsRegions_Case2_Expected.cs")]
		public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case2(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacForbiddenFieldsRegions_Case3.cs",
			"DacForbiddenFieldsRegions_Case3_Expected.cs")]
		public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case3(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacForbiddenFields.cs",
			"DacForbiddenFieldsSuppressComment_Expected.cs")]
		public virtual void TestFixForDacWithForbidden_SuppressComment(string actual, string expected) =>
			VerifyCSharpFix(actual, expected,codeFixIndex: 1);
	}
}