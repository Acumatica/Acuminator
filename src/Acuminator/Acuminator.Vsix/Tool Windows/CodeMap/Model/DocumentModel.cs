#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text.Editor;

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

		public SyntaxNode? Root
		{
			get;
			private set;
		}

		public SemanticModel? SemanticModel { get; private set; }

		private readonly List<ISemanticModel> _codeMapSemanticModels = new List<ISemanticModel>(capacity: 4);

		public ReadOnlyCollection<ISemanticModel> CodeMapSemanticModels { get; }

		public bool IsCodeFileDataLoaded => Root != null && SemanticModel != null;

		public DocumentModel(IWpfTextView wpfTextView, Document document)
		{
			wpfTextView.ThrowOnNull(nameof(wpfTextView));
			document.ThrowOnNull(nameof(document));
			
			WpfTextView = wpfTextView;
			Document = document;
			CodeMapSemanticModels = _codeMapSemanticModels.AsReadOnly();
		}

		public async Task<bool> LoadCodeFileDataAsync(IRootCandidateSymbolsRetriever rootCandidatesRetriever, ISemanticModelFactory semanticModelFactory,
													  CancellationToken cancellationToken)
		{
			if (semanticModelFactory == null || rootCandidatesRetriever == null || cancellationToken.IsCancellationRequested)
				return false;

			try
			{
				_codeMapSemanticModels.Clear();
				SemanticModel = await GetSemanticModelAsync(cancellationToken);
				Root = await GetRootAsync(cancellationToken);

				if (!(Root is CompilationUnitSyntax compilationUnit))
					return false;

				PXContext context = new PXContext(SemanticModel.Compilation, codeAnalysisSettings: null);

				if (!context.IsPlatformReferenced)
					return false;

				var candidateSymbols = rootCandidatesRetriever.GetCodeMapRootCandidates(compilationUnit, context, SemanticModel, cancellationToken);

				foreach (var (candidateSymbol, candidateNode) in candidateSymbols)
				{
					if (semanticModelFactory.TryToInferSemanticModel(candidateSymbol, candidateNode, context, out ISemanticModel codeMapSemanticModel, cancellationToken) &&
						codeMapSemanticModel != null)
					{
						_codeMapSemanticModels.Add(codeMapSemanticModel);
					}
				}

				return _codeMapSemanticModels.Count > 0;
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
