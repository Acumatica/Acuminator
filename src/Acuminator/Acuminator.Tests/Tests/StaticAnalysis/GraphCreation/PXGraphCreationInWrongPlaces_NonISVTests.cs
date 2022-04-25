using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
#nullable enable

using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphCreation
{
	public class PXGraphCreationInWrongPlaces_NonISVTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
					.WithStaticAnalysisEnabled()
					.WithSuppressionMechanismDisabled()
					.WithRecursiveAnalysisEnabled()					
					.WithIsvSpecificAnalyzersDisabled(),
				new PXGraphCreationInGraphInWrongPlacesAnalyzer());

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInstanceConstructor.cs")]
		public async Task GraphInstanceConstructor(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(15, 41));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitMethod.cs")]
		public async Task GraphExtensionInitialize(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(14, 41));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInitDelegate.cs")]
		public async Task GraphInitDelegate(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(20, 14));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInIsActiveMethods.cs")]
		public async Task GraphExtension_IsActiveAndIsActiveForGraph(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization_NonISV.CreateFor(20, 14));
	}
}