using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Tests.Tests.StaticAnalysis.ViewDeclarationOrder
{
    /// <summary>
    /// A view declaration order analyzer test that check that error is not shown for the version of Acumatica greater than 2018R1. 
    /// Acuminator tests are using a greater version, so we shouldn't get a diagnostic.
    /// </summary>
    public class ViewDeclarationOrder_CheckForVersionTests : DiagnosticVerifier
    {
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>  
			new PXGraphAnalyzer(CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
															.WithSuppressionMechanismDisabled(), 
								new ViewDeclarationOrderAnalyzer());

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrderInOneGraph.cs")]
        public void SimpleGraph_NoDiagnostic(string actual) => VerifyCSharpDiagnostic(actual);

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrderWithGraphInheritance.cs")]
        public void GraphInheritance_NoDiagnostic(string actual) => VerifyCSharpDiagnostic(actual);

		[Theory]
		[EmbeddedFileData("ViewDeclarationOrderWithGraphInheritance_OneCache.cs")]
		public void GraphInheritance_OneCache_NoDiagnostic(string actual) => VerifyCSharpDiagnostic(actual);

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrderWithGraphExtension.cs")]
        public void GraphExtension_NoDiagnostic(string actual) => VerifyCSharpDiagnostic(actual);
	}
}
