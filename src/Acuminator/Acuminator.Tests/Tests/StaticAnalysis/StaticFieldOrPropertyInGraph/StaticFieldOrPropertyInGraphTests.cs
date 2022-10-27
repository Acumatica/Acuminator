#nullable enable

using System;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.StaticFieldOrPropertyInGraph;
using Acuminator.Utilities;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

using AnalysisResources = Acuminator.Analyzers.Resources;

namespace Acuminator.Tests.Tests.StaticAnalysis.StaticFieldOrPropertyInGraph
{
	public class StaticFieldOrPropertyInGraphTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default
													.WithStaticAnalysisEnabled()
													.WithSuppressionMechanismDisabled(),
								new StaticFieldOrPropertyInGraphAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => 
			new StaticFieldOrPropertyInGraphFix();

		[Theory]
		[EmbeddedFileData("GraphWithStaticMembers.cs")] 
		public virtual async Task Graph_WithStatic_Fields_Properties_Actions_Views(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(10, 10, AnalysisResources.PX1062MessageFormatArg_Fields),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(12, 10, AnalysisResources.PX1062MessageFormatArg_Properties),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(18, 10, AnalysisResources.PX1062MessageFormatArg_Views),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(20, 10, AnalysisResources.PX1062MessageFormatArg_Views),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(24, 10, AnalysisResources.PX1062MessageFormatArg_Actions));

		[Theory]
		[EmbeddedFileData("GraphExtensionWithStaticMembers.cs")]
		public virtual async Task GraphExtension_WithStatic_Fields_Properties_Actions_Views(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(10, 10, AnalysisResources.PX1062MessageFormatArg_Fields),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(12, 10, AnalysisResources.PX1062MessageFormatArg_Properties),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(18, 10, AnalysisResources.PX1062MessageFormatArg_Views),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(20, 10, AnalysisResources.PX1062MessageFormatArg_Views),
				Descriptors.PX1062_StaticFieldOrPropertyInGraph.CreateFor(24, 10, AnalysisResources.PX1062MessageFormatArg_Actions));

		[Theory]
		[EmbeddedFileData(@"CodeFix\GraphWithStaticField_ReadOnlyFix.cs",
						  @"CodeFix\GraphWithStaticField_ReadOnlyFix_Expected.cs")]
		public async Task CodeFix_Graph_WithStaticMutableField_MakeReadonly(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\GraphWithStaticProperty_ReadOnlyFix.cs",
						  @"CodeFix\GraphWithStaticProperty_ReadOnlyFix_Expected.cs")]
		public async Task CodeFix_Graph_WithStaticMutableProperty_MakeReadonly(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\GraphWithStaticAction_RemoveStaticFix.cs",
						  @"CodeFix\GraphWithStaticAction_RemoveStaticFix_Expected.cs")]
		public async Task CodeFix_Graph_WithStaticAction_RemoveStatic(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"CodeFix\GraphWithStaticReadOnlyView_RemoveStaticFix.cs",
						  @"CodeFix\GraphWithStaticReadOnlyView_RemoveStaticFix_Expected.cs")]
		public async Task CodeFix_Graph_WithStaticReadOnlyView_RemoveStatic(string actual, string expected) =>
			await VerifyCSharpFixAsync(actual, expected);
	}
}
