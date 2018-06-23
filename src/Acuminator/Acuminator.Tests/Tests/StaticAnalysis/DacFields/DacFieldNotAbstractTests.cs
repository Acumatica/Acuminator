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
	public class DacFieldNotAbstractTests : DiagnosticVerifier
	{
		[Theory]
		[EmbeddedFileData(@"Dac\SOOrderNotAbstractField.cs")]
		public virtual void Test_Dac_Property_With_PXDBScalar_Attribute(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1024NotAbstractDacFieldDiagnosticResult(line: 22, column: 16),
				CreatePX1024NotAbstractDacFieldDiagnosticResult(line: 34, column: 16),
				CreatePX1024NotAbstractDacFieldDiagnosticResult(line: 45, column: 16));	

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacNonAbstractFieldTypeAnalyzer();

		
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
