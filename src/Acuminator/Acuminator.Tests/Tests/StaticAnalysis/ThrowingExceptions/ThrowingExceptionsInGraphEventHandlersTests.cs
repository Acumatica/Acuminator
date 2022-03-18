using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ThrowingExceptions
{
	public class ThrowingExceptionsInGraphEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default
													.WithIsvSpecificAnalyzersEnabled()
													.WithRecursiveAnalysisEnabled()
													.WithSuppressionMechanismDisabled(),
				new ThrowingExceptionsInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Graph\ExceptionInRowPersisted.cs")]
		public async Task ExceptionInRowPersisted(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1073_ThrowingExceptionsInRowPersisted.CreateFor(14, 4),
			Descriptors.PX1073_ThrowingExceptionsInRowPersisted.CreateFor(20, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Graph\ExceptionInValidEventHandlers.cs")]
		public async Task ExceptionInValidEventHandlers_ShouldNotReportDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Graph\SetupNotEnteredExceptionInInvalidEventHandlers.cs")]
		public async Task SetupNotEnteredExceptionInInvalidEventHandlers(string actual) => await VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(29, 4, EventType.RowInserting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(34, 4, EventType.RowUpdating),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(39, 4, EventType.RowDeleting),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(44, 4, EventType.RowInserted),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(49, 4, EventType.RowUpdated),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(54, 4, EventType.RowDeleted),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(59, 4, EventType.RowPersisting),
			Descriptors.PX1073_ThrowingExceptionsInRowPersisted.CreateFor(64, 4),
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers.CreateFor(64, 4, EventType.RowPersisted));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Graph\SetupNotEnteredExceptionInRowSelected.cs")]
		public async Task SetupNotEnteredExceptionInRowSelected_ShouldNotReportDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}
