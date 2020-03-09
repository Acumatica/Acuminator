using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using Path = System.IO.Path;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Coloriser
{
	public class ParsedDocument
    {
        private static readonly HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs"
        };

        public Workspace Workspace { get; }

        public Document Document { get; }

        public SyntaxNode SyntaxRoot { get; }

        public ITextSnapshot Snapshot { get; }


        public ParsedDocument(Workspace workspace, Document document, SyntaxNode syntaxRoot,
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

		public static async Task<ParsedDocument> ResolveAsync(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            Workspace workspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync()
																	.ConfigureAwait(false);
			Document document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            
            if (document == null || !IsSupportedFileType(document) || !document.SupportsSemanticModel || 
                !document.SupportsSyntaxTree)
            {     
                return null;        // Razor cshtml returns a null document for some reason.
            }

            if (document.TryGetSyntaxRoot(out SyntaxNode syntaxRoot))
            {         
                return new ParsedDocument(workspace, document, syntaxRoot, snapshot);
            }

            if (cancellationToken.IsCancellationRequested)
                return null;

			TaskResult<SyntaxNode> taskResult = await document.GetSyntaxRootAsync(cancellationToken)
															  .TryAwait()
															  .ConfigureAwait(false);
			syntaxRoot = taskResult.Result;

			if (!taskResult.IsSuccess || syntaxRoot == null || cancellationToken.IsCancellationRequested)
                return null;

            return new ParsedDocument(workspace, document, syntaxRoot, snapshot);
        }

        private static bool IsSupportedFileType(Document document) => allowedExtensions.Contains(Path.GetExtension(document.FilePath));       
    }
}
