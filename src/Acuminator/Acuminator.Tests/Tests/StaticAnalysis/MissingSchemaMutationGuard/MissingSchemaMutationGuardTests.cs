using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.MissingSchemaMutationGuard;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.MissingSchemaMutationGuard
{
	public class MissingSchemaMutationGuardTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new MissingSchemaMutationGuardAnalyzer(
				CodeAnalysisSettings.Default
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled());

		#region Positive cases — diagnostic expected

		[Theory]
		[EmbeddedFileData("Diagnostic_BareAlterTableAdd.cs")]
		public Task BareAlterTableAdd_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(9, 4));

		[Theory]
		[EmbeddedFileData("Diagnostic_MultipleUnguardedMutations.cs")]
		public Task MultipleUnguardedMutations_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(9, 4));

		[Theory]
		[EmbeddedFileData("Diagnostic_UnrelatedGuardDoesNotSatisfyMutation.cs")]
		public Task UnrelatedGuardDoesNotSatisfyMutation_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(9, 4));

		[Theory]
		[EmbeddedFileData("Diagnostic_ConstantStringConcatenation.cs")]
		public Task ConstantStringConcatenation_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(9, 4));

		[Theory]
		[EmbeddedFileData("Diagnostic_InterpolatedIdentifiers.cs")]
		public Task InterpolatedIdentifiers_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(11, 4));

		[Theory]
		[EmbeddedFileData("Diagnostic_BareCreateTable.cs")]
		public Task BareCreateTable_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(9, 4));

		[Theory]
		[EmbeddedFileData("Diagnostic_BareCreateIndex.cs")]
		public Task BareCreateIndex_ShouldReport(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1121_MissingSchemaMutationGuard.CreateFor(9, 4));

		#endregion

		#region Negative cases — no diagnostic expected

		[Theory]
		[EmbeddedFileData("NoDiagnostic_ColLengthGuard.cs")]
		public Task ColLengthGuard_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoDiagnostic_IfNotExistsSysColumns.cs")]
		public Task IfNotExistsSysColumns_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoDiagnostic_ObjectIdGuardCreateTable.cs")]
		public Task ObjectIdGuardCreateTable_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoDiagnostic_IndexPropertyGuardCreateIndex.cs")]
		public Task IndexPropertyGuardCreateIndex_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoDiagnostic_BeginEndBlockUnderGuard.cs")]
		public Task BeginEndBlockUnderGuard_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoDiagnostic_NonSchemaMutationPlugin.cs")]
		public Task NonSchemaMutation_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("NoDiagnostic_AlterInCommentIsIgnored.cs")]
		public Task AlterInCommentIsIgnored_ShouldNotReport(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		#endregion
	}
}
