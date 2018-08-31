using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.BqlParameterMismatch
{
	public class BqlParameterMismatchTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new BqlParameterMismatchAnalyzer();

		[Theory]
		[EmbeddedFileData("StaticCall.cs")]
		public virtual void Test_Static_Calls(string source) =>
			VerifyCSharpDiagnostic(source, 
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(20, 6, "SelectSingleBound", 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(33, 6, "SelectSingleBound", 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(47, 6, "SelectSingleBound", 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(62, 6, "SelectSingleBound", 1, 2));

		[Theory]
		[EmbeddedFileData("StaticCallWithCustomPredicate.cs")]
		public virtual void Test_Static_Call_With_Custom_Predicates(string source) =>
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(28, 6, "SelectSingleBound", 4));

		[Theory]
		[EmbeddedFileData("InheritanceCall.cs")]
		public virtual void Test_Inheritance_Calls_Instance_And_Static(string source) => VerifyCSharpDiagnostic(source,
			Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(28, 31, "Select", 2));

		[Theory]
		[EmbeddedFileData("FieldInstanceCall.cs", "SOOrder.cs")]
		public virtual void Test_Field_Instance_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(20, 24, "SelectSingle", 2));

		[Theory]
		[EmbeddedFileData("SearchCall.cs", "SOOrder.cs")]
		public virtual void Test_Search_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(22, 24, "Search", 1),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(24, 24, "Search", 3),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(34, 7, "Search", 1),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(46, 6, "Search", 3));

		[Theory]
		[EmbeddedFileData("PXUpdateCall.cs", "SOOrder.cs")]
		public virtual void Test_PXUpdate_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(43, 7, "Update", 3),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(54, 7, "Update", 3));

		[Theory]
		[EmbeddedFileData("VariableInstanceCall.cs", "SOOrder.cs")]
		public virtual void Test_Variable_Instance_Calls(string source, string dacSource) =>
			VerifyCSharpDiagnostic(new[] { source, dacSource },
				Descriptors.PX1015_PXBqlParametersMismatchWithRequiredAndOptionalParams.CreateFor(24, 27, "Select", 1, 2),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(38, 27, "Select", 1),
				Descriptors.PX1015_PXBqlParametersMismatchWithOnlyRequiredParams.CreateFor(57, 54, "Select", 2));
	}
}
