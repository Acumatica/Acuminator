using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.ExceptionSerialization;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ExceptionSerialization
{
	public class ExceptionSerializationTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new ExceptionSerializationAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		[Theory]
		[EmbeddedFileData("ExceptionWithNewSerializableData.cs")]
		public void Exception_WithNewSerializableData_AndNoSerializationConstructor_AndNoGetObjectDataOverride(string actual) =>
			VerifyCSharpDiagnostic(actual,
				Descriptors.PX1063_NoSerializationConstructorInException.CreateFor(10, 22),
				Descriptors.PX1064_NoGetObjectDataOverrideInExceptionWithNewFields.CreateFor(10, 22));

		[Theory]
		[EmbeddedFileData("ExceptionWithNoNewSerializableData.cs")]
		public void Exception_NoNewSerializableData_AndNoSerializationConstructor_NoGetObjectDataOverrideNeeded(string actual) =>
			VerifyCSharpDiagnostic(actual,
				Descriptors.PX1063_NoSerializationConstructorInException.CreateFor(7, 17),
				Descriptors.PX1063_NoSerializationConstructorInException.CreateFor(15, 15),
				Descriptors.PX1063_NoSerializationConstructorInException.CreateFor(31, 15),
				Descriptors.PX1063_NoSerializationConstructorInException.CreateFor(56, 15));
	}
}