using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
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
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideMismatchAnalyzer());

		[Theory]
		[EmbeddedFileData("ArgumentsExactlyMatch.cs")]
		public void ArgumentsExactlyMatch(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("ArgumentsDoNotMatch.cs")]
		public void ArgumentsDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("LastArgumentIsNotDelegate.cs")]
		public void LastArgumentIsNotDelegate(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22),
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(23, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("ArgumentTypesDoNotMatch.cs")]
		public void ArgumentTypesDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("ReturnTypesDoNotMatch.cs")]
		public void ReturnTypesDoNotMatch(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 23)
			);
		}

		[Theory]
		[EmbeddedFileData("ArgumentsDoNotMatchWithDelegate.cs")]
		public void ArgumentsDoNotMatchWithDelegate(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 25)
			);
		}

		[Theory]
		[EmbeddedFileData("ArgumentsDoNotMatchBaseHasMoreParameters.cs")]
		public void ArgumentsDoNotMatchBaseHasMoreParameters(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("BaseMethodIsNotVirtual.cs")]
		public void BaseMethodIsNotVirtual(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("BaseMethodIsNotAccessible.cs")]
		public void BaseMethodIsNotAccessible(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 22)
			);
		}

		[Theory]
		[EmbeddedFileData("DerivedMethodIsGeneric.cs")]
		public void DerivedMethodIsGeneric(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 15)
			);
		}

		[Theory]
		[EmbeddedFileData("DerivedMethodIsStatic.cs")]
		public void DerivedMethodIsStatic(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 21)
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
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 25)
			);
		}

		[Theory]
		[EmbeddedFileData("PxOverrideInADifferentType.cs")]
		public void PxOverrideInADifferentType(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 26)
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
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(21, 25)
			);
		}

		[Theory]
		[EmbeddedFileData("BaseTypeDefinedAsExtension.cs")]
		public void BaseTypeDefinedAsExtension(string source)
		{
			VerifyCSharpDiagnostic(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(28, 26)
			);
		}

		[Theory]
		[EmbeddedFileData("BaseTypeDefinedAsExtensionNoError.cs")]
		public void BaseTypeDefinedAsExtensionNoError(string source) => VerifyCSharpDiagnostic(source);

		[Theory]
		[EmbeddedFileData("OverridenMethodIsInTheBaseOfTheBaseExtension.cs")]
		public void OverridenMethodIsInTheBaseOfTheBaseExtension(string source) => VerifyCSharpDiagnostic(source);
	}
}