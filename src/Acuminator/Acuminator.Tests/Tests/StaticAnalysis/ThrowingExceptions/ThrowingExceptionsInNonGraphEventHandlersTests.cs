using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions
{
	public class ThrowingExceptionsInNonGraphEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
														 .WithIsvSpecificAnalyzersEnabled()
														 .WithRecursiveAnalysisEnabled()
														 .WithSuppressionMechanismDisabled(),
				new ThrowingExceptionsInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\NonGraph\ExceptionInRowPersisted.cs")]
		public async Task ExceptionInRowPersisted(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1073_ThrowingExceptionsInRowPersisted.CreateFor(25, 6));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\NonGraph\ExceptionInValidEventHandlers.cs")]
		public async Task ExceptionInValidEventHandlers_ShouldNotReportDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"EventHandlers\NonGraph\SetupNotEnteredExceptionInInvalidEventHandlers.cs")]
		public async Task SetupNotEnteredExceptionInInvalidEventHandlers(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(22, 4, EventType.FieldDefaulting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(27, 4, EventType.FieldVerifying),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(32, 4, EventType.RowSelecting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(37, 4, EventType.RowInserting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(42, 4, EventType.RowUpdating),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(47, 4, EventType.RowDeleting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(52, 4, EventType.RowInserted),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(57, 4, EventType.RowUpdated),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(62, 4, EventType.RowDeleted),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(67, 4, EventType.RowPersisting),

			Descriptors.PX1073_ThrowingExceptionsInRowPersisted.CreateFor(72, 4),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(72, 4, EventType.RowPersisted),

			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(76, 4, EventType.ExceptionHandling),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(79, 4, EventType.CommandPreparing),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(82, 4, EventType.FieldSelecting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(85, 4, EventType.FieldUpdating),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(88, 4, EventType.FieldUpdated));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\NonGraph\SetupNotEnteredExceptionInRowSelected.cs")]
		public async Task SetupNotEnteredExceptionInRowSelected_ShouldNotReportDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}
