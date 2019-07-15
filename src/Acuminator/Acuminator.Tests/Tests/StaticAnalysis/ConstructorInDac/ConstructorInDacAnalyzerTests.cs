using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacDeclaration
{
    public class ConstructorInDacAnalyzerTests : CodeFixVerifier
    {
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new ConstructorInDacAnalyzer());
	    protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConstructorInDacFix();

		[Theory]
        [EmbeddedFileData("DacWithConstructor.cs")]
        public virtual void TestDacWithConstructor(string source) =>
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 13, column: 16),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 17, column: 16),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 74, column: 16),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 88, column: 16),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 92, column: 16));

        [Theory]
        [EmbeddedFileData("DacWithConstructor.cs",
                          "DacWithConstructor_Expected.cs")]
        public virtual void TestCodeFixDacWithConstructor(string actual, string expected) =>
            VerifyCSharpFix(actual,expected);
    }
}
