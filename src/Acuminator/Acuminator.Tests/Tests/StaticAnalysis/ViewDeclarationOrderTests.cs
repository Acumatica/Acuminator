using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ViewDeclarationOrder;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using CodeFixVerifier = Acuminator.Tests.Verification.CodeFixVerifier;

namespace Acuminator.Tests
{
    public class ViewDeclarationOrderTests : Verification.CodeFixVerifier
    {
	    private DiagnosticResult[] CreateDiagnosticResults()
	    {
		    return new DiagnosticResult[] 
            {
                new DiagnosticResult
                {
                    Id = Descriptors.PX1006_ViewDeclarationOrder.Id,
                    Message = string.Format(Descriptors.PX1006_ViewDeclarationOrder.Title.ToString(), "Vendor", "BAccount"),
                    Severity = Descriptors.PX1004_ViewDeclarationOrder.DefaultSeverity,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 7, 14)},
                },

                new DiagnosticResult
                {
                    Id = Descriptors.PX1004_ViewDeclarationOrder.Id,
                    Message = string.Format(Descriptors.PX1004_ViewDeclarationOrder.Title.ToString(), "Customer", "BAccount"),
                    Severity = Descriptors.PX1004_ViewDeclarationOrder.DefaultSeverity,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 15, 14) }
                },
            };
	    }

        [Theory]
        [EmbeddedFileData("ViewDeclarationOrder.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreateDiagnosticResults());
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new ViewDeclarationOrderAnalyzer();
        }
    }
}
