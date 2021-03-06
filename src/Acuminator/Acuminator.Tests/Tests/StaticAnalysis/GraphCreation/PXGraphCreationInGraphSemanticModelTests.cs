﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationDuringInitialization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.GraphCreation
{
	public class PXGraphCreationInGraphSemanticModelTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(),
				new PXGraphCreationInGraphSemanticModelAnalyzer());

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceInInstanceConstructor.cs")]
		public async Task PXGraphCreationDuringInitialization_GraphInstanceConstructor_CreateInstanceUsage(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(15, 41));

		[Theory]
		[EmbeddedFileData("PXGraphExtensionWithCreateInstanceInInitMethod.cs")]
		public async Task PXGraphCreationDuringInitialization_GraphExtensionInitialize_CreateInstanceUsage(
			string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(14, 41));

		[Theory]
		[EmbeddedFileData("PXGrapWithCreateInstanceInInitDelegate.cs")]
		public async Task PXGraphCreationDuringInitialization_GraphInitDelegate_CreateInstanceUsage(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1057_PXGraphCreationDuringInitialization.CreateFor(20, 14));

		[Theory]
		[EmbeddedFileData("PXGraphWithCreateInstanceOutsideOfInitialization.cs")]
		public async Task PXGraph_OutsideOfInitialization_CreateInstanceUsage(string source) =>
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