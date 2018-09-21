using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(new ChangesInPXCacheInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlers.cs")]
		public void TestDiagnostic_EventHandlers(string actual)
		{
			VerifyCSharpDiagnostic(actual, 
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(34, 4, EventType.RowInserting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(39, 4, EventType.RowUpdating),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(44, 4, EventType.RowDeleting));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlersWithExternalMethod.cs")]
		public void TestDiagnostic_EventHandlersWithExternalMethod(string actual)
		{
			VerifyCSharpDiagnostic(actual,
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(34, 4, EventType.RowInserting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(39, 4, EventType.RowUpdating),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(44, 4, EventType.RowDeleting));
		}

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ValidEventHandlers.cs")]
		public void TestDiagnostic_EventHandlers_ShouldNotShowDiagnostic(string actual)
		{
			VerifyCSharpDiagnostic(actual);
		}
	}
}
