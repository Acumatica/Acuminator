using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;

using Path = System.IO.Path;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Coloriser
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

        public SyntaxNode SyntaxRoot { get; }

        public ITextSnapshot Snapshot { get; }


        public ParsedDocument(Workspace workspace, Document document, SemanticModel semanticModel, SyntaxNode syntaxRoot,
                               ITextSnapshot snapshot)
        {
            Workspace = workspace;
            Document = document;
            SyntaxRoot = syntaxRoot;
            Snapshot = snapshot;     
        }

		public SemanticModel GetSemanticModel(CancellationToken cancellationToken = default) =>
			Document.TryGetSemanticModel(out var semanticModel)
				? semanticModel
				: ThreadHelper.JoinableTaskFactory.Run(() =>  Document.GetSemanticModelAsync(cancellationToken));

		public static async Task<ParsedDocument> ResolveAsync(ITextBuffer buffer, ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

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

            if (cancellationToken.IsCancellationRequested)
                return null;

            Task<SemanticModel> semanticModelTask = document.GetSemanticModelAsync(cancellationToken);
            Task<SyntaxNode> syntaxRootTask = document.GetSyntaxRootAsync(cancellationToken);

            bool success = await Task.WhenAll(semanticModelTask, syntaxRootTask)
                                     .TryAwait()
									 .ConfigureAwait(false);
            if (!success)
                return null;

            if (!semanticModelTask.IsCompleted || !syntaxRootTask.IsCompleted || cancellationToken.IsCancellationRequested)
            {
                return null;
            }

			#pragma warning disable VSTHRD103 // Call async methods when in an async method - task already finished
			syntaxRoot = syntaxRootTask.Result;	
			Compilation newCompilation = semanticModelTask.Result.Compilation.AddReferences(PXDataReference);   //Add reference to PX.Data
			#pragma warning restore VSTHRD103 

			if (cancellationToken.IsCancellationRequested)
                return null;

            semanticModel = newCompilation.GetSemanticModel(syntaxRoot.SyntaxTree);

            if (cancellationToken.IsCancellationRequested)
                return null;

            return new ParsedDocument(workspace, document, semanticModel, syntaxRoot, snapshot);
        }

        private static bool IsSupportedFileType(Document document) => allowedExtensions.Contains(Path.GetExtension(document.FilePath));       
    }
}
