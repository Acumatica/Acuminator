using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Verification
{
	/// <summary>
	/// Superclass of all Unit tests made for code refactorings.
	/// Contains methods used to verify correctness of refactorings
	/// </summary>
	public abstract class CodeRefactoringVerifier
	{
		/// <summary>
		/// Returns the refactoring being tested (C#) - to be implemented in non-abstract class
		/// </summary>
		/// <returns>The CodeFixRefactoring to be used for CSharp code</returns>
		protected virtual CodeRefactoringProvider GetCSharpCodeRefactoringProvider()
		{
			return null;
		}

		/// <summary>
		/// Returns the refactoring being tested (VB) - to be implemented in non-abstract class
		/// </summary>
		/// <returns>The CodeFixRefactoring to be used for CSharp code</returns>
		protected virtual CodeRefactoringProvider GetBasicCodeRefactoringProvider()
		{
			return null;
		}

		/// <summary>
		/// Called to test a C# refactoring when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the refactoring was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the refactoring was applied to it</param>
		/// <param name="codeRefactoringIndex">Index determining which refactoring to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the refactoring introduces other warnings after being applied</param>
		protected void VerifyCSharpRefactoring(string oldSource, string newSource, int? codeRefactoringIndex = null, bool allowNewCompilerDiagnostics = false)
		{
			VerifyRefactoring(LanguageNames.CSharp, GetCSharpCodeRefactoringProvider(), oldSource, newSource, codeRefactoringIndex, allowNewCompilerDiagnostics);
		}

		/// <summary>
		/// Called to test a VB refactoring when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the refactoring was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the refactoring was applied to it</param>
		/// <param name="codeRefactoringIndex">Index determining which refactoring to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the refactoring introduces other warnings after being applied</param>
		protected void VerifyBasicRefactoring(string oldSource, string newSource, int? codeRefactoringIndex = null, bool allowNewCompilerDiagnostics = false)
		{
			VerifyRefactoring(LanguageNames.VisualBasic, GetBasicCodeRefactoringProvider(), oldSource, newSource, codeRefactoringIndex, allowNewCompilerDiagnostics);
		}

		/// <summary>
		/// General verifier for refactorings.
		/// Creates a Document from the source string, then applies the relevant refactorings.
		/// Then gets the string after the refactoring is applied and compares it with the expected result.
		/// Note: If any refactoring causes new compiler diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
		/// </summary>
		/// <param name="language">The language the source code is in</param>
		/// <param name="codeRefactoringProvider">The refactoring to be applied to the code</param>
		/// <param name="oldSource">A class in the form of a string before the refactoring was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the refactoring was applied to it</param>
		/// <param name="codeRefactoringIndex">Index determining which refactoring to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the refactoring introduces other warnings after being applied</param>
		private void VerifyRefactoring(string language, CodeRefactoringProvider codeRefactoringProvider,
			string oldSource, string newSource, int? codeRefactoringIndex, bool allowNewCompilerDiagnostics)
		{
			var document = CreateDocument(oldSource, language);
			var compilerDiagnostics = GetCompilerDiagnostics(document);

			var actions = new List<CodeAction>();

			var documentSpan = document.GetSyntaxRootAsync().Result.FullSpan;
			var context = new CodeRefactoringContext(document, documentSpan, a => actions.Add(a), CancellationToken.None);
			codeRefactoringProvider.ComputeRefactoringsAsync(context).Wait();

			if (actions.Count == 0)
				return;

			document = ApplyCodeAction(document, codeRefactoringIndex != null 
				? actions[(int) codeRefactoringIndex] 
				: actions[0]);

			var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

			//check if applying the code fix introduced any new compiler diagnostics
			if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
			{
				// Format and get the compiler diagnostics again so that the locations make sense in the output
				document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
				newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

				Assert.True(false,
					string.Format("Refactoring introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
						string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
						document.GetSyntaxRootAsync().Result.ToFullString()));
			}

			//after applying all of the refactorings, compare the resulting string to the inputted one
			var actual = GetStringFromDocument(document);
			Assert.Equal(newSource, actual);
		}
	}
}