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
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.RowChangesInEventHandlers
{
	public class RowChangesInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(new RowChangesInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData("DirectAssignment.cs")]
		public Task DirectAssignment(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(14, 4, EventType.RowSelected),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(19, 4, EventType.FieldDefaulting),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(24, 4, EventType.FieldVerifying));
		}

		[Theory]
		[EmbeddedFileData("IndirectAssignment.cs")]
		public Task IndirectAssignment(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(15, 4, EventType.RowSelected),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(20, 4, EventType.FieldDefaulting),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(25, 4, EventType.FieldVerifying));
		}

		[Theory]
		[EmbeddedFileData("SetValue.cs")]
		public Task SetValue(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(14, 4, EventType.RowSelected),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(19, 4, EventType.FieldDefaulting),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(24, 4, EventType.FieldVerifying));
		}

		[Theory]
		[EmbeddedFileData("SetValueExt.cs")]
		public Task SetValueExt(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(14, 4, EventType.RowSelected),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(19, 4, EventType.FieldDefaulting),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(24, 4, EventType.FieldVerifying));
		}

		[Theory]
		[EmbeddedFileData("IndirectSetValue.cs")]
		public Task IndirectSetValue(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(15, 4, EventType.RowSelected),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(20, 4, EventType.FieldDefaulting),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(25, 4, EventType.FieldVerifying));
		}

		[Theory]
		[EmbeddedFileData("AssignmentInExternalMethod.cs")]
		public Task AssignmentInExternalMethod(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(16, 4, EventType.RowSelected),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(22, 4, EventType.FieldDefaulting),
				Descriptors.PX1047_RowChangesInEventHandlers.CreateFor(27, 4, EventType.FieldVerifying));
		}

		[Theory]
		[EmbeddedFileData("ValidEventHandlers.cs")]
		public Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual);
		}

		[Theory]
		[EmbeddedFileData("NonRowAssignment.cs")]
		public Task NonRowAssignment_ShouldNotShowDiagnostic(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual);
		}
	}
}
