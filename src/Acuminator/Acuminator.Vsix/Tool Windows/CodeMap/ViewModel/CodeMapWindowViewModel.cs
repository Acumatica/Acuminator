using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Threading;
using Acuminator.Utilities.Common;


using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private CancellationTokenSource _cancellationTokenSource;

		private DocumentModel _documentModel;

		public Document Document => _documentModel.Document;

		private TreeViewModel _tree;

		public TreeViewModel Tree
		{
			get => _tree;
			private set
			{
				if (!ReferenceEquals(_tree, value))
				{
					_tree = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool _isCalculating;

		public bool  IsCalculating
		{
			get => _isCalculating;
			private set
			{
				if (_isCalculating != value)
				{
					_isCalculating = value;
					NotifyPropertyChanged();
				}
			}
		}

		private CodeMapWindowViewModel(IWpfTextView wpfTextView, Document document)
		{
			_documentModel = new DocumentModel(wpfTextView, document);
			Tree = new TreeViewModel(this);
		}

		public static CodeMapWindowViewModel InitCodeMap(IWpfTextView wpfTextView, Document document)
		{
			if (wpfTextView == null || document == null)
				return null;

			var codeMapViewModel = new CodeMapWindowViewModel(wpfTextView, document);
			codeMapViewModel.BuildCodeMapAsync().Forget();
			return codeMapViewModel;
		}

		public void CancelCodeMapBuilding()
		{
			_cancellationTokenSource?.Cancel();
		}

		public override void FreeResources()
		{
			base.FreeResources();
			_cancellationTokenSource?.Dispose();
		}

		private async Task BuildCodeMapAsync()
		{
			try
			{
				using (_cancellationTokenSource = new CancellationTokenSource())
				{
					CancellationToken cancellationToken = _cancellationTokenSource.Token;
					IsCalculating = true;

					await TaskScheduler.Default;
					await _documentModel.LoadCodeFileDataAsync(cancellationToken)
										.ConfigureAwait(false);

					TreeViewModel newTreeVM = BuildCodeMapTreeView(cancellationToken);

					if (newTreeVM == null)
						return;

					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

					Tree = newTreeVM;
					IsCalculating = false;
				}
			}
			catch (OperationCanceledException e)
			{

			}
			finally
			{
				_cancellationTokenSource = null;
			}
		}

		private TreeViewModel BuildCodeMapTreeView(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested || !_documentModel.IsCodeFileDataLoaded)
				return null;

			TreeViewModel tree = new TreeViewModel(this);
			var rootItems = _documentModel.GraphModels.Select(graph => GraphNodeViewModel.Create(graph, tree))
													  .Where(graphVM => graphVM != null);
			tree.RootItems.AddRange(rootItems);
			cancellationToken.ThrowIfCancellationRequested();
			return tree;
		}
	}
}
