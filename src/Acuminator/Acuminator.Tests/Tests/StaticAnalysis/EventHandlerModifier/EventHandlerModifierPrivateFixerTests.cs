using Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier;
using Acuminator.Analyzers.StaticAnalysis.PrivateEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.EventHandlerModifier
{
	public class EventHandlerModifierPrivateFixerTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new EventHandlerPrivateModifierFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new EventHandlerModifierAnalyzer());


		[Theory]
		[EmbeddedFileData("PrivateModifier.cs",
						  "PrivateModifier_Expected.cs")]
		public void Test_Invalid_Modifiers_In_Event_Handlers(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ContainerWithInterface.cs",
						  "ContainerWithInterface_Expected.cs")]
		public void Test_Modifiers_With_Interfaces(string actual, string expected)
		{
			VerifyCSharpFix(actual, expected);
		}
	}
}
