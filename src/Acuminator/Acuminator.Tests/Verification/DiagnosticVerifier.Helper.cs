using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using PX.Data;

namespace Acuminator.Tests.Verification
{
	/// <summary>
	/// Class for turning strings into documents and getting the diagnostics on them
	/// All methods are static
	/// </summary>
	public abstract partial class DiagnosticVerifier
	{
		private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
		private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
		private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
		private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
	    private static readonly MetadataReference PXDataReference = MetadataReference.CreateFromFile(typeof(PXGraph).Assembly.Location);
	    private static readonly MetadataReference PXCommonReference = MetadataReference.CreateFromFile(typeof(PX.Common.PXContext).Assembly.Location);

        internal static string DefaultFilePathPrefix = "Test";
		internal static string CSharpDefaultFileExt = "cs";
		internal static string VisualBasicDefaultExt = "vb";
		internal static string TestProjectName = "TestProject";

		#region  Get Diagnostics

		/// <summary>
		/// Given classes in the form of strings, their language, and an IDiagnosticAnlayzer to apply to it, return the diagnostics found in the string after converting it to a document.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source classes are in</param>
		/// <param name="analyzer">The analyzer to be run on the sources</param>
		/// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
		private static Diagnostic[] GetSortedDiagnostics(string[] sources, string language, DiagnosticAnalyzer analyzer)
		{
			return GetSortedDiagnosticsFromDocuments(analyzer, GetDocuments(sources, language));
		}

		/// <summary>
		/// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
		/// The returned diagnostics are then ordered by location in the source document.
		/// </summary>
		/// <param name="analyzer">The analyzer to run on the documents</param>
		/// <param name="documents">The Documents that the analyzer will be run on</param>
		/// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
		protected static Diagnostic[] GetSortedDiagnosticsFromDocuments(DiagnosticAnalyzer analyzer, Document[] documents)
		{
			var projects = new HashSet<Project>();
			foreach (var document in documents)
			{
				projects.Add(document.Project);
			}

			var diagnostics = new List<Diagnostic>();
			foreach (var project in projects)
			{
				var compilationWithAnalyzers = project.GetCompilationAsync().Result.WithAnalyzers(ImmutableArray.Create(analyzer));
				var diags = compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
				foreach (var diag in diags)
				{
					if (diag.Location == Location.None || diag.Location.IsInMetadata)
					{
						diagnostics.Add(diag);
					}
					else
					{
						for (int i = 0; i < documents.Length; i++)
						{
							var document = documents[i];
							var tree = document.GetSyntaxTreeAsync().Result;
							if (tree == diag.Location.SourceTree)
							{
								diagnostics.Add(diag);
							}
						}
					}
				}
			}

			var results = SortDiagnostics(diagnostics);
			diagnostics.Clear();
			return results;
		}

		/// <summary>
		/// Sort diagnostics by location in source document
		/// </summary>
		/// <param name="diagnostics">The list of Diagnostics to be sorted</param>
		/// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
		private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
		{
			return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
		}

		#endregion

		#region Set up compilation and documents
		/// <summary>
		/// Given an array of strings as sources and a language, turn them into a project and return the documents and spans of it.
		/// </summary>
		/// <param name="sources">Classes in the form of strings</param>
		/// <param name="language">The language the source code is in</param>
		/// <returns>A Tuple containing the Documents produced from the sources and their TextSpans if relevant</returns>
		private static Document[] GetDocuments(string[] sources, string language)
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
		/// <returns>A Document created from the source string</returns>
		protected static Document CreateDocument(string source, string InternalCode = @"", MetadataReference[] references = null, string language = LanguageNames.CSharp)
		{
			return CreateProject(new[] { source }, language, InternalCode, references).Documents.First();
		}

		protected static Document CreateCSharpDocument(string source, params string[] additionalSources)
		{
			var sources = new List<string>(additionalSources) { source };
			return CreateProject(sources.ToArray()).Documents.Last();
		}
        
        /// <summary>
        /// Create a project using the inputted strings as sources.
        /// </summary>
        /// <param name="sources">Classes in the form of strings</param>
        /// <param name="language">The language the source code is in</param>
        /// <returns>A Project created out of the Documents created from the source strings</returns>
        private static Project CreateProject(string[] sources, string language = LanguageNames.CSharp, string InternalCode = @"", MetadataReference[] references = null)
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
									.AddMetadataReference(projectId, PXCommonReference);
            
            //add check result execution BuildReferenceFromSource
            if (InternalCode.Length > 0)
            {
                ImmutableArray<byte> DynamicAssembly = BuildReferenceFromSource(InternalCode, references).ToImmutableArray();
                MetadataReference DynamicReference = MetadataReference.CreateFromImage(DynamicAssembly);

                solution = solution.AddMetadataReference(projectId,DynamicReference);
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

        private static byte[] BuildReferenceFromSource(string code, MetadataReference[] references = null)
        {

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            string assemblyName = Path.GetRandomFileName();
/*            MetadataReference[] references = new MetadataReference[]
            {
               // MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(PXGraph).Assembly.Location)
            };
            */
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // Emit the image of this assembly 
            byte[] image = null;
            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                {
                    throw new InvalidOperationException();//how i can return error message inside exception
                }
                image = ms.ToArray();
            }
            return image;

/*            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    return null;
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);
                    
                    foreach (Diagnostic diagnostic in failures)
                    {
                       
                        // Assert.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    return Assembly.Load(ms.ToArray());
                }
            }*/
            
        }
        #endregion
    }
}

