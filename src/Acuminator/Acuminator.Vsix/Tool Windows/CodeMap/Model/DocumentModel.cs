using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax.PXGraph;
using Acuminator.Vsix.Utilities;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DocumentModel
	{		
		public IWpfTextView WpfTextView
		{
			get;
			private set;
		}

		public Document Document
		{
			get;
			private set;
		}

		public SyntaxNode Root
		{
			get;
			private set;
		}

		public SemanticModel SemanticModel { get; private set; }

		private readonly List<PXGraphEventSemanticModel> _graphModels = new List<PXGraphEventSemanticModel>(capacity: 2);

		public ReadOnlyCollection<PXGraphEventSemanticModel> GraphModels { get; }

		public bool IsCodeFileDataLoaded => Root != null && SemanticModel != null;

		public DocumentModel(IWpfTextView wpfTextView, Document document)
		{
			wpfTextView.ThrowOnNull(nameof(wpfTextView));
			document.ThrowOnNull(nameof(document));
			
			WpfTextView = wpfTextView;
			Document = document;
			GraphModels = _graphModels.AsReadOnly();
		}

		public void ChangeWpfTextView(IWpfTextView newWpfTextView, Document newDocument)
		{
			newWpfTextView.ThrowOnNull(nameof(newWpfTextView));
			newDocument.ThrowOnNull(nameof(newDocument));

			string newWpfTextViewFilePath = newWpfTextView.TextBuffer?.GetFilePath();

			if (newWpfTextViewFilePath != newDocument.FilePath)
			{
				throw new ArgumentException("The file paths for WPF text view and Roslyn Document are not equal");
			}

			WpfTextView = newWpfTextView;
			Document = newDocument;
		}

		public async Task<bool> LoadCodeFileDataAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return false;

			try
			{
				_graphModels.Clear();
				SemanticModel = await GetSemanticModelAsync(cancellationToken);
				Root = await GetRootAsync(cancellationToken);

				if (!(Root is CompilationUnitSyntax compilationUnit))
					return false;

				PXContext context = new PXContext(SemanticModel.Compilation);
				var graphs = compilationUnit.GetDeclaredGraphsAndExtensions(SemanticModel, context, cancellationToken)
											.Select(graphInfo => graphInfo.GraphSymbol)
											.OfType<INamedTypeSymbol>()
											.ToList();
				if (graphs.Count == 0)
					return false;

				var graphSemanticModels = graphs.Select(graph => PXGraphEventSemanticModel.InferModels(context, graph, cancellationToken)
																						  .FirstOrDefault())
												.Where(graphModel => graphModel != null && graphModel.Type != GraphType.None)
												.ToList();

				if (graphSemanticModels.Count == 0)
					return false;

				_graphModels.AddRange(graphSemanticModels);
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
