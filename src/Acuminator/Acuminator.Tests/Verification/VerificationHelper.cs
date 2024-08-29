﻿
#nullable enable

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

using Acuminator.Utilities.Common;

using FbqlCommand = PX.Data.BQL.Fluent.FbqlCommand;

namespace Acuminator.Tests.Verification
{
	public static class VerificationHelper
	{
		private const string GeneratedFileNameSeed = "Test";
		private const string CSharpDefaultFileExt = "cs";
		private const string VisualBasicDefaultExt = "vb";
		private const string GeneratedProjectName = "GenProject";
		private const string BuildFailMessage = "External assembly build failure";

		private static readonly string DotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

		private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

		private static readonly MetadataReference PXDataReference = MetadataReference.CreateFromFile(typeof(PXGraph).Assembly.Location);
		private static readonly MetadataReference FluentBqlReference = MetadataReference.CreateFromFile(typeof(FbqlCommand).Assembly.Location);
		private static readonly MetadataReference PXCommonReference = MetadataReference.CreateFromFile(typeof(PX.Common.PXContext).Assembly.Location);
		private static readonly MetadataReference PXCommonStdReference = MetadataReference.CreateFromFile(typeof(PX.Common.PXInternalUseOnlyAttribute).Assembly.Location);
		private static readonly MetadataReference PXObjectsReference =
			MetadataReference.CreateFromFile(typeof(PX.Objects.GL.PeriodIDAttribute).Assembly.Location);

		private static readonly MetadataReference[] DotNetReferences;
		private static readonly MetadataReference[] MetadataReferences;
		
		private static readonly MetadataReference ExternalDependencyReference = 
			MetadataReference.CreateFromFile(typeof(ExternalDependency.NoBqlFieldForDacFieldProperty.BaseDacWithoutBqlField).Assembly.Location);

		static VerificationHelper()
		{
			List<MetadataReference> dotNetReferences = new(capacity: 5);

			AddMetadataReferenceIfDllExists(dotNetReferences, "mscorlib.dll");
			AddMetadataReferenceIfDllExists(dotNetReferences, "netstandard.dll");
			AddMetadataReferenceIfDllExists(dotNetReferences, "System.dll");
			AddMetadataReferenceIfDllExists(dotNetReferences, "System.Core.dll");
			AddMetadataReferenceIfDllExists(dotNetReferences, "System.Runtime.dll");

			DotNetReferences = dotNetReferences.ToArray();
			MetadataReferences = DotNetReferences.Concat(
			[
				CSharpSymbolsReference,
				CodeAnalysisReference,
				PXDataReference,
				PXCommonReference,
				PXCommonStdReference,
				PXObjectsReference,
				FluentBqlReference,
				ExternalDependencyReference
			])
			.ToArray();
		}

		private static void AddMetadataReferenceIfDllExists(List<MetadataReference> dotNetReferences, string dllName)
		{
			string pathToDll = Path.Combine(DotNetAssemblyPath, dllName);

			if (!File.Exists(pathToDll))
				return;

			try
			{
				var reference = MetadataReference.CreateFromFile(pathToDll);
				dotNetReferences.Add(reference);
			}
			catch (Exception)
			{
			}
		}

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
		/// <returns>A Document created from the source string</returns>
		public static Document CreateDocument(string source, string language = LanguageNames.CSharp, string[]? externalCode = null)
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
		/// <returns>A Project created out of the Documents created from the source strings</returns>
		private static Project CreateProject(string[] sources, string language = LanguageNames.CSharp, string[]? externalCode = null)
		{
			string fileNamePrefix = GeneratedFileNameSeed;
			string fileExt = language == LanguageNames.CSharp ? CSharpDefaultFileExt : VisualBasicDefaultExt;

			var projectId = ProjectId.CreateNewId(debugName: GeneratedProjectName);

			var workspace = new AdhocWorkspace(); //-V3114
			workspace.Options = workspace.Options.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, true)
												 .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, FormattingOptions.IndentStyle.Smart)
												 .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
												 .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, 4);

			var solution = workspace.CurrentSolution
									.AddProject(projectId, GeneratedProjectName, GeneratedProjectName, language)
									.AddMetadataReferences(projectId, MetadataReferences);
									
			if (externalCode != null && externalCode.Length > 0)
			{
				IEnumerable<byte> dynamicAssembly = BuildAssemblyFromSources(externalCode);
				MetadataReference dynamicReference = MetadataReference.CreateFromImage(dynamicAssembly);
				solution = solution.AddMetadataReference(projectId, dynamicReference);
			}

			var project = solution.GetProject(projectId);

			// Prepare C# parsing and compilation options
			var parseOptions = CreateParseOptions(project);
			solution = solution.WithProjectParseOptions(projectId, parseOptions);

			CSharpCompilationOptions compileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary); // compile to a dll
			solution = solution.WithProjectCompilationOptions(projectId, compileOptions);

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

		private static ParseOptions CreateParseOptions(Project project)
		{
			var customeFeatures = new[] { KeyValuePair.Create("IOperation", "true") };
			var projectFeatures = project.ParseOptions.Features.Union(customeFeatures);

			return new CSharpParseOptions()
						.WithKind(SourceCodeKind.Regular)			// as representing a complete .cs file
						.WithLanguageVersion(LanguageVersion.Latest)
						.WithFeatures(projectFeatures);				// enabling the latest language features
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
		/// <param name="document">The Document to run the compiler diagnostic analyzers on.</param>
		/// <param name="ignoreHiddenDiagnostics">(Optional) True to ignore hidden diagnostics.</param>
		/// <returns>
		/// The compiler diagnostics that were found in the code.
		/// </returns>
		public static async Task<IEnumerable<Diagnostic>> GetCompilerDiagnosticsAsync(Document document, bool ignoreHiddenDiagnostics = true)
		{
			var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
			IEnumerable<Diagnostic> diagnostics = semanticModel.GetDiagnostics();

			if (ignoreHiddenDiagnostics)
			{
				diagnostics = diagnostics.Where(d => d.DefaultSeverity != DiagnosticSeverity.Hidden);
			}

			return diagnostics;
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
					references: MetadataReferences,
					options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			// Emit the image of this assembly 
			byte[]? image = null;

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
