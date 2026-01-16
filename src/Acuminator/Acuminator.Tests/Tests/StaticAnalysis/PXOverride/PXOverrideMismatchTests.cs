#nullable enable
using System.Collections.Immutable;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXOverride;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverride
{
	public class PXOverrideMismatchTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForSignatureMismatchTests());

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsExactlyMatch.cs")]
		public Task ArgumentsExactlyMatch(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsDoNotMatch.cs")]
		public Task ArgumentsDoNotMatch(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 14));

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\LastArgumentIsNotDelegate.cs")]
		public Task LastArgumentIsNotDelegate(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 14),
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(23, 14));

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentTypesDoNotMatch.cs")]
		public Task ArgumentTypesDoNotMatch(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 14)
			);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ReturnTypesDoNotMatch.cs")]
		public Task ReturnTypesDoNotMatch(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 15)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsDoNotMatchWithDelegate.cs")]
		public Task ArgumentsDoNotMatchWithDelegate(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 17)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsDoNotMatchBaseHasMoreParameters.cs")]
		public Task ArgumentsDoNotMatchBaseHasMoreParameters(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 14)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseMethodIsNotVirtual.cs")]
		public Task BaseMethodIsNotVirtual(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 14)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseMethodIsNotAccessible.cs")]
		public Task BaseMethodIsNotAccessible(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 14)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\DerivedMethodIsGeneric.cs")]
		public Task DerivedMethodIsGeneric(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\DerivedMethodIsStatic.cs")]
		public Task DerivedMethodIsStatic(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ArgumentsMatchWithDelegate.cs")]
		public Task ArgumentsMatchWithDelegate(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\DelegateSignatureDoesNotMatch.cs")]
		public Task DelegateSignatureDoesNotMatch(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 17)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\PxOverrideInADifferentType.cs")]
		public Task PxOverrideInADifferentType(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(17, 17)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\ParentMethodAlsoHasTheDelegateSignature.cs")]
		public Task ParentMethodAlsoHasTheDelegateSignature(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\NoOverridenMethodDoesNotCrash.cs")]
		public Task NoOverridenMethodDoesNotCrash(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\NoPxOverrideAttributeDoNotCrash.cs")]
		public Task NoPxOverrideAttributeDoNotCrash(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\MethodHasTheDelegateAsType.cs")]
		public Task MethodHasTheDelegateAsType(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\MethodHasTheDelegateAsTypeAndReturnsVoid.cs")]
		public Task MethodHasTheDelegateAsTypeAndReturnsVoid(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeImplementsPxGraphExtension.cs")]
		public Task BaseTypeImplementsPxGraphExtension(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\PXOverrideOfMethodFromBasePXGraph.cs")]
		public Task PXOverride_OfMethod_FromBasePXGraph(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeImplementsPxGraphExtensionSignatureIsWrong.cs")]
		public Task BaseTypeImplementsPxGraphExtensionSignatureIsWrong(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(21, 17)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeDefinedAsExtension.cs")]
		public Task BaseTypeDefinedAsExtension(string source)
		{
			return VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1096_PXOverrideMustMatchSignature.CreateFor(28, 17)
			);
		}

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\BaseTypeDefinedAsExtensionNoError.cs")]
		public Task BaseTypeDefinedAsExtensionNoError(string source) => VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"SignatureMismatch\OverridenMethodIsInTheBaseOfTheBaseExtension.cs")]
		public Task OverriddenMethodIsInTheBaseOfTheBaseExtension(string source) => VerifyCSharpDiagnosticAsync(source);


		private sealed class PXOverrideAnalyzerForSignatureMismatchTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1096_PXOverrideMustMatchSignature
				);

			protected override void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext, 
																	   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodBaseDelegateParameter(SymbolAnalysisContext context, PXContext pxContext,
																		  PXOverrideInfo pxOverrideInfo)
			{ }

			protected override void CheckPatchMethodForXmlDocComment(SymbolAnalysisContext context, PXContext pxContext, 
																	 PXOverrideInfo pxOverrideInfo)
			{ }
		}
	}
}