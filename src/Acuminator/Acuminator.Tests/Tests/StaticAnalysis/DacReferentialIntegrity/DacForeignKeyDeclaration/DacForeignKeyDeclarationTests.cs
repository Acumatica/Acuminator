﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity
{
	public class DacForeignKeyDeclarationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
		new DacAnalyzersAggregator(
			CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
										.WithSuppressionMechanismDisabled(),
			new DacForeignKeyDeclarationAnalyzer());

		[Theory]
		[EmbeddedFileData("Dac_Without_ForeignKey.cs")]
		public async Task Dac_WithoutForeignKey(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1034_MissingDacForeignKeyDeclaration.CreateFor(6, 15));

		[Theory]
		[EmbeddedFileData("Dac_GoodForeignKey.cs")]
		public async Task Dac_WithCorrectForeignKey_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("PXMappedCacheExtension.cs")]
		public async Task PXMappedCacheExtension_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("Fully_Unbound_Dac.cs")]
		public async Task FullyUnboundDac_DoesntReportDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);
	}
}