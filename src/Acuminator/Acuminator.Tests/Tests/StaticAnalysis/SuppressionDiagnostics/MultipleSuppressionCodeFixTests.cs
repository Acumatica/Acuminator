using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	public class MultipleSuppressionCodeFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default
				.WithIsvSpecificAnalyzersEnabled(),
				new ForbiddenFieldsInDacAnalyzer(),
				new DacExtensionDefaultAttributeAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressCommentFix();

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_All.cs",
			@"Dac\SuppressMuiltipleDiagnostics_All_Suppressed.cs")]
		public virtual void SuppressMuiltipleDiagnostics_All_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_One.cs",
		@"Dac\SuppressMuiltipleDiagnostics_One_Suppressed.cs")]
		public virtual void SuppressMuiltipleDiagnostics_One_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_All.cs")]
		public virtual void ShowAllUnsuppressedDiagnostics(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(line: 13, column: 25, "companyMask"),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 14, column: 4),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(line: 16, column: 17, "CompanyMask"));

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_One.cs")]
		public virtual void ShowUnsuppressedDiagnostic(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_One_Suppressed.cs")]
		public virtual void SuppressMultipleDiagnostics(string source) =>
			VerifyCSharpDiagnostic(source);
	}
}