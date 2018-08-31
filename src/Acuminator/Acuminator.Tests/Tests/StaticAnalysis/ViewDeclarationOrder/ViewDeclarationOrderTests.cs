using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ViewDeclarationOrder
{
    public class ViewDeclarationOrderTests : CodeFixVerifier
    {
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new ViewDeclarationOrderAnalyzer();

		[Theory]
        [EmbeddedFileData("ViewDeclarationOrder.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1006_ViewDeclarationOrder.CreateFor(7, 14, "Vendor", "BAccount"),
	            Descriptors.PX1004_ViewDeclarationOrder.CreateFor(15, 14, "Customer", "BAccount"));
        }
    }
}
