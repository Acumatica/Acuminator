using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent; 
using System.Linq;
using System.Threading.Tasks;

using Path = System.IO.Path;


namespace PX.Analyzers.Coloriser
{
    public class ParsedDocument
    {
        private static MetadataReference PXDataReference { get; } = 
            MetadataReference.CreateFromFile(typeof(PX.Data.PXGraph).Assembly.Location);

        private static readonly HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs"
        };

        public Workspace Workspace { get; }

        public Document Document { get; }

        public SemanticModel SemanticModel { get; }

        public SyntaxNode SyntaxRoot { get; }

        public ITextSnapshot Snapshot { get; }

        //internal ParsedSymbolsCache SymbolsCache { get; }

        public ParsedDocument(Workspace workspace, Document document, SemanticModel semanticModel, SyntaxNode syntaxRoot,
                               ITextSnapshot snapshot)
        {
            Workspace = workspace;
            Document = document;
            SyntaxRoot = syntaxRoot;
            SemanticModel = semanticModel;
            Snapshot = snapshot;
            //SymbolsCache = new ParsedSymbolsCache();
        }

        public static async Task<ParsedDocument> Resolve(ITextBuffer buffer, ITextSnapshot snapshot)
        {
            Workspace workspace = buffer.GetWorkspace();
            Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            
            if (document == null || !IsSupportedFileType(document) || !document.SupportsSemanticModel || 
                !document.SupportsSyntaxTree)
            {     
                return null;        // Razor cshtml returns a null document for some reason.
            }

            if (document.TryGetSemanticModel(out SemanticModel semanticModel) && 
                document.TryGetSyntaxRoot(out SyntaxNode syntaxRoot))
            {         
                return new ParsedDocument(workspace, document, semanticModel, syntaxRoot, snapshot);
            }

            // the ConfigureAwait() calls are important, otherwise we'll deadlock VS
            Task<SemanticModel> semanticModelTask = document.GetSemanticModelAsync();
            Task<SyntaxNode> syntaxRootTask = document.GetSyntaxRootAsync();

            await Task.WhenAll(semanticModelTask, syntaxRootTask)
                      .ConfigureAwait(continueOnCapturedContext: false);

            syntaxRoot = syntaxRootTask.Result;

            //Add reference to PX.Data
            Compilation newCompilation = semanticModelTask.Result.Compilation.AddReferences(PXDataReference);
            semanticModel = newCompilation.GetSemanticModel(syntaxRoot.SyntaxTree);

            return new ParsedDocument(workspace, document, semanticModel, syntaxRoot, snapshot);
        }

        private static bool IsSupportedFileType(Document document) => allowedExtensions.Contains(Path.GetExtension(document.FilePath));       
    }
}
