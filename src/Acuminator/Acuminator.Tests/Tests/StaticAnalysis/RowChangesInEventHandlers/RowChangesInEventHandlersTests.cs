using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.RowChangesInEventHandlers
{
	public class RowChangesInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default.WithRecursiveAnalysisEnabled(), 
				new RowChangesInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData("DirectAssignment.cs")]
		public Task DirectAssignment(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(14, 4, EventType.RowSelected),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(19, 4, EventType.FieldDefaulting),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(24, 4, EventType.FieldVerifying));

		[Theory]
		[EmbeddedFileData("IndirectAssignment.cs")]
		public Task IndirectAssignment(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(16, 4, EventType.RowSelected),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(22, 4, EventType.FieldDefaulting),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(28, 4, EventType.FieldVerifying));

		[Theory]
		[EmbeddedFileData("SetValue.cs")]
		public Task SetValue(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(14, 4, EventType.RowSelected),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(19, 4, EventType.FieldDefaulting),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(24, 4, EventType.FieldVerifying));

		[Theory]
		[EmbeddedFileData("SetValueExt.cs")]
		public Task SetValueExt(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(14, 4, EventType.RowSelected),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(19, 4, EventType.FieldDefaulting),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(24, 4, EventType.FieldVerifying));

		[Theory]
		[EmbeddedFileData("IndirectSetValue.cs")]
		public Task IndirectSetValue(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(16, 4, EventType.RowSelected),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(22, 4, EventType.FieldDefaulting),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(28, 4, EventType.FieldVerifying));

		[Theory(Skip = "Recursive analysis is too complicated for this diagnostic and is skipped for now")]
		[EmbeddedFileData("AssignmentInExternalMethod.cs")]
		public Task AssignmentInExternalMethod(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(16, 4, EventType.RowSelected),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(22, 4, EventType.FieldDefaulting),
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs.CreateFor(27, 4, EventType.FieldVerifying));

		[Theory]
		[EmbeddedFileData("ValidEventHandlers.cs")]
		public Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("NonRowAssignment.cs")]
		public Task NonRowAssignment_ShouldNotShowDiagnostic(string actual) => VerifyCSharpDiagnosticAsync(actual);

		
		[Theory]
		[EmbeddedFileData(@"Reversed\DirectAssignment.cs")]
		public Task Reversed_DirectAssignment(string actual) => VerifyCSharpDiagnosticAsync(actual, 
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(17, 4, EventType.RowInserting),
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(23, 4, EventType.RowSelecting));

		[Theory]
		[EmbeddedFileData(@"Reversed\IndirectAssignment.cs")]
		public Task Reversed_IndirectAssignment(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(18, 4, EventType.RowInserting),
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(25, 4, EventType.RowSelecting));

		[Theory]
		[EmbeddedFileData(@"Reversed\SetValue.cs")]
		public Task Reversed_SetValue(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(17, 4, EventType.RowInserting),
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(23, 4, EventType.RowSelecting));

		[Theory]
		[EmbeddedFileData(@"Reversed\SetValueExt.cs")]
		public Task Reversed_SetValueExt(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(17, 4, EventType.RowInserting),
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(23, 4, EventType.RowSelecting));

		[Theory]
		[EmbeddedFileData(@"Reversed\IndirectSetValue.cs")]
		public Task Reversed_IndirectSetValue(string actual) => VerifyCSharpDiagnosticAsync(actual,
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(19, 4, EventType.RowInserting),
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly.CreateFor(27, 4, EventType.RowSelecting));
	}
}
