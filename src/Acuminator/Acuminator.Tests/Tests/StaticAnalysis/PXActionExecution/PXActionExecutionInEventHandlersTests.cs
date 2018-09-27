using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Analyzers.StaticAnalysis.UiPresentationLogic;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class PXActionExecutionInEventHandlersTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled(),
				new PXActionExecutionInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Press.cs")]
		public Task Press(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(71, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressOnDerivedType.cs")]
		public Task PressOnDerivedType(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(71, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressWithAdapter.cs")]
		public Task PressWithAdapter(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(71, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressWithExternalMethod.cs")]
		public Task PressWithExternalMethod(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers.CreateFor(71, 4));
	}
}
