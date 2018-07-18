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
    public class DacDepricatedFieldsTests : CodeFixVerifier
    {
        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacDepricatedFields.cs")]
        public virtual void Test_Dac_With_Depricated_Fields(string source) =>
            VerifyCSharpDiagnostic(source,
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 13, column: 9),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 17, column: 23),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 20, column: 9),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 23, column: 23),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 26, column: 9),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 29, column: 23),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 39, column: 9),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line: 42, column: 23));

        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacDepricatedFields.cs",
                          @"Dac\PX1027\CodeFixes\DacDepricatedFields_Expected.cs")]
        public virtual void Test_Fix_For_Dac_With_Depricated_Fields(string actual, string expected) =>
            VerifyCSharpFix(actual, expected);
            
            
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new DepricatedFieldsInDacFix();


        private DiagnosticResult CreatePX1027DepricatedDacFieldDiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1027_DepricatedFieldsInDacDeclaration.Id,
                Message = Descriptors.PX1027_DepricatedFieldsInDacDeclaration.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations =
                new[]
                { new DiagnosticResultLocation ("Test0.cs", line, column) }
            };

        }
    }
}
