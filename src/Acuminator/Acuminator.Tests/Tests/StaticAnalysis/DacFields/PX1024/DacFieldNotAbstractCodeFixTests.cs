using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

using Acuminator.Analyzers;
using Acuminator.Tests.Helpers;
using Acuminator.Analyzers.FixProviders;


namespace Acuminator.Tests
{
	public class DacFieldNotAbstractCodeFixTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData(@"Dac\Diagnostics\PX1024\SOOrderNotAbstractField.cs",
						  @"Dac\CodeFixes\PX1024\SOOrderNotAbstractFieldExpected.cs")]
		public virtual void Test_Fix_For_Dac_With_Not_Abstract_Fields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacNonAbstractFieldTypeAnalyzer();

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacNonAbstractFieldTypeFix();

		private DiagnosticResult CreatePX1024NotAbstractDacFieldDiagnosticResult(int line, int column)
		{
			return new DiagnosticResult
			{
				Id = Descriptors.PX1024_DacNonAbstractFieldType.Id,
				Message = Descriptors.PX1024_DacNonAbstractFieldType.Title.ToString(),
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
