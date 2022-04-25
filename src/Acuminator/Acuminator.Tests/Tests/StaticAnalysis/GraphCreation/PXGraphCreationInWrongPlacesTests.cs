#nullable enable

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
	public class PXGraphCreationInWrongPlacesTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
					.WithStaticAnalysisEnabled()
					.WithSuppressionMechanismDisabled()
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(),
				new PXGraphCreationInGraphInWrongPlacesAnalyzer());

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInstanceConstructor.cs")]
		public async Task GraphInstanceConstructor(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(15, 41));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitMethod.cs")]
		public async Task GraphExtensionInitialize(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(14, 41));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInitDelegate.cs")]
		public async Task GraphInitDelegate(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(20, 14));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceOutsideOfInitialization.cs")]
		public async Task PXGraph_OutsideOfInitialization(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("ViewDelegate.cs")]
		public async Task ViewDelegate(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(13, 25));

		[Theory]
		[EmbeddedFileData("ViewDelegateWithParameter.cs")]
		public async Task ViewDelegateWithParameter(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(13, 30));

		[Theory]
		[EmbeddedFileData("ViewDelegateInGraphExtensionOwnView.cs")]
		public async Task ViewDelegateInGraphExtensionOwnView(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(12, 33));

		[Theory]
		[EmbeddedFileData("ViewDelegateInGraphExtensionBaseView.cs")]
		public async Task ViewDelegateInGraphExtensionBaseView(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(10, 25));

		[Theory]
		[EmbeddedFileData("ViewDelegateInGraphExtensionOverride.cs")]
		public async Task ViewDelegateInGraphExtensionOverride(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1084_GraphCreationInDataViewDelegate.CreateFor(10, 33));
	}
}