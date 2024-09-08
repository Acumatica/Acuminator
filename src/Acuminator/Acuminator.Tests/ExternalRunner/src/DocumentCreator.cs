#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace ExternalRunner
{
	public static class DocumentCreator
	{
		private const string GeneratedFileNameSeed = "Test";
		private const string CSharpDefaultFileExt = "cs";
		private const string VisualBasicDefaultExt = "vb";
		private const string GeneratedProjectName = "GenProject";
		private const string BuildFailMessage = "External assembly build failure";

		private static readonly string DotNetAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

		private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

		private static readonly MetadataReference[] DotNetReferences;
		private static readonly MetadataReference[] MetadataReferences;

		static DocumentCreator()
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
				CodeAnalysisReference
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
			var workspaceOptions = workspace.Options.WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, true)
													.WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp, FormattingOptions.IndentStyle.Smart)
													.WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
													.WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, 4);

			workspace.TryApplyChanges(workspace.CurrentSolution.WithOptions(workspaceOptions));

			var solution = workspace.CurrentSolution
									.AddProject(projectId, GeneratedProjectName, GeneratedProjectName, language)
									.AddMetadataReferences(projectId, MetadataReferences);

			if (externalCode != null && externalCode.Length > 0)
			{
				IEnumerable<byte> dynamicAssembly = BuildAssemblyFromSources(externalCode);
				MetadataReference dynamicReference = MetadataReference.CreateFromImage(dynamicAssembly);
				solution = solution.AddMetadataReference(projectId, dynamicReference);
			}

			var project = solution.GetProject(projectId).CheckIfNull();

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

			return solution.GetProject(projectId).CheckIfNull();
		}

		private static ParseOptions CreateParseOptions(Project project)
		{
			var customFeatures = new[] { KeyValuePair.Create("IOperation", "true") };
			var projectFeatures = project.ParseOptions?.Features.Union(customFeatures) ?? [];

			return new CSharpParseOptions()
						.WithKind(SourceCodeKind.Regular)           // as representing a complete .cs file
						.WithLanguageVersion(LanguageVersion.Latest)
						.WithFeatures(projectFeatures);             // enabling the latest language features
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
