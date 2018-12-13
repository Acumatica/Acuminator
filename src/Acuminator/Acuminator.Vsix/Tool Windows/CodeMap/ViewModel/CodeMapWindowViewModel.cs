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
using Acuminator.Vsix.Utilities;


using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private CancellationTokenSource _cancellationTokenSource;

		private DocumentModel _documentModel;
		private Workspace _workspace;

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

		public CancellationToken? CancellationToken => _cancellationTokenSource?.Token;

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

			_workspace = _documentModel.Document.Project.Solution.Workspace;
			_workspace.WorkspaceChanged += OnWorkspaceChanged;
		}

		private async void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
		{
			if (e.Kind != WorkspaceChangeKind.DocumentChanged || Document.Id != e.DocumentId ||
				Document.Project.Id != e.ProjectId || !(sender is Workspace newWorkspace))
			{
				return;
			}

			_cancellationTokenSource?.Cancel();
			Tree?.RootItems.Clear();
			Tree = null;
			_workspace = newWorkspace;
			Document changedDocument = e.NewSolution.GetDocument(e.DocumentId);

			if (changedDocument == null)
				return;
			
			var root = await changedDocument.GetSyntaxRootAsync().ConfigureAwait(false);

			if (root == null || root.ContainsDiagnostics)
				return;

			_documentModel = new DocumentModel(_documentModel.WpfTextView, changedDocument);
			BuildCodeMapAsync().Forget();
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

			if (_workspace != null)
			{
				_workspace.WorkspaceChanged -= OnWorkspaceChanged;
			}
		}

		private async Task BuildCodeMapAsync()
		{
			try
			{
				using (_cancellationTokenSource = new CancellationTokenSource())
				{
					CancellationToken cancellationToken = _cancellationTokenSource.Token;

					if (!ThreadHelper.CheckAccess())
					{
						await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
					}
				
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
