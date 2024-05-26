#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
    public class NonPublicGraphAndGraphExtensionsTests : CodeFixVerifier
    {
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new NonPublicDacGraphAndExtensionFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new NonPublicGraphAndDacAndExtensionsAnalyzer());

		[Theory]
		[EmbeddedFileData("NonPublicGraphExtension.cs")]
		public async Task NonPublicGraphExtensions(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(10, 2),
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(18, 8),
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(30, 24),
				 Descriptors.PX1022_NonPublicGraphExtension.CreateFor(38, 4));

		[Theory]
		[EmbeddedFileData("NonPublicGraph.cs")]
		public async Task NonPublicGraphs(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicGraph.CreateFor(10, 2),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(18, 8),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(30, 24),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(38, 4));

		[Theory]
		[EmbeddedFileData("NonPublicGraphWithBadModifiers.cs")]
		public async Task NonPublicGraphsWithBadModifiers(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicGraph.CreateFor(9, 4),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(9, 19),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(17, 11));

		[Theory]
		[EmbeddedFileData("NonPublicPartialGraph.cs")]
		public async Task NonPublicPartialGraphs(string actual) =>
			 await VerifyCSharpDiagnosticAsync(actual,
				 Descriptors.PX1022_NonPublicGraph.CreateFor(7, 17),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(15, 3),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(26, 17),
				 Descriptors.PX1022_NonPublicGraph.CreateFor(33, 24));

		[Theory]
        [EmbeddedFileData("NonPublicGraphExtension_Expected.cs")]
        public async Task NonPublicGraphExtensions_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("NonPublicGraph_Expected.cs")]
		public async Task NonPublicGraphs_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("NonPublicGraphWithBadModifiers_Expected.cs")]
		public async Task NonPublicGraphsWithBadModifiers_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("NonPublicPartialGraph_Expected.cs")]
		public async Task NonPublicPartialGraphs_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
	    [EmbeddedFileData("NonPublicGraphExtension.cs",
						  "NonPublicGraphExtension_Expected.cs")]
	    public async Task NonPublicGraphExtensions_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("NonPublicGraph.cs",
						  "NonPublicGraph_Expected.cs")]
		public async Task NonPublicGraphs_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("NonPublicGraphWithBadModifiers.cs",
						  "NonPublicGraphWithBadModifiers_Expected.cs")]
		public async Task NonPublicGraphsWithBadModifiers_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("NonPublicPartialGraph.cs",
						  "NonPublicPartialGraph_Expected.cs")]
		public async Task NonPublicPartialGraphs_CodeFix(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
