using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using PX.Data;

using FbqlCommand = PX.Data.BQL.Fluent.FbqlCommand;


namespace Acuminator.Tests.Verification
{
	public static class VerificationHelper
	{
		private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
		private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
		private static readonly MetadataReference PXDataReference = MetadataReference.CreateFromFile(typeof(PXGraph).Assembly.Location);
		private static readonly MetadataReference FluentBqlReference = MetadataReference.CreateFromFile(typeof(FbqlCommand).Assembly.Location);
		private static readonly MetadataReference PXCommonReference = MetadataReference.CreateFromFile(typeof(PX.Common.PXContext).Assembly.Location);

		internal static string DefaultFilePathPrefix = "Test";
		internal static string CSharpDefaultFileExt = "cs";
		internal static string VisualBasicDefaultExt = "vb";
		internal static string TestProjectName = "TestProject";
		internal static string BuildFailMessage = "External assembly build failure";


		/// <summary>
		/// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source code is in</param>
		/// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
		public static Document[] GetDocuments(string[] sources, string language)
		{
			if (language != LanguageNames.CSharp && language != LanguageNames.VisualBasic)
			{
				throw new ArgumentException("Unsupported Language");
			}

			var project = CreateProject(sources, language);
			var documents = project.Documents.ToArray();

			if (sources.Length != documents.Length)
			{
				throw new SystemException("Amount of sources did not match amount of Documents created");
			}

			return documents;
		}

		/// <summary>
		/// Create a Document from a string through creating a project that contains it.
		/// </summary>
		/// <param name="source">Classes in the form of a string</param>
		/// <param name="language">The language the source code is in</param>
		/// <param name="externalCode">The source codes for new memory compilation. The goal of the external code is to simulate the behaviour of the extenal assembly without source code.</param>
		/// <param name="compiledAssemblies">The compiled libraries for usage in test. The goal of the library is to simulate the behaviour of the extenal assembly without source code.</param>
		/// <returns>A Document created from the source string</returns>
		public static Document CreateDocument(string source, string language = LanguageNames.CSharp, string[] externalCode = null)
		{
			return CreateProject(new[] { source }, language, externalCode).Documents.First();
		}

		public static Document CreateCSharpDocument(string source, params string[] additionalSources)
		{
			var sources = new List<string>(additionalSources) { source };
			return CreateProject(sources.ToArray()).Documents.Last();
		}

		/// <summary>
		/// Create a project using the inputted strings as sources.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source code is in</param>
		/// <param name="externalCode">The source codes for new memory compilation. The goal of the external code is to simulate the behaviour of the extenal assembly without source code.</param>
		/// <param name="compiledAssemblies">The compiled libraries for usage in test. The goal of the library is to simulate the behaviour of the extenal assembly without source code.</param>
		/// <returns>A Project created out of the Documents created from the source strings</returns>
		private static Project CreateProject(string[] sources, string language = LanguageNames.CSharp, string[] externalCode = null)
		{
			string fileNamePrefix = DefaultFilePathPrefix;
			string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

			var projectId = ProjectId.CreateNewId(debugName: TestProjectName);

			var workspace = new AdhocWorkspace();
			workspace.Options = workspace.Options.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, true)
												 .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, FormattingOptions.IndentStyle.Smart)
												 .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
												 .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, 4);

			var solution = workspace.CurrentSolution
									.AddProject(projectId, TestProjectName, TestProjectName, language)
									.AddMetadataReference(projectId, CorlibReference)
									.AddMetadataReference(projectId, SystemCoreReference)
									.AddMetadataReference(projectId, CSharpSymbolsReference)
									.AddMetadataReference(projectId, CodeAnalysisReference)
									.AddMetadataReference(projectId, PXDataReference)
									.AddMetadataReference(projectId, FluentBqlReference)
									.AddMetadataReference(projectId, PXCommonReference);

			if (externalCode != null && externalCode.Length > 0)
			{
				IEnumerable<byte> dynamicAssembly = BuildAssemblyFromSources(externalCode);
				MetadataReference dynamicReference = MetadataReference.CreateFromImage(dynamicAssembly);
				solution = solution.AddMetadataReference(projectId, dynamicReference);
			}

			var project = solution.GetProject(projectId);
			var parseOptions = project.ParseOptions.WithFeatures(
				project.ParseOptions.Features.Union(new[] { new KeyValuePair<string, string>("IOperation", "true") }));
			solution = solution.WithProjectParseOptions(projectId, parseOptions);

			int count = 0;

			foreach (var source in sources)
			{
				var newFileName = fileNamePrefix + count + "." + fileExt;
				var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
				solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));
				count++;
			}

			return solution.GetProject(projectId);
		}

		/// <summary>
		/// Given a document, turn it into a string based on the syntax root
		/// </summary>
		/// <param name="document">The Document to be converted to a string</param>
		/// <returns>A string containing the syntax of the Document after formatting</returns>
		public static async Task<string> GetStringFromDocumentAsync(Document document)
		{
			var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation).ConfigureAwait(false);
			var root = await simplifiedDoc.GetSyntaxRootAsync().ConfigureAwait(false);
			root = Formatter.Format(root, Formatter.Annotation, simplifiedDoc.Project.Solution.Workspace);
			return root.GetText().ToString();
		}

		/// <summary>
		/// Apply the inputted CodeAction to the inputted document.
		/// </summary>
		/// <param name="document">The Document to apply the fix on</param>
		/// <param name="codeAction">A CodeAction that will be applied to the Document.</param>
		/// <returns>A Document with the changes from the CodeAction</returns>
		public static async Task<Document> ApplyCodeActionAsync(Document document, CodeAction codeAction)
		{
			var operations = await codeAction.GetOperationsAsync(CancellationToken.None).ConfigureAwait(false);
			var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
			return solution.GetDocument(document.Id);
		}


		/// <summary>
		/// Compare two collections of Diagnostics,and return a list of any new diagnostics that appear only in the second collection.
		/// Note: Considers Diagnostics to be the same if they have the same Ids.  In the case of multiple diagnostics with the same Id in a row,
		/// this method may not necessarily return the new one.
		/// </summary>
		/// <param name="diagnostics">The Diagnostics that existed in the code before the CodeFix was applied</param>
		/// <param name="newDiagnostics">The Diagnostics that exist in the code after the CodeFix was applied</param>
		/// <returns>A list of Diagnostics that only surfaced in the code after the CodeFix was applied</returns>
		public static IEnumerable<Diagnostic> GetNewDiagnostics(IEnumerable<Diagnostic> diagnostics, IEnumerable<Diagnostic> newDiagnostics)
		{
			var oldArray = diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
			var newArray = newDiagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();

			int oldIndex = 0;
			int newIndex = 0;

			while (newIndex < newArray.Length)
			{
				if (oldIndex < oldArray.Length && oldArray[oldIndex].Id == newArray[newIndex].Id)
				{
					++oldIndex;
					++newIndex;
				}
				else
				{
					yield return newArray[newIndex++];
				}
			}
		}

		/// <summary>
		/// Get the existing compiler diagnostics on the inputted document.
		/// </summary>
		/// <param name="document">The Document to run the compiler diagnostic analyzers on</param>
		/// <returns>The compiler diagnostics that were found in the code</returns>
		public static async Task<IEnumerable<Diagnostic>> GetCompilerDiagnosticsAsync(Document document)
		{
			var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			return semanticModel.GetDiagnostics();
		}

		/// <summary>
		/// Compile internal source code into memory array 
		/// </summary>
		/// <param name="sourceCodes">Source codes of library</param>
		/// <returns>Byte array from compiled binary dll assembly</returns>
		private static byte[] BuildAssemblyFromSources(string[] sourceCodes)
		{
			string assemblyName = Path.GetRandomFileName();
			CSharpCompilation compilation = CSharpCompilation.Create(
					assemblyName,
					syntaxTrees: sourceCodes.Select(code => CSharpSyntaxTree.ParseText(text: code)),
					references: new MetadataReference[] { CorlibReference,
														  SystemCoreReference,
														  CSharpSymbolsReference,
														  CodeAnalysisReference,
														  PXDataReference,
														  PXCommonReference },
					options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			// Emit the image of this assembly 
			byte[] image = null;

			using (var ms = new MemoryStream())
			{
				var emitResult = compilation.Emit(ms);

				if (!emitResult.Success)
				{
					StringBuilder diagnosticMessages = new StringBuilder(BuildFailMessage + "\r\n");
					
					IEnumerable<Diagnostic> failures = emitResult.Diagnostics.Where(diagnostic =>
															diagnostic.IsWarningAsError ||
															diagnostic.Severity == DiagnosticSeverity.Error);

					foreach (Diagnostic diagnostic in failures)
					{
						diagnosticMessages.Append(string.Format("{0}: {1} \r\n", diagnostic.Id, diagnostic.GetMessage()));
					}

					throw new ArgumentException(diagnosticMessages.ToString());
				}

				image = ms.ToArray();
			}

			return image;
		}
	}
}
