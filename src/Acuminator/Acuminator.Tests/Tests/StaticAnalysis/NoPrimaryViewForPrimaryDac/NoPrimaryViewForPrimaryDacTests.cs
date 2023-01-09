#nullable enable

using System;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoPrimaryViewForPrimaryDac
{
	public class NoPrimaryViewForPrimaryDacTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			 new PXGraphAnalyzer(CodeAnalysisSettings.Default
													 .WithStaticAnalysisEnabled()
													 .WithSuppressionMechanismDisabled(), 
								 new NoPrimaryViewForPrimaryDacAnalyzer());
			
		[Theory]
		[EmbeddedFileData("NoPrimaryViewForPrimaryDac.cs")]
		public virtual Task WithoutPrimaryView(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1018_NoPrimaryViewForPrimaryDac.CreateFor(line: 17, column: 56),
				Descriptors.PX1018_NoPrimaryViewForPrimaryDac.CreateFor(line: 23, column: 15));

		[Theory]
		[EmbeddedFileData("HasPrimaryViewForPrimaryDac.cs")]
		public virtual Task WithPrimaryView_NoDiagnostics(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoPrimaryViewForPrimaryDacFluentBQL.cs")]
		public virtual Task WithoutPrimaryView_FluentBql(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1018_NoPrimaryViewForPrimaryDac.CreateFor(line: 18, column: 68));

		[Theory]
		[EmbeddedFileData("HasPrimaryViewForPrimaryDacFluentBQL.cs")]
		public virtual Task WithPrimaryView_FluentBql_NoDiagnostics(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("GenericGraphNoPrimaryViewForPrimaryDac.cs")]
		public virtual Task GenericGraphs_WithInheritance_WithoutPrimaryView_NoDiagnostics(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("AbstractGraphNoPrimaryViewForPrimaryDac.cs")]
		public virtual Task AbstractGraphs_WithInheritance_WithoutPrimaryView_NoDiagnostics(string source) => VerifyCSharpDiagnosticAsync(source);
	}
}
