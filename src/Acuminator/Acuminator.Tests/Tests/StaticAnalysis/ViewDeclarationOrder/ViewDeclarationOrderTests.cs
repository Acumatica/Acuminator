using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ViewDeclarationOrder
{
    public class ViewDeclarationOrderTests : CodeFixVerifier
    {
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>  
			new PXGraphAnalyzer(new ViewDeclarationOrderAnalyzer());

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrder.cs")]
        public void SimpleGraph(string actual)
        {
            VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(9, 29, "Vendor", "BAccount"),
	            Descriptors.PX1004_ViewDeclarationOrder.CreateFor(18, 28, "Customer", "BAccount"));
        }
    }
}
