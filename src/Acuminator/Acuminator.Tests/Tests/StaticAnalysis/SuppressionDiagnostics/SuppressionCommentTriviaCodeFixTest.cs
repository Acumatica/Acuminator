using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	public class SuppressionCommentTriviaCodeFixTest : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
		  new BqlParameterMismatchAnalyzer(
			  CodeAnalysisSettings.Default.WithStaticAnalysisEnabled());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressCommentFix();

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCall_Suppressed.cs")]
		public virtual void StaticCalls_Suppressed(string source) =>
			VerifyCSharpDiagnostic(source);


		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCall.cs", 
						  @"BqlParameterMismatch\StaticCall_Suppressed.cs")]
		public virtual void StaticCalls_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCallWithCustomPredicate_Suppressed.cs")]
		public virtual void StaticCall_WithCustomPredicates_Suppressed(string source) =>
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCallWithCustomPredicate.cs",
						  @"BqlParameterMismatch\StaticCallWithCustomPredicate_Suppressed.cs")]
		public virtual void StaticCall_WithCustomPredicates_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\InheritanceCall_Suppressed.cs")]
		public virtual void InheritanceCalls_InstanceAndStatic_Suppressed(string source) => 
			VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\InheritanceCall.cs",
						  @"BqlParameterMismatch\InheritanceCall_Suppressed.cs")]
		public virtual void InheritanceCalls_InstanceAndStatic_CodeFix(string actual, string expected) =>
			VerifyCSharpFix(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\PXUpdateCall_Suppressed.cs", @"BqlParameterMismatch\SOOrder.cs")]
		public virtual void PXUpdateCalls_Suppressed(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource });
		
		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\SearchCall_Suppressed.cs", @"BqlParameterMismatch\SOOrder.cs")]
		public virtual void SearchCalls_Suppressed(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource });

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\VariableInstanceCall_Suppressed.cs", @"BqlParameterMismatch\SOOrder.cs")]
		public virtual void VariableInstanceCalls_Suppressed(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource });
	}
}
