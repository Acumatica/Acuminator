using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphCreation
{
	public class PXGraphCreationInGraphSemanticModelNonISVTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(),
				new PXGraphCreationInGraphInWrongPlacesAnalyzer());

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInstanceConstructor.cs")]
		public async Task PXGraphCreationDuringInitialization_GraphInstanceConstructor_CreateInstanceUsage(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(15, 41));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitMethod.cs")]
		public async Task PXGraphCreationDuringInitialization_GraphExtensionInitialize_CreateInstanceUsage(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(14, 41));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInitDelegate.cs")]
		public async Task PXGraphCreationDuringInitialization_GraphInitDelegate_CreateInstanceUsage(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(20, 14));
	}
}