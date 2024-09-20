using System.Threading.Tasks;
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
	public class EventHandlerModifierFixerTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new EventHandlerProtectedModifierFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new EventHandlerModifierAnalyzer());


		[Theory]
		[EmbeddedFileData("InvalidHandlerModifier.cs",
						  "InvalidHandlerModifier_Expected.cs")]
		public async Task Test_Invalid_Modifiers_In_Event_Handlers(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("ContainerWithInterface.cs",
						  "ContainerWithInterface_Expected.cs")]
		public async Task Test_Modifiers_With_Interfaces(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("SealedContainer.cs",
						  "SealedContainer_Expected.cs")]
		public async Task Test_Modifiers_With_Sealed_Class(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("PrivateModifier.cs",
						  "PrivateModifier_Expected.cs")]
		public async Task Test_Private_Modifiers_In_Event_Handlers(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("PrivateModifierComments.cs",
						  "PrivateModifierComments_Expected.cs")]
		public async Task Test_Modifiers_With_Comments(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}
	}
}
