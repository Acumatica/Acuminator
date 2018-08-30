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
    public class DacForbiddenFieldsTests : CodeFixVerifier
    {
        [Theory]
        [EmbeddedFileData("DacForbiddenFields.cs")]
        public virtual void TestDacWithForbiddenFields(string source) =>
            VerifyCSharpDiagnostic(source,           
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 13, column: 25, fieldName: "companyId"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 17, column: 17, fieldName: "CompanyID"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 27, column: 25, fieldName: "deletedDatabaseRecord"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 30, column: 17, fieldName: "DeletedDatabaseRecord"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 39, column: 25, fieldName: "companyMask"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 42, column: 17, fieldName: "CompanyMask"));
                
        [Theory]
        [EmbeddedFileData("DacForbiddenFields.cs",
                          "DacForbiddenFields_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFields(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);
        [Theory]
        [EmbeddedFileData("DacForbiddenFieldsWithoutRegions.cs",
                          "DacForbiddenFieldsWithoutRegions_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFieldWithoutRegions(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);

        [Theory]
        [EmbeddedFileData("DacForbiddenFieldsRegions_Case1.cs",
                            "DacForbiddenFieldsRegions_Case1_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case1(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);

        [Theory]
        [EmbeddedFileData("DacForbiddenFieldsRegions_Case2.cs",
                            "DacForbiddenFieldsRegions_Case2_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case2(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);

        [Theory]
        [EmbeddedFileData("DacForbiddenFieldsRegions_Case3.cs",
                            "DacForbiddenFieldsRegions_Case3_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case3(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ForbiddenFieldsInDacFix();

	    private DiagnosticResult CreatePX1027ForbiddenDacFieldDiagnosticResult(int line, int column, string fieldName)
	    {
		    string format = Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.Title.ToString();
		    string expectedMessage = string.Format(format, fieldName);

		    return new DiagnosticResult
		    {
			    Id = Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.Id,
			    Message = expectedMessage,
			    Severity = DiagnosticSeverity.Error,
			    Locations =
				    new[]
					    { new DiagnosticResultLocation ("Test0.cs", line, column) }
		    };

	    }
	}
}
