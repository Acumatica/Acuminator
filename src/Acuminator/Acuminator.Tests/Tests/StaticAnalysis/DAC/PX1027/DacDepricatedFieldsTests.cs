using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class DacDepricatedFieldsTests : DiagnosticVerifier
    {
        [Theory]
        [EmbeddedFileData(@"Dac\PX1027\Diagnostics\DacDepricatedFields.cs")]
        public virtual void Test_Dac_With_Depricated_Fields(string source) =>
            VerifyCSharpDiagnostic(source,
                CreatePX1027DepricatedDacFieldDiagnosticResult(line:13,column:31),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line:20,column:31),
                CreatePX1027DepricatedDacFieldDiagnosticResult(line:26,column:31));

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();

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
