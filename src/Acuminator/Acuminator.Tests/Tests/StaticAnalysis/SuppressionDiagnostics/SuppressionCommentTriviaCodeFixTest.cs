﻿#nullable enable

using System.Threading.Tasks;

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

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new SuppressDiagnosticTestCodeFix();

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCall_Suppressed.cs")]
		public virtual Task StaticCalls_Suppressed(string source) =>
			VerifyCSharpDiagnosticAsync(source);


		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCall.cs", 
						  @"BqlParameterMismatch\StaticCall_Suppressed.cs")]
		public virtual Task StaticCalls_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCallWithCustomPredicate_Suppressed.cs")]
		public virtual Task StaticCall_WithCustomPredicates_Suppressed(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\StaticCallWithCustomPredicate.cs",
						  @"BqlParameterMismatch\StaticCallWithCustomPredicate_Suppressed.cs")]
		public virtual Task StaticCall_WithCustomPredicates_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\InheritanceCall_Suppressed.cs")]
		public virtual Task InheritanceCalls_InstanceAndStatic_Suppressed(string source) => 
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\InheritanceCall.cs",
						  @"BqlParameterMismatch\InheritanceCall_Suppressed.cs")]
		public virtual Task InheritanceCalls_InstanceAndStatic_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\PXUpdateCall_Suppressed.cs", @"BqlParameterMismatch\SOOrder.cs")]
		public virtual Task PXUpdateCalls_Suppressed(string source, string dacSource) =>
			VerifyCSharpDiagnosticAsync(new[] { source, dacSource });
		
		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\SearchCall_Suppressed.cs", @"BqlParameterMismatch\SOOrder.cs")]
		public virtual Task SearchCalls_Suppressed(string source, string dacSource) =>
			VerifyCSharpDiagnosticAsync(new[] { source, dacSource });

		[Theory]
		[EmbeddedFileData(@"BqlParameterMismatch\VariableInstanceCall_Suppressed.cs", @"BqlParameterMismatch\SOOrder.cs")]
		public virtual Task VariableInstanceCalls_Suppressed(string source, string dacSource) =>
			VerifyCSharpDiagnosticAsync(new[] { source, dacSource });
	}
}
