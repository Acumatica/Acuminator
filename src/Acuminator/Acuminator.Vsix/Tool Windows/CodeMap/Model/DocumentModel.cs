using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using System.ComponentModel;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DocumentModel
	{		
		public IWpfTextView WpfTextView { get; }

		public Document Document { get; }

		public SyntaxNode Root { get; private set; }

		public SemanticModel SemanticModel { get; private set; }

		public bool IsCodeFileDataLoaded => Root != null && SemanticModel != null;

		public DocumentModel(IWpfTextView wpfTextView, Document document)
		{
			wpfTextView.ThrowOnNull(nameof(wpfTextView));
			document.ThrowOnNull(nameof(document));

			WpfTextView = wpfTextView;
			Document = document;
		}

		public DocumentModel()
		{			
		}

		public async Task<bool> LoadCodeFileDataAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return false;

			try
			{		
				SemanticModel = await GetSemanticModelAsync(cancellationToken);
				Root = await GetRootAsync(cancellationToken);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private Task<SemanticModel> GetSemanticModelAsync(CancellationToken cancellationToken)
		{
			if (Document.TryGetSemanticModel(out var semanticModel))
				return Task.FromResult(semanticModel);

			return Document.GetSemanticModelAsync(cancellationToken);
		}

		private Task<SyntaxNode> GetRootAsync(CancellationToken cancellationToken)
		{
			if (Document.TryGetSyntaxRoot(out var root))
				return Task.FromResult(root);

			return Document.GetSyntaxRootAsync(cancellationToken);
		}
	}
}
