using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
    public class LocalizationHardcodeTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new LocalizationAnalyzer();

        private DiagnosticResult CreatePX1050DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1050_HardcodedStringInLocalizationMethod.Id,
                Message = Descriptors.PX1050_HardcodedStringInLocalizationMethod.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData(@"Localization\PX1050\LocalizationWithHardcodedStrings.cs")]
        public void Localization_Methods_With_Hardcoded_Messages(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1050DiagnosticResult(19, 51),
                CreatePX1050DiagnosticResult(20, 51),
                CreatePX1050DiagnosticResult(21, 59),
                CreatePX1050DiagnosticResult(31, 57),
                CreatePX1050DiagnosticResult(32, 57),
                CreatePX1050DiagnosticResult(33, 65),
                CreatePX1050DiagnosticResult(34, 68),
                CreatePX1050DiagnosticResult(44, 52),
                CreatePX1050DiagnosticResult(45, 52),
                CreatePX1050DiagnosticResult(46, 58),
                CreatePX1050DiagnosticResult(47, 65));
        }
    }
}
