using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationForBqlQueries
{
    public class PXGraphCreationForBqlQueriesTests : CodeFixVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphCreationForBqlQueriesAnalyzer();

	    protected override CodeFixProvider GetCSharpCodeFixProvider() => new PXGraphCreationForBqlQueriesFix();

	    #region Positive checks

	    [Theory]
        [EmbeddedFileData("ExternalServiceWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
        public async Task TestDiagnostic_ExternalServiceWithPXGraphConstructor(string source, string dacSource, string graphSource) => 
	        await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithCreateInstance.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_ExternalServiceWithCreateInstance(string source, string dacSource, string graphSource) => 
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphWithPXGraphConstructor.cs")]
	    public async Task TestDiagnostic_InstanceMethodInPXGraphWithPXGraphConstructor(string source) => await VerifyCSharpDiagnosticAsync(source, 
		    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(15, 13));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphWithCreateInstance.cs")]
	    public async Task TestDiagnostic_InstanceMethodInPXGraphWithCreateInstance(string source) => await VerifyCSharpDiagnosticAsync(source, 
		    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(15, 13));

	    [Theory]
	    [EmbeddedFileData("PXGraphConstructorInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_PXGraphConstructorInVariable(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(18, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithCreateInstanceInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_ExternalServiceWithCreateInstanceInVariable(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(18, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithMethodParameter.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_ExternalServiceWithMethodParameter(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("PropertyInExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_PropertyInExternalService(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(19, 14));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphExtensionWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_InstanceMethodInPXGraphExtensionWithPXGraphConstructor(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(15, 13));

		#endregion

		#region False-positive checks

	    [Theory]
	    [EmbeddedFileData("ExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_ExternalService_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

	    [Theory]
	    [EmbeddedFileData("StaticMethodInPXGraphWithPXGraphConstructor.cs")]
	    public async Task TestDiagnostic_StaticMethodInPXGraphWithPXGraphConstructor_ShouldNotShowDiagnostic(string source) =>
		    await VerifyCSharpDiagnosticAsync(source);

	    [Theory]
	    [EmbeddedFileData("InstanceIsUsedOutsideBql.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_InstanceIsUsedOutsideBql_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithUsedVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task TestDiagnostic_ExternalServiceWithUsedVariable_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

		#endregion

		#region Code Fixes

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_ThisKeyword.cs")]
	    public async Task TestCodeFix_ThisKeyword(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 0);

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_MethodParameter.cs")]
	    public async Task TestCodeFix_MethodParameter(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 1);

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_LocalVariable1.cs")]
	    public async Task TestCodeFix_LocalVariable1(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 2);

	    [Theory]
	    [EmbeddedFileData("CodeFix.cs", "CodeFix_Expected_LocalVariable2.cs")]
	    public async Task TestCodeFix_LocalVariable2(string actual, string expected) => 
		    await VerifyCSharpFixAsync(actual, expected, 3);

		#endregion
    }
}
