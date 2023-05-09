using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverrideMismatch
{
	public class PXOverrideMismatchTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXOverrideMismatchAnalyzer(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled());

		[Theory]
		[EmbeddedFileData("ArgumentsExactlyMatch.cs")]
		public void ArgumentsExactlyMatch(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("ArgumentsDoNotMatch.cs")]
		public void ArgumentsDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(16, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("ArgumentsMatchWithDelegate.cs")]
		public void ArgumentsMatchWithDelegate(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("DelegateSignatureDoesNotMatch.cs")]
		public void DelegateSignatureDoesNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(16, 25)
			);
		}

		[Theory]
		[EmbeddedFileData("PxOverrideInADifferentType.cs")]
		public void PxOverrideInADifferentType(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(16, 26)
			);
		}

		[Theory]
		[EmbeddedFileData("PxOverrideInADifferentBaseType.cs")]
		public void PxOverrideInADifferentBaseType(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideContainerIsNotPXGraphExtension.CreateFor(16, 26)
			);
		}

		[Theory]
		[EmbeddedFileData("ParentMethodAlsoHasTheDelegateSignature.cs")]
		public void ParentMethodAlsoHasTheDelegateSignature(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("NoOverridenMethodDoesNotCrash.cs")]
		public void NoOverridenMethodDoesNotCrash(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("NoPxOverrideAttributeDoNotCrash.cs")]
		public void NoPxOverrideAttributeDoNotCrash(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("MethodHasTheDelegateAsType.cs")]
		public void MethodHasTheDelegateAsType(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("MethodHasTheDelegateAsTypeAndReturnsVoid.cs")]
		public void MethodHasTheDelegateAsTypeAndReturnsVoid(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("BaseTypeImplementsPxGraphExtension.cs")]
		public void BaseTypeImplementsPxGraphExtension(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("BaseTypeImplementsPxGraphExtensionSignatureIsWrong.cs")]
		public void BaseTypeImplementsPxGraphExtensionSignatureIsWrong(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(20, 25)
			);
		}

		[Theory]
		[EmbeddedFileData("TypeArgumentImplementsPxGraphExtension.cs")]
		public void TypeArgumentImplementsPxGraphExtension(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("TypeArgumentImplementsPxGraphExtensionSignatureIsWrong.cs")]
		public void TypeArgumentImplementsPxGraphExtensionSignatureIsWrong(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(24, 25)
			);
		}

		[Theory]
		[EmbeddedFileData("BaseTypeDefinedAsExtension.cs")]
		public void BaseTypeDefinedAsExtension(string source) => VerifyCSharpDiagnostic(source);
	}
}
