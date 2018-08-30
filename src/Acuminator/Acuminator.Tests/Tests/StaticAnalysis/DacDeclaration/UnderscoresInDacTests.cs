using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.DacDeclaration;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using FluentAssertions;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacDeclaration
{
	public class UnderscoresInDacTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("DacWithUnderscores.cs")]
		public virtual void Test_Dac_With_Underscores_In_Declaration(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1026DiagnosticResult(line: 10, column: 15),
				CreatePX1026DiagnosticResult(line: 13, column: 25),
				CreatePX1026DiagnosticResult(line: 18, column: 17),
				CreatePX1026DiagnosticResult(line: 44, column: 25));

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnderscores.cs")]
		public virtual void Test_Dac_Extension_With_Underscores_In_Declaration(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1026DiagnosticResult(line: 10, column: 15),
				CreatePX1026DiagnosticResult(line: 13, column: 25),
				CreatePX1026DiagnosticResult(line: 17, column: 18),
				CreatePX1026DiagnosticResult(line: 21, column: 25),
				CreatePX1026DiagnosticResult(line: 24, column: 18));

		[Theory]
		[EmbeddedFileData("DacWithUnderscores.cs",
						  "DacWithUnderscores_Expected.cs")]
		public virtual void Test__Fix_For_Dac_With_Underscores_In_Declaration(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData("DacExtensionWithUnderscores.cs",
						  "DacExtensionWithUnderscores_Expected.cs")]
		public virtual void Test__Fix_For_Dac_Extension_With_Underscores_In_Declaration(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacDeclarationAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new UnderscoresInDacFix();

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
