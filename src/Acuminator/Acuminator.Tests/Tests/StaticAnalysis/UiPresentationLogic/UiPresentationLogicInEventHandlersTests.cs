using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.UiPresentationLogic
{
	public class UiPresentationLogicInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled(),
				new UiPresentationLogicInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlers.cs")]
		public Task EventHandlers(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(16, 4, EventType.FieldDefaulting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(21, 4, EventType.FieldVerifying),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(26, 4, EventType.RowSelecting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(31, 4, EventType.RowInserting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(36, 4, EventType.RowUpdating),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(41, 4, EventType.RowDeleting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(46, 4, EventType.RowInserted),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(51, 4, EventType.RowUpdated),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(56, 4, EventType.RowDeleted),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(61, 4, EventType.RowPersisting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(66, 4, EventType.RowPersisted),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(67, 4, EventType.RowPersisted),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(68, 4, EventType.RowPersisted));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlersWithExternalMethod.cs")]
		public Task EventHandlersWithExternalMethod(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(16, 4, EventType.FieldDefaulting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(21, 4, EventType.FieldVerifying),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(26, 4, EventType.RowSelecting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(31, 4, EventType.RowInserting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(36, 4, EventType.RowUpdating),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(41, 4, EventType.RowDeleting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(46, 4, EventType.RowInserted),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(51, 4, EventType.RowUpdated),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(56, 4, EventType.RowDeleted),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(61, 4, EventType.RowPersisting),
				Descriptors.PX1070_UiPresentationLogicInEventHandlers.CreateFor(66, 4, EventType.RowPersisted));
		
		[Theory]
		[EmbeddedFileData(@"EventHandlers\ValidEventHandlers.cs")]
		public Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);
	}
}
