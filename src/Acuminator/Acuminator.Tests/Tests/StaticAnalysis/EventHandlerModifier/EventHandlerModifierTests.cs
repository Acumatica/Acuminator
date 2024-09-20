using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PrivateEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.EventHandlerModifier
{
	public class EventHandlerModifierTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new EventHandlerModifierAnalyzer());

		[Theory]
		[EmbeddedFileData("PrivateModifier.cs")]
		public async Task PrivateModifierNotAllowed(string source)
		{
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.CreateFor(8, 16),
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.CreateFor(12, 23, "protected virtual"),
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.CreateFor(19, 16),
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.CreateFor(23, 18, "protected virtual"),
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.CreateFor(27, 27, "protected virtual"),
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.CreateFor(31, 26, "protected virtual")
			);
		}

		[Theory]
		[EmbeddedFileData("ContainerWithInterface.cs")]
		public async Task ContainerWithInterface(string source)
		{
			// The test should return exactly two errors.

			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations.CreateFor(12, 26),
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations.CreateFor(17, 26)
			);
		}

		[Theory]
		[EmbeddedFileData("SealedContainer.cs")]
		public async Task SealedContainer(string source)
		{
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.CreateFor(8, 16)
			);
		}
	}
}
