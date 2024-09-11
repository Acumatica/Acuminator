﻿using Acuminator.Analyzers.StaticAnalysis;
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
		public void PrivateModifierNotAllowed(string source)
		{
			// The test should return exactly two errors.

			VerifyCSharpDiagnostic(source,
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.CreateFor(8, 16),
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.CreateFor(8, 16),
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.CreateFor(21, 16),
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.CreateFor(21, 16),
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.CreateFor(26, 18),
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.CreateFor(31, 27),
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.CreateFor(36, 26),
				Descriptors.PX1078_EventHandlersShouldNotBeSealed.CreateFor(41, 34)
			);
		}
	}
}
