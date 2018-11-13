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
    public class ViewDeclarationOrderTests : CodeFixVerifier
    {
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>  
			new PXGraphAnalyzer(CodeAnalysisSettings.Default
													.WithRecursiveAnalysisEnabled()
													.WithIsvSpecificAnalyzersEnabled(), 
								new ViewDeclarationOrderAnalyzerForTests());

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrderInOneGraph.cs")]
        public void SimpleGraph(string actual)
        {
            VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(9, 29, "Vendor", "BAccount"),
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(10, 31, "Vendor", "BAccount"),
	            Descriptors.PX1004_ViewDeclarationOrder.CreateFor(17, 28, "Customer", "BAccount"),
	            Descriptors.PX1004_ViewDeclarationOrder.CreateFor(18, 28, "Customer", "BAccount"));
        }

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrderWithGraphInheritance.cs")]
        public void GraphInheritance(string actual)
        {
            VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1004_ViewDeclarationOrder.CreateFor(25, 29, "SOInvoice", "ARInvoice"),
	            Descriptors.PX1006_ViewDeclarationOrder.CreateFor(27, 26, "SOTran", "ARTran"));
        }

		[Theory]
		[EmbeddedFileData("ViewDeclarationOrderWithGraphInheritance_OneCache.cs")]
		public void GraphInheritance_OneCache(string actual)
		{
			VerifyCSharpDiagnostic(actual,
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(13, 29, "SOInvoice", "ARInvoice"),
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(15, 29, "SOInvoice", "ARInvoice"),
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(20, 29, "SOInvoice", "ARInvoice"));
		}

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrderWithGraphExtension.cs")]
        public void GraphExtension(string actual)
        {
            VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1004_ViewDeclarationOrder.CreateFor(25, 29, "SOInvoice", "ARInvoice"),
	            Descriptors.PX1006_ViewDeclarationOrder.CreateFor(27, 26, "SOTran", "ARTran"));
        }


		/// <summary>
		/// A special derived view declaration order analyzer. Used for unit tests, overrides <see cref="ShouldAnalyze(PXContext, CodeAnalysisSettings)"/> logic 
		/// to always run analyzer no matter what version of Acumatica assemblies is used.
		/// </summary>
		private class ViewDeclarationOrderAnalyzerForTests : ViewDeclarationOrderAnalyzer
		{
			public override bool ShouldAnalyze(PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel graph) => true;
		}
	}
}
