using Acuminator.Analyzers.StaticAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Acuminator.Tests.Tests.StaticAnalysis.SuppressionDiagnostics
{
	/// <summary>
	/// The test code fix to verify diagnostic suppression with a comment code fix .
	/// </summary>
	internal class SuppressDiagnosticTestCodeFix : SuppressDiagnosticFix
	{
		/// <summary>
		/// Gets code action to register. OVerrides the default method that returns code action with nested code actions. 
		/// The override returns just one code action with a "suppress with comment" code fix.
		/// </summary>
		/// <param name="diagnostic">The diagnostic.</param>
		/// <param name="context">The context.</param>
		/// <returns>
		/// The code action to register.
		/// </returns>
		protected override CodeAction GetCodeActionToRegister(Diagnostic diagnostic, CodeFixContext context) =>
			GetSuppressWithCommentCodeAction(diagnostic, context);
	}
}