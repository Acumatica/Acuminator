using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Analyzers.Analyzers;
using PX.Analyzers.FixProviders;
using PX.Analyzers.Test.Helpers;
using TestHelper;
using Xunit;

namespace PX.Analyzers.Test
{
	public class BQLFormatterTests : CodeFixVerifier
	{
		[Theory]
		[EmbeddedFileData("BQL.BadBQL.cs")]
		public void TestDiagnostic(string actual)
		{
			var diagnostics = new[]
			{
				new DiagnosticResult
				{
					Id = Descriptors.PXF1001_PXBadBqlDiagnostic.Id,
					Message = Descriptors.PXF1001_PXBadBqlDiagnostic.Title.ToString(),
					Severity = DiagnosticSeverity.Warning,
					Locations = new[]
					{
						new DiagnosticResultLocation("Test0.cs", line: 13, column: 10)
					}
				},

				new DiagnosticResult
				{
					Id = Descriptors.PXF1001_PXBadBqlDiagnostic.Id,
					Message = Descriptors.PXF1001_PXBadBqlDiagnostic.Title.ToString(),
					Severity = DiagnosticSeverity.Warning,
					Locations = new[]
					{
						new DiagnosticResultLocation("Test0.cs", line: 23, column: 16)
					}
				}
			};
			
			VerifyCSharpDiagnostic(actual, diagnostics);
		}

		[Theory]
		[EmbeddedFileData("BQL.BadBQL.cs", "BQL.GoodBQL.cs")]
		public void TestCodeFix(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		protected override CodeFixProvider GetCSharpCodeFixProvider()
		{
			return new BQLFormatterFixProvider();
		}

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
		{
			return new BQLFormatterAnalyzer();
		}
	}
}
