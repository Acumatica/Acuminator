﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Acuminator.Analyzers.StaticAnalysis.Dac;

namespace Acuminator.Tests.Tests.StaticAnalysis.ForbiddenFieldsInDac
{
	public class DacForbiddenFieldsNonISVTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithIsvSpecificAnalyzersDisabled()
											.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new ForbiddenFieldsInDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new ForbiddenFieldsInDacFix();

		[Theory]
		[EmbeddedFileData("DacForbiddenFields.cs")]
		public virtual void TestDacWithForbiddenFields(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(13, 25, "companyId"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(17, 17, "CompanyID"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV.CreateFor(27, 25, "deletedDatabaseRecord"),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV.CreateFor(30, 17, "DeletedDatabaseRecord"),
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
	}
}
