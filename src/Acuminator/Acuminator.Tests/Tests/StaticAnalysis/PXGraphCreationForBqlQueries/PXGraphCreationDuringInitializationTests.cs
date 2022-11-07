using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationForBqlQueries
{
    public class PXGraphCreationForBqlQueriesTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new PXGraphCreationForBqlQueriesAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithRecursiveAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

	    protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXGraphCreationForBqlQueriesFix();

	    #region Positive checks

	    [Theory]
        [EmbeddedFileData("ExternalServiceWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
        public async Task ExternalServiceWithPXGraphConstructor(string source, string dacSource, string graphSource) => 
	        await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithCreateInstance.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalServiceWithCreateInstance(string source, string dacSource, string graphSource) => 
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphWithPXGraphConstructor.cs")]
	    public async Task InstanceMethodInPXGraphWithPXGraphConstructor(string source) => await VerifyCSharpDiagnosticAsync(source, 
		    Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(15, 13));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphWithCreateInstance.cs")]
	    public async Task InstanceMethodInPXGraphWithCreateInstance(string source) => await VerifyCSharpDiagnosticAsync(source, 
		    Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(15, 13));

	    [Theory]
	    [EmbeddedFileData("PXGraphConstructorInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task PXGraphConstructorInVariable(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(18, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithCreateInstanceInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalServiceWithCreateInstanceInVariable(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(18, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithMethodParameter.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalServiceWithMethodParameter(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("PropertyInExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task PropertyInExternalService(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(19, 14));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphExtensionWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task InstanceMethodInPXGraphExtensionWithPXGraphConstructor(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(15, 13));

		[Theory]
		[EmbeddedFileData("CustomerMaint_CheckGraphContext.cs")]
		public async Task InsideGraph_OnlyInstanceMethodsAreReported_WithThisReferenceSuggestion(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_ReuseExistingGraphVariable.CreateFor(17, 50));

		[Theory]
		[EmbeddedFileData("ExternalService_TwoQueryWithGraphCreation.cs", "Customer.cs", "CustomerMaint.cs")]
		public async Task TwoQuery_WithGraphCreationInArgument(string source, string dacSource, string graphSource) =>
			await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable.CreateFor(14, 51),
				Descriptors.PX1072_PXGraphCreationForBqlQueries_CreateSharedGraphVariable.CreateFor(15, 51));
		#endregion

		#region False-positive checks

		[Theory]
	    [EmbeddedFileData("ExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalService_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

	    [Theory]
	    [EmbeddedFileData("StaticMethodInPXGraphWithPXGraphConstructor.cs")]
	    public async Task StaticMethodInPXGraphWithPXGraphConstructor_ShouldNotShowDiagnostic(string source) =>
		    await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("ExternalService_SingleQueryWithGraphCreation.cs", "Customer.cs", "CustomerMaint.cs")]
		public async Task SingleQuery_WithGraphCreationInArgument_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
			await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		[Theory]
		[EmbeddedFileData("ExternalService_TwoQueriesReuseGraphField.cs", "Customer.cs")]
		public async Task TwoQueries_ReusingSameGraphInArgument_ShouldNotShowDiagnostic(string source, string dacSource) =>
			await VerifyCSharpDiagnosticAsync(source, dacSource);


		[Theory]
	    [EmbeddedFileData("InstanceIsUsedOutsideBql.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task InstanceIsUsedOutsideBql_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithUsedVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalServiceWithUsedVariable_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		#endregion

		#region Code Fixes

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_ThisKeyword.cs")]
	    public async Task CodeFix_ThisKeyword(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 0);

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_MethodParameter.cs")]
	    public async Task CodeFix_MethodParameter(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 1);

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_LocalVariable1.cs")]
	    public async Task CodeFix_LocalVariable1(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 2);

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_LocalVariable2.cs")]
	    public async Task CodeFix_LocalVariable2(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 3);

	    [Theory]
	    [EmbeddedFileData("CodeFix_GraphExtension.cs", "CodeFix_GraphExtension_Expected.cs")]
	    public async Task CodeFix_GraphExtension(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected);

		#endregion
    }
}
