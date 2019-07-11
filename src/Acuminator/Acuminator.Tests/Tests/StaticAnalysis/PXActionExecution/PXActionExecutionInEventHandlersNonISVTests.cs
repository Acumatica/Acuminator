using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXActionExecution;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXActionExecution
{
	public class PXActionExecutionInEventHandlersNonISVTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new EventHandlerAnalyzer(CodeAnalysisSettings.Default
					.WithRecursiveAnalysisEnabled()
					.WithIsvSpecificAnalyzersDisabled(),
				new PXActionExecutionInEventHandlersAnalyzer());

		[Theory]
		[EmbeddedFileData(@"EventHandlers\Press.cs")]
		public Task Press(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(71, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressOnDerivedType.cs")]
		public Task PressOnDerivedType(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(71, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressWithAdapter.cs")]
		public Task PressWithAdapter(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(71, 4));

		[Theory]
		[EmbeddedFileData(@"EventHandlers\PressWithExternalMethod.cs")]
		public Task PressWithExternalMethod(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(16, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(21, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(26, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(31, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(36, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(41, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(46, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(51, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(56, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(61, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(66, 4),
				Descriptors.PX1071_PXActionExecutionInEventHandlers_NonISV.CreateFor(71, 4));
	}
}