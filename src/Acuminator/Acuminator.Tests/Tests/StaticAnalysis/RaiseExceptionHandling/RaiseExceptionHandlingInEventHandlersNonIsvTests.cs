using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.RaiseExceptionHandling;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.RaiseExceptionHandling
{
	public class RaiseExceptionHandlingInEventHandlersNonIsvTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(),
				new RaiseExceptionHandlingInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlers.cs")]
		public async Task EventHandlers(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonIsv.CreateFor(19, 4, EventType.FieldSelecting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers.CreateFor(29, 4, EventType.RowPersisted));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlersWithExternalMethod.cs")]
		public async Task EventHandlersWithExternalMethod(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers_NonIsv.CreateFor(19, 4, EventType.FieldSelecting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
			Descriptors.PX1075_RaiseExceptionHandlingInEventHandlers.CreateFor(29, 4, EventType.RowPersisted));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ValidEventHandlers.cs")]
		public async Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}
