using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Analyzers.FixProviders;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public partial class DacForbiddenFieldsTests : CodeFixVerifier
    {
        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacForbiddenFields.cs")]
        public virtual void TestDacWithForbiddenFields(string source) =>
            VerifyCSharpDiagnostic(source,
                //DAC part 1
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 13, column: 31, fieldName: "companyId"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 17, column: 23, fieldName: "CompanyID"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 27, column: 31, fieldName: "deletedDatabaseRecord"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 30, column: 23, fieldName: "DeletedDatabaseRecord"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 39, column: 31, fieldName: "companyMask"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 42, column: 23, fieldName: "CompanyMask"));
                //DAC part 2
               /* CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 49, column: 31, fieldName: "deletedDatabaseRecord"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 52, column: 23, fieldName: "DeletedDatabaseRecord"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 55, column: 31, fieldName: "companyMask"),
                CreatePX1027ForbiddenDacFieldDiagnosticResult(line: 58, column: 23, fieldName: "CompanyMask"));*/

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacForbiddenFields.cs",
                          @"Dac\PX1027\CodeFixes\DacForbiddenFields_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFields(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);
        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacForbiddenFieldsWithoutRegions.cs",
                          @"Dac\PX1027\CodeFixes\DacForbiddenFieldsWithoutRegions_Expected.cs")]
        public virtual void TestFixForDacWithForbiddenFieldWithoutRegions(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacForbiddenFieldsRegions_Case1.cs",
                            @"Dac\PX1027\CodeFixes\DacForbiddenFieldsRegions_Case1_Expected")]
        public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case1(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacForbiddenFieldsRegions_Case2.cs",
                            @"Dac\PX1027\CodeFixes\DacForbiddenFieldsRegions_Case2_Expected")]
        public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case2(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacForbiddenFieldsRegions_Case3.cs",
                            @"Dac\PX1027\CodeFixes\DacForbiddenFieldsRegions_Case3_Expected")]
        public virtual void TestFixForDacWithForbiddenFieldsWithRegions_Case3(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ForbiddenFieldsInDacFix();
    }
    public partial class DacForbiddenFieldsTests : CodeFixVerifier
    {
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
