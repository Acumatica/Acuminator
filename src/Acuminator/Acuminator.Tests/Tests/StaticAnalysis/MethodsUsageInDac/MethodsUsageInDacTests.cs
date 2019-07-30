using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MethodsUsageInDac
{
    public class MethodsUsageInDacTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new MethodsUsageInDacAnalyzer());

        [Theory]
        [EmbeddedFileData("DacWithMethodsUsage.cs")]
        public void TestDiagnostic_Dac(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.CreateFor(23, 17),
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.CreateFor(24, 17),
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.CreateFor(26, 26),
	            Descriptors.PX1031_DacCannotContainInstanceMethods.CreateFor(47, 32));
        }

        [Theory]
        [EmbeddedFileData("DacExtensionWithMethodsUsage.cs")]
        public void TestDiagnostic_CacheExtension(string source)
        {
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.CreateFor(27, 17),
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.CreateFor(28, 17),
                Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.CreateFor(30, 26),
	            Descriptors.PX1031_DacCannotContainInstanceMethods.CreateFor(51, 32));
        }

	    [Theory]
	    [EmbeddedFileData("DacWithNestedTypes.cs")]
	    public void TestDiagnostic_DacWithNestedTypes_ShouldNotShowDiagnostic(string source)
	    {
		    VerifyCSharpDiagnostic(source);
	    }

        [Theory]
        [EmbeddedFileData("DacWithSystemTypesUsage.cs")]
        public void TestDiagnostic_DacWithSystemTypesUsage_ShouldNotShowDiagnostic(string source)
        {
            VerifyCSharpDiagnostic(source);
        }

    }
}
