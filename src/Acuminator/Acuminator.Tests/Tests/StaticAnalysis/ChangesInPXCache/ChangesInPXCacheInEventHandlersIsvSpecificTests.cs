using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.ChangesInPXCache;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ChangesInPXCache
{
	public class ChangesInPXCacheInEventHandlersIsvSpecificTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(
				CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersEnabled(), 
				new ChangesInPXCacheInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlers.cs")]
		public async Task EventHandlers(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(34, 4, EventType.RowInserting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(39, 4, EventType.RowUpdating),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(44, 4, EventType.RowDeleting));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\EventHandlersWithExternalMethod.cs")]
		public async Task EventHandlersWithExternalMethod(string actual) => 
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(34, 4, EventType.RowInserting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(39, 4, EventType.RowUpdating),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(44, 4, EventType.RowDeleting));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\TypedCache.cs")]
		public async Task TypedCache(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.FieldDefaulting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.FieldVerifying),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowSelecting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(29, 4, EventType.RowSelected),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(34, 4, EventType.RowInserting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(39, 4, EventType.RowUpdating),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(44, 4, EventType.RowDeleting));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ExternalCache.cs")]
		public async Task ExternalCache(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(14, 4, EventType.RowInserting),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(19, 4, EventType.RowUpdating),
				Descriptors.PX1044_ChangesInPXCacheInEventHandlers.CreateFor(24, 4, EventType.RowDeleting));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\ValidEventHandlers.cs")]
		public async Task ValidEventHandlers_ShouldNotShowDiagnostic(string actual) =>
			await VerifyCSharpDiagnosticAsync(actual);
	}
}