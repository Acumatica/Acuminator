using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXActionOnNonPrimaryDac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionOnNonPrimaryDac
{
	public class PXActionOnNonPrimaryDacTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default
													.WithStaticAnalysisEnabled()
													.WithSuppressionMechanismDisabled(),
								new PXActionOnNonPrimaryDacAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXActionOnNonPrimaryDacFix();

		[Theory]
		[EmbeddedFileData("GraphWithNonPrimaryDacView.cs")] 
		public virtual Task Test_Diagnostic_For_Graph_And_Graph_Extension(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(23, 10, "Release1", "SOOrder"),
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(25, 10, "Release2", "SOOrder"),
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(34, 10, "Action1", "SOOrder"),
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(38, 10, "Action3", "SOOrder"),
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(47, 10, "Release1", "SOOrder"),
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(49, 10, "Release2", "SOOrder"));

		[Theory]
		[EmbeddedFileData("DerivedGraphWithBaseGraphPrimaryDac.cs")]
		public virtual Task Test_Diagnostic_For_Derived_Graph(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1012_PXActionOnNonPrimaryDac.CreateFor(26, 10, "Release1", "SOOrder"));

		[Theory]
		[EmbeddedFileData("GraphWithNonPrimaryDacView.cs",
						  "GraphWithNonPrimaryDacView_Expected.cs")]
		public Task Test_Code_Fix_For_Graph_And_Graph_Extension(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("DerivedGraphWithBaseGraphPrimaryDac.cs",
						  "DerivedGraphWithBaseGraphPrimaryDac_Expected.cs")]
		public Task Test_Code_Fix_For_Derived_Graph(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}
