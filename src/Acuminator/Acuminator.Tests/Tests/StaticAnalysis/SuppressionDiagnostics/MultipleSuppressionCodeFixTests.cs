#nullable enable

using System.Threading.Tasks;

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
									.WithIsvSpecificAnalyzersEnabled()
									.WithStaticAnalysisEnabled()
									.WithRecursiveAnalysisEnabled(),
				new ForbiddenFieldsInDacAnalyzer(),
				new DacExtensionDefaultAttributeAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressDiagnosticTestCodeFix();

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_All.cs",
			@"Dac\SuppressMuiltipleDiagnostics_All_Suppressed.cs")]
		public virtual Task SuppressMuiltipleDiagnostics_All_CodeFix(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_One.cs",
		@"Dac\SuppressMuiltipleDiagnostics_One_Suppressed.cs")]
		public virtual Task SuppressMuiltipleDiagnostics_One_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_All.cs")]
		public virtual Task ShowAllUnsuppressedDiagnostics(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(line: 13, column: 25,  messageArgs: "companyMask"),
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 14, column: 4),
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.CreateFor(line: 16, column: 17, messageArgs : "CompanyMask"));

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_One.cs")]
		public virtual Task ShowUnsuppressedDiagnostic(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC.CreateFor(line: 16, column: 4));

		[Theory]
		[EmbeddedFileData(@"Dac\SuppressMuiltipleDiagnostics_One_Suppressed.cs")]
		public virtual Task SuppressMultipleDiagnostics(string source) =>
			VerifyCSharpDiagnosticAsync(source);
	}
}