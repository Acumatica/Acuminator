using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using Acuminator.Analyzers;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Tests.Helpers;
using DiagnosticVerifier = Acuminator.Tests.Verification.DiagnosticVerifier;

namespace Acuminator.Tests
{
	public class GraphWithPrimaryDacWithoutViewTests : Verification.DiagnosticVerifier
	{
		[Theory]
		[EmbeddedFileData(@"View\PX1018\Diagnostics\GraphWithPrimaryDacWithoutView.cs")]
		public virtual void Test_Graph_With_Primary_Dac_Without_Primary_View(string source) =>
			VerifyCSharpDiagnostic(source,
				CreatePX1018DiagnosticResult(line: 17, column: 56),
				CreatePX1018DiagnosticResult(line: 23, column: 15));

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new NoPrimaryViewForPrimaryDacAnalyzer();

		private DiagnosticResult CreatePX1018DiagnosticResult(int line, int column) =>
			new DiagnosticResult
			{
				Id = Descriptors.PX1018_NoPrimaryViewForPrimaryDac.Id,
				Message = Descriptors.PX1018_NoPrimaryViewForPrimaryDac.Title.ToString(),
				Severity = DiagnosticSeverity.Error,
				Locations = new[]
				{
					new DiagnosticResultLocation("Test0.cs", line, column)
				}
			};
	}
}
