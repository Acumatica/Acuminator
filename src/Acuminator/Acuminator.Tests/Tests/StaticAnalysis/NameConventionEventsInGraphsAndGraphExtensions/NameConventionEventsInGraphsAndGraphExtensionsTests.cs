using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		
		[Theory]
		[EmbeddedFileData("CacheAttached.cs", "CacheAttached_Expected.cs")]
		public Task CacheAttached(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("RowEventHandler.cs", "RowEventHandler_Expected.cs")]
		public Task RowEventHandler(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("FieldEventHandler.cs", "FieldEventHandler_Expected.cs")]
		public Task FieldEventHandler(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("CacheAttachedWithArgUsages.cs", "CacheAttachedWithArgUsages_Expected.cs")]
		public Task CacheAttachedWithArgUsages(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		

		[Theory]
		[EmbeddedFileData("EventHandlerWithArgsUsages.cs", "EventHandlerWithArgsUsages_Expected.cs")]
		public Task EventHandlerWithArgsUsages(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
		
		[Theory]
		[EmbeddedFileData("AdditionalParameters.cs")]
		public Task AdditionalParameters_ShouldNotSuggestRefactoring(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);
		
		[Theory]
		[EmbeddedFileData("Override.cs")]
		public Task Override_ShouldNotSuggestRefactoring(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("PXOverride.cs")]
		public Task PXOverride_ShouldNotSuggestRefactoring(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);	
	}
}
