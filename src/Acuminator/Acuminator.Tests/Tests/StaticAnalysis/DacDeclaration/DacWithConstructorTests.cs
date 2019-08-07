using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacDeclaration;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacDeclaration
{
    public class DacWithConstructorTests : CodeFixVerifier
    {
	    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();
	    protected override CodeFixProvider GetCSharpCodeFixProvider() => new ConstructorInDacFix();

		[Theory]
        [EmbeddedFileData("DacWithConstructor.cs")]
        public virtual void TestDacWithConstructor(string source) =>
            VerifyCSharpDiagnostic(source,
                Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 14, column: 10),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 22, column: 10),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 97, column: 10),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 120, column: 10),
	            Descriptors.PX1028_ConstructorInDacDeclaration.CreateFor(line: 128, column: 10));

        [Theory]
        [EmbeddedFileData("DacWithConstructor.cs",
                          "DacWithConstructor_Expected.cs")]
        public virtual void TestCodeFixDacWithConstructor(string actual, string expected) =>
            VerifyCSharpFix(actual,expected);

        [Theory]
        [EmbeddedFileData("DacWithConstructor.cs",
	        "DacWithConstructorSuppressComment_Expected.cs")]
        public virtual void TestCodeFixDacWithConstructorSuppressComment(string actual, string expected) =>
	        VerifyCSharpFix(actual, expected, codeFixIndex:1);
	}
}
