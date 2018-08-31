using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraphUsageInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXGraphUsageInDac
{
    public class PXGraphUsageInDacTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphUsageInDacAnalyzer();

        [Theory]
        [EmbeddedFileData("DacWithGraphUsage.cs")]
        public void TestDiagnostic_Dac(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(15, 17),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(17, 24),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(27, 42),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(37, 24),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(41, 17));
        }

        [Theory]
        [EmbeddedFileData("DacExtensionWithGraphUsage.cs")]
        public void TestDiagnostic_CacheExtension(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(15, 17),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(17, 24),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(27, 42),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(37, 24),
                Descriptors.PX1029_PXGraphUsageInDac.CreateFor(41, 17));
        }

		[Theory]
		[EmbeddedFileData("DacWithNestedTypes.cs")]
	    public void TestDiagnostic_DacWithNestedTypes(string source)
	    {
		    VerifyCSharpDiagnostic(source,
				Descriptors.PX1029_PXGraphUsageInDac.CreateFor(23, 6),
			    Descriptors.PX1029_PXGraphUsageInDac.CreateFor(24, 16));
	    }
	}
}
