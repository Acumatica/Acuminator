using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.ForbidPrivateEventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.ForbidPrivateEventHandlers
{
	public class ForbidPrivateEventHandlersFixerTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new ForbidPrivateEventHandlersFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new ForbidPrivateEventHandlersAnalyzer());


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
		[EmbeddedFileData("ModifierComments.cs",
						  "ModifierComments_Expected.cs")]
		public async Task Test_Modifiers_With_Comments(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}

		[Theory]
		[EmbeddedFileData("AbstractHandler.cs",
						  "AbstractHandler_Expected.cs")]
		public async Task Test_Abstract_modifier(string actual, string expected)
		{
			await VerifyCSharpFixAsync(actual, expected);
		}
	}
}
