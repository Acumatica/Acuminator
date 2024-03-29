﻿#nullable enable

using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.CallingBaseActionHandler;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallingBaseActionHandler
{
	public class CallingBaseActionHandlerFromOverrideHandlerTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new CallingBaseActionHandlerFromOverrideHandlerAnalyzer());

		[Theory]
		[EmbeddedFileData("BaseActionInvocation_Bad.cs")]
		public async Task BaseActionInvocation_ThroughAction_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation.CreateFor(14, 20));

		[Theory]
		[EmbeddedFileData("BaseActionHandlerInvocation_Bad.cs")]
		public async Task BaseActionInvocation_ThroughHandler_ReportsDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation.CreateFor(17, 20));

		[Theory]
		[EmbeddedFileData("BaseActionInvocation_Good.cs")]
		public async Task BaseActionInvocation_ThroughAction_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("BaseActionHandlerInvocation_Good.cs")]
		public async Task BaseActionInvocation_ThroughHandler_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("BaseActionHandlerOverrideInvocation_Good_Generic.cs")]
		public async Task OverriddenActionHandler_GenericExtensions_InvocationOfDotNet_base_Method_NoDiagnostic(string source) =>
			await VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("BaseActionHandlerOverrideInvocation_NonGeneric.cs")]
		public async Task OverriddenActionHandler_NonGenericExtensions_MixedStylesOfBaseHandlerInvocations(string source) =>
			await VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1091_CausingStackOverflowExceptionInBaseActionHandlerInvocation.CreateFor(43, 4));
	}
}