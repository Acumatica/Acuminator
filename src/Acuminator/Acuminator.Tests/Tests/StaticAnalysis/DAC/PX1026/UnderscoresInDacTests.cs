using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace Acuminator.Tests
{
	public class UnderscoresInDacTests : DiagnosticVerifier
	{
		[Theory]
		[EmbeddedFileData(@"Dac\PX1026\Diagnostics\DacWithUnderscores.cs")]
		public virtual void Test_Dac_With_Underscores_In_Declaration(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1026DiagnosticResult(line: 10, column: 15),
				CreatePX1026DiagnosticResult(line: 13, column: 25),
				CreatePX1026DiagnosticResult(line: 18, column: 17),
				CreatePX1026DiagnosticResult(line: 49, column: 18),
				CreatePX1026DiagnosticResult(line: 52, column: 25));

		[Theory]
		[EmbeddedFileData(@"Dac\PX1026\Diagnostics\DacExtensionWithUnderscores.cs")]
		public virtual void Test_Dac_Extension_With_Underscores_In_Declaration(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1026DiagnosticResult(line: 10, column: 15),
				CreatePX1026DiagnosticResult(line: 13, column: 25),
				CreatePX1026DiagnosticResult(line: 17, column: 18));

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();
		
		private DiagnosticResult CreatePX1026DiagnosticResult(int line, int column)
		{
			return new DiagnosticResult
			{
				Id = Descriptors.PX1026_UnderscoresInDacDeclaration.Id,
				Message = Descriptors.PX1026_UnderscoresInDacDeclaration.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations =
					new[]
					{
						new DiagnosticResultLocation("Test0.cs", line, column)
					}
			};
		}
	}
}
