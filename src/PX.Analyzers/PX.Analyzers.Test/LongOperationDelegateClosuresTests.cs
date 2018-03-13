using System;
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
    public class LongOperationDelegateClosuresTests : CodeFixVerifier
    {
	    private DiagnosticResult[] CreateDiagnosticResults()
	    {
		    return new DiagnosticResult[] 
            {
                new DiagnosticResult
                {
                    Id = Descriptors.PX1008_LongOperationDelegateClosures.Id,
                    Message = Descriptors.PX1008_LongOperationDelegateClosures.Title.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 15, 9) }
                },

                new DiagnosticResult
                {
                    Id = Descriptors.PX1008_LongOperationDelegateClosures.Id,
                    Message = Descriptors.PX1008_LongOperationDelegateClosures.Title.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 25, 9) }
                },

                new DiagnosticResult
                {
                    Id = Descriptors.PX1008_LongOperationDelegateClosures.Id,
                    Message = Descriptors.PX1008_LongOperationDelegateClosures.Title.ToString(),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] { new DiagnosticResultLocation("Test0.cs", 28, 9) }
                },
            };
	    }

        [Theory]
        [EmbeddedFileData("LongOperationDelegateClosuresAnalyzer.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual, CreateDiagnosticResults());
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new LongOperationDelegateClosuresAnalyzer();
        }
    }
}
