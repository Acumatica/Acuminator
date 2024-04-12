﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ForbiddenFieldsInDac
{
	public class DacForbiddenFieldsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithIsvSpecificAnalyzersEnabled()
											.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new ForbiddenFieldsInDacAnalyzer());

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
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(42, 17, "CompanyMask"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(45, 25, "notes"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(48, 17, "Notes"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(51, 25, "files"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(54, 17, "Files"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(57, 25, "databaseRecordStatus"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(60, 15, "DatabaseRecordStatus"));

		[Theory]
		[EmbeddedFileData("DacFieldsWithCompanyPrefix.cs")]
		public virtual void DacField_WithCompanyPrefix(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName.CreateFor(11, 25),
				Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName.CreateFor(15, 17),
				Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName.CreateFor(26, 25),
				Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName.CreateFor(30, 17));

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
	}
}