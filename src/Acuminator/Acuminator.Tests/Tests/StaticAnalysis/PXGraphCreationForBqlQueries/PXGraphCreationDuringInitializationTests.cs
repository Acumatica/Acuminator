using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphCreationForBqlQueries
{
    public class PXGraphCreationForBqlQueriesTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphCreationForBqlQueriesAnalyzer();

        [Theory]
        [EmbeddedFileData("ExternalServiceWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
        public async Task ExternalServiceWithPXGraphConstructor(string source, string dacSource, string graphSource) => 
	        await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("ExternalServiceWithCreateInstance.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalServiceWithCreateInstance(string source, string dacSource, string graphSource) => 
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(17, 13));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphWithPXGraphConstructor.cs")]
	    public async Task InstanceMethodInPXGraphWithPXGraphConstructor(string source) => await VerifyCSharpDiagnosticAsync(source, 
		    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(15, 13));

	    [Theory]
	    [EmbeddedFileData("PXGraphConstructorInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task PXGraphConstructorInVariable(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(18, 13));

	    [Theory]
	    [EmbeddedFileData("CreateInstanceInVariable.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task CreateInstanceInVariable(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
				Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(18, 13));

	    [Theory]
	    [EmbeddedFileData("PropertyInExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task PropertyInExternalService(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(19, 14));

	    [Theory]
	    [EmbeddedFileData("InstanceMethodInPXGraphExtensionWithPXGraphConstructor.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task InstanceMethodInPXGraphExtensionWithPXGraphConstructor(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource,
			    Descriptors.PX1072_PXGraphCreationForBqlQueries.CreateFor(15, 13));


	    [Theory]
	    [EmbeddedFileData("ExternalService.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task ExternalService_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);

	    [Theory]
	    [EmbeddedFileData("StaticMethodInPXGraphWithPXGraphConstructor.cs")]
	    public async Task StaticMethodInPXGraphWithPXGraphConstructor_ShouldNotShowDiagnostic(string source) =>
		    await VerifyCSharpDiagnosticAsync(source);

	    [Theory]
	    [EmbeddedFileData("InstanceIsUsedOutsideBql.cs", "Customer.cs", "CustomerMaint.cs")]
	    public async Task InstanceIsUsedOutsideBql_ShouldNotShowDiagnostic(string source, string dacSource, string graphSource) =>
		    await VerifyCSharpDiagnosticAsync(source, dacSource, graphSource);
    }
}
