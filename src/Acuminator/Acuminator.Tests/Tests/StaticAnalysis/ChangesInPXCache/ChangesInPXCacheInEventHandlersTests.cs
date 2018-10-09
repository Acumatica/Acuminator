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
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(
				CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(), 
				new ChangesInPXCacheInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlers.cs")]
		public async Task EventHandlers(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlersWithExternalMethod.cs")]
		public async Task EventHandlersWithExternalMethod(string actual) => 
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\TypedCache.cs")]
		public async Task TypedCache(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ExternalCache.cs")]
		public async Task ExternalCache(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ValidEventHandlers.cs")]
		public async Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}
