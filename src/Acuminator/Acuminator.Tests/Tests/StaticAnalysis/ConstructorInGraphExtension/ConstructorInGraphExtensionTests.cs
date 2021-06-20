using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ConstructorInGraphExtension
{
	public class ConstructorInGraphExtensionTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConstructorInGraphExtensionCodeFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default
													.WithStaticAnalysisEnabled()
													.WithSuppressionMechanismDisabled(),
								new ConstructorInGraphExtensionAnalyzer());

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtension.cs")]
		public Task TestDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1040_ConstructorInGraphExtension.CreateFor(18, 10));

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtensionWithInitialize.cs")]
		public Task TestDiagnostic_WithInitialize(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1040_ConstructorInGraphExtension.CreateFor(18, 10));

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtension_Expected.cs")]
		public Task TestDiagnostic_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtensionWithInitialize_Expected.cs")]
		public Task TestDiagnostic_WithInitialize_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtension.cs",
			"ConstructorInGraphExtension_Expected.cs")]
		public Task TestCodeFix(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("ConstructorInGraphExtensionWithInitialize.cs",
			"ConstructorInGraphExtensionWithInitialize_Expected.cs")]
		public Task TestCodeFix_WithInitialize(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);
	}
}
