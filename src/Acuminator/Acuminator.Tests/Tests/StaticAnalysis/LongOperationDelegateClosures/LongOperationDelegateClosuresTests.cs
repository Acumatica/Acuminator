using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationDelegateClosures
{
    public class LongOperationDelegateClosuresTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LongOperationDelegateClosuresAnalyzer();

		[Theory]
        [EmbeddedFileData("LongOperationDelegateClosures.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual,
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 23, column: 3),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 34, column: 3),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 37, column: 3),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 44, column: 3),
				Descriptors.PX1008_LongOperationDelegateClosures.CreateFor(line: 45, column: 3));
        }
	}
}
