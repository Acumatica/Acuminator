using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions
{
	public class NameConventionEventsInGraphsAndGraphExtensionsTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
															.WithSuppressionMechanismDisabled(),
				new NameConventionEventsInGraphsAndGraphExtensionsAnalyzer());

		protected override CodeFixProvider GetCSharpCodeFixProvider() =>
			new NameConventionEventsInGraphsAndGraphExtensionsFix();

		#region Code Fix Tests
		[Theory]
		[EmbeddedFileData("CacheAttached.cs", "CacheAttached_Expected.cs")]
		public Task CacheAttached_Fix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("RowEventHandler.cs", "RowEventHandler_Expected.cs")]
		public Task RowEventHandler_Fix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("FieldEventHandler.cs", "FieldEventHandler_Expected.cs")]
		public Task FieldEventHandler_Fix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("CacheAttachedWithArgUsages.cs", "CacheAttachedWithArgUsages_Expected.cs")]
		public Task CacheAttachedWithArgUsages_Fix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("EventHandlerWithArgsUsages.cs", "EventHandlerWithArgsUsages_Expected.cs")]
		public Task EventHandlerWithArgsUsages_Fix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		#endregion

		#region Diagnostic Tests
		[Theory]
		[EmbeddedFileData("CacheAttached.cs")]
		public Task CacheAttached(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.CreateFor(12, 26));

		[Theory]
		[EmbeddedFileData("CacheAttached_Expected.cs")]
		public Task CacheAttached_AfterFix(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("RowEventHandler.cs")]
		public Task RowEventHandler(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.CreateFor(12, 26));


		[Theory]
		[EmbeddedFileData("FieldEventHandler.cs")]
		public Task FieldEventHandler(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.CreateFor(12, 26));


		[Theory]
		[EmbeddedFileData("CacheAttachedWithArgUsages.cs")]
		public Task CacheAttachedWithArgUsages(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.CreateFor(12, 26));


		[Theory]
		[EmbeddedFileData("EventHandlerWithArgsUsages.cs")]
		public Task EventHandlerWithArgsUsages(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1041_NameConventionEventsInGraphsAndGraphExtensions.CreateFor(12, 26));

		[Theory]
		[EmbeddedFileData("AdditionalParameters.cs")]
		public Task AdditionalParameters_ShouldNotSuggestRefactoring(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);
		
		[Theory]
		[EmbeddedFileData("Override.cs", "Override_DacAndBaseGraph.cs")]
		public Task Override_ShouldNotSuggestRefactoring(string actual, string dacAndBaseGraphSourceFile) =>
			VerifyCSharpDiagnosticAsync(actual, dacAndBaseGraphSourceFile);

		[Theory]
		[EmbeddedFileData("PXOverride.cs", "Override_DacAndBaseGraph.cs")]
		public Task PXOverride_ShouldNotSuggestRefactoring(string actual, string dacAndBaseGraphSourceFile) =>
			VerifyCSharpDiagnosticAsync(actual, dacAndBaseGraphSourceFile);
		#endregion
	}
}
