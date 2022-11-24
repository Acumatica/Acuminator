#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ExceptionSerialization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ExceptionSerialization
{
	public class MissingGetObjectDataOverrideFixTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new ExceptionSerializationAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new MissingGetObjectDataOverrideFix();

		[Theory]
		[EmbeddedFileData(@"CodeFix\MissingGetObjectDataOverride.cs",
						  @"CodeFix\MissingGetObjectDataOverride_Expected.cs")]
		public async Task Exception_WithNewSerializableData_GenerateGetObjectDataOverride(string actual, string expected) => 
			await VerifyCSharpFixAsync(actual, expected);
	}
}