using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.BqlParameterMismatch
{
	public class BqlParameterMismatchTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new BqlParameterMismatchAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		[Theory]
		[EmbeddedFileData("StaticCall.cs")]
		public virtual async Task StaticCalls(string source) =>
			await VerifyCSharpDiagnosticAsync(source, 
					Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(20, 6, "SelectSingleBound", 2),
					Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(33, 6, "SelectSingleBound", 2),
					Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(47, 6, "SelectSingleBound", 2),
					Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(62, 6, "SelectSingleBound", 1, 2));

		[Theory]
		[EmbeddedFileData("StaticCallWithCustomPredicate.cs")]
		public virtual async Task StaticCall_WithCustomPredicates(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(28, 6, "SelectSingleBound", 4));

		[Theory]
		[EmbeddedFileData("InheritanceCall.cs")]
		public virtual async Task InheritanceCalls_Instance_And_Static(string source) => 
			await VerifyCSharpDiagnosticAsync(source,
											  Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(28, 31, "Select", 2));

		[Theory]
		[EmbeddedFileData("FieldInstanceCall.cs", "SOOrder.cs")]
		public virtual async Task Field_InstanceCalls(string source, string dacSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(20, 24, "SelectSingle", 2));

		[Theory]
		[EmbeddedFileData("SearchCall.cs", "SOOrder.cs")]
		public virtual async Task Search_Calls(string source, string dacSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(22, 24, "Search", 1),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(24, 24, "Search", 3),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(34, 7, "Search", 1),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(46, 6, "Search", 3));

		[Theory]
		[EmbeddedFileData("FbqlCalls.cs", "SOOrder.cs")]
		public virtual async Task FBQL_Calls(string source, string dacSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(30, 30, "SelectSingle", 1, 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(31, 30, "SelectSingle", 1, 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(70, 6, "SelectSingleBound", 1, 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(79, 6, "SelectSingleBound", 1, 2));

		[Theory]
		[EmbeddedFileData("PXUpdateCall.cs", "SOOrder.cs")]
		public virtual async Task PXUpdate_Calls(string source, string dacSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(43, 7, "Update", 3),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(54, 7, "Update", 3));

		[Theory]
		[EmbeddedFileData("VariableInstanceCall.cs", "SOOrder.cs")]
		public virtual async Task Variable_Instance_Calls(string source, string dacSource) =>
			await VerifyCSharpDiagnosticAsync(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(24, 27, "Select", 1, 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(38, 27, "Select", 1),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(57, 54, "Select", 2));

		[Theory]
		[EmbeddedFileData("PXSelectExtensionCall.cs")]
		public virtual async Task PXSelectExtension_Call_NoDiagnostic(string source) => 
			await VerifyCSharpDiagnosticAsync(source);
	}
}
