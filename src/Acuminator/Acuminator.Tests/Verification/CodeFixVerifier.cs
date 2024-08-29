#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;

using Xunit;

using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Verification
{
	/// <summary>
	/// Superclass of all Unit tests made for diagnostics with codefixes.
	/// Contains methods used to verify correctness of codefixes
	/// </summary>
	public abstract class CodeFixVerifier : DiagnosticVerifier
	{
		protected const int MaxAttemptsLimit = 100; 

		/// <summary>
		/// Returns the codefix being tested (C#) - to be implemented in non-abstract class
		/// </summary>
		/// <returns>The CodeFixProvider to be used for CSharp code</returns>
		protected abstract CodeFixProvider GetCSharpCodeFixProvider();

		/// <summary>
		/// Called to test a C# codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected Task VerifyCSharpFixAsync(string oldSource, string newSource, int codeFixIndex = 0, bool allowNewCompilerDiagnostics = false)
		{
			return VerifyFixAsync(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource,
									allowNewCompilerDiagnostics, codeFixIndex);
		}

		/// <summary>
		/// Called to test a C# codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Ok in unit tests")]
		protected void VerifyCSharpFix(string oldSource, string newSource, int codeFixIndex = 0, bool allowNewCompilerDiagnostics = false)
		{
			VerifyFixAsync(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource,
							allowNewCompilerDiagnostics, codeFixIndex).Wait();
		}

		/// <summary>
		/// General verifier for codefixes.
		/// Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
		/// Then gets the string after the codefix is applied and compares it with the expected result.
		/// Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
		/// </summary>
		/// <param name="language">The language the source code is in</param>
		/// <param name="analyzer">The analyzer to be applied to the source code</param>
		/// <param name="codeFixProvider">The codefix to be applied to the code wherever the relevant Diagnostic is found</param>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		private async Task VerifyFixAsync(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, 
										  string oldSource, string newSource, bool allowNewCompilerDiagnostics, int codeFixIndex = 0)
		{
			var document = CreateDocument(oldSource, language);
			var analyzerDiagnostics = await GetSortedDiagnosticsFromDocumentsAsync(analyzer, new[] { document }, checkOnlyFirstDocument: true).ConfigureAwait(false);
			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			int? attempts = GetAttemptsCount(analyzerDiagnostics.Length);
			int diagnosticToUseIndex = 0;
			int counter = 0;

			while (attempts == null || attempts.Value > 0)
			{
				attempts = attempts - 1;
				counter++;

				if (counter > MaxAttemptsLimit)
					throw new InvalidOperationException($"Exceeded maximum attempts limit of {MaxAttemptsLimit}.");

				if (diagnosticToUseIndex > (analyzerDiagnostics.Length - 1))
					break;

				var actions = new List<CodeAction>();
				var context = new CodeFixContext(document, analyzerDiagnostics[diagnosticToUseIndex], (a, d) => actions.Add(a), CancellationToken.None);
				await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

				if (!actions.Any())
				{
					diagnosticToUseIndex++;		// increase diagnostic counter to try next found diagnostic
					continue;
				}

				if (codeFixIndex >= actions.Count)
					throw new InvalidOperationException($"Code fix index value \"{codeFixIndex}\" is out of bounds of the {nameof(actions)} collection.");

				document = await ApplyCodeActionAsync(document, actions[codeFixIndex]).ConfigureAwait(false);		
				analyzerDiagnostics = await GetSortedDiagnosticsFromDocumentsAsync(analyzer, [document], checkOnlyFirstDocument: true).ConfigureAwait(false);

				// Reset diagnostic index to use to try first diagnostic again
				diagnosticToUseIndex = 0;

				var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

				//check if applying the code fix introduced any new compiler diagnostics
				if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
				{
					// Format and get the compiler diagnostics again so that the locations make sense in the output
					var changedRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
					var formattedRoot = Formatter.Format(changedRoot.CheckIfNull(), Formatter.Annotation, document.Project.Solution.Workspace);

					document = document.WithSyntaxRoot(formattedRoot);
					newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

					var formattedDocumentRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
					var documentString = formattedDocumentRoot?.ToFullString() ?? "Failed to obtain the changed document";

					Assert.True(false,
						string.Format($"Fix introduced new compiler diagnostics:{Environment.NewLine}{{0}}{Environment.NewLine}{Environment.NewLine}"+
									  $"New document:{Environment.NewLine}{{1}}{Environment.NewLine}",
									  newCompilerDiagnostics.Select(d => d.ToString()).Join(Environment.NewLine),
									  documentString));
				}

				//check if there are analyzer diagnostics left after the code fix
				if (!analyzerDiagnostics.Any())
				{
					break;
				}
			}

			//after applying all of the code fixes, compare the resulting string to the inputted one
			var actual = await GetStringFromDocumentAsync(document).ConfigureAwait(false);
			Assert.Equal(newSource, actual);
		}

		/// <summary>
		/// Gets attempts count. If returns null then code fix is applied infinitely as long as there are analyzer diagnostics.
		/// </summary>
		/// <param name="initialDiagnosticCount">Number of initial analyzer diagnostics.</param>
		/// <returns>
		/// The attempts count.
		/// </returns>
		protected virtual int? GetAttemptsCount(int initialDiagnosticCount) => initialDiagnosticCount;
	}
}