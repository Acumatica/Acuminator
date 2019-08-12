using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;


namespace Acuminator.Tests.Tests.StaticAnalysis.DacNonAbstractFieldType
{
	public class DacFieldNotAbstractTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacNonAbstractFieldTypeAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new DacNonAbstractFieldTypeFix();

		[Theory]
		[EmbeddedFileData("SOOrderNotAbstractField.cs")]
		public virtual void DacWithNotAbstractFields(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1024_DacNonAbstractFieldType.CreateFor(line: 22, column: 16),
				Descriptors.PX1024_DacNonAbstractFieldType.CreateFor(line: 34, column: 16),
				Descriptors.PX1024_DacNonAbstractFieldType.CreateFor(line: 45, column: 16));

		

		[Theory]
		[EmbeddedFileData(@"SOOrderNotAbstractField.cs",
						  @"SOOrderNotAbstractField_Expected.cs")]
		public virtual void Fix_DacWithNotAbstractFields(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);
	}
}
