﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Analyzers.Analyzers;
using PX.Analyzers.Test.Helpers;
using TestHelper;
using Xunit;

namespace PX.Analyzers.Test
{
    public class ViewDeclarationOrderTests : CodeFixVerifier
    {
	    private DiagnosticResult[] CreateDiagnosticResults()
	    {
		    return new DiagnosticResult[] 
            {
                new DiagnosticResult
                {
                    Id = Descriptors.PX1006_ViewDeclarationOrder.Id,
                    Message = Descriptors.PX1006_ViewDeclarationOrder.Title.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 7, 14) }
                },

                new DiagnosticResult
                {
                    Id = Descriptors.PX1004_ViewDeclarationOrder.Id,
                    Message = Descriptors.PX1004_ViewDeclarationOrder.Title.ToString(),
                    Severity = DiagnosticSeverity.Warning,
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
