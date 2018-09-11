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
		[EmbeddedFileData("ValidEventHandlers.cs")]
		public Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual)
		{
			return VerifyCSharpDiagnosticAsync(actual);
		}
	}
}
