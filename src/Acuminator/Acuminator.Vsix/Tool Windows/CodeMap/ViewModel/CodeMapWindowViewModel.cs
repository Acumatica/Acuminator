using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Threading;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private readonly EnvDTE.WindowEvents _windowEvents;
		private readonly EnvDTE80.WindowVisibilityEvents _visibilityEvents;

		private CancellationTokenSource _cancellationTokenSource;

		private DocumentModel _documentModel;

		public Workspace Workspace
		{
			get;
			private set;
		}

		public Document Document => _documentModel?.Document;

		private CodeMapDocChangesClassifier DocChangesClassifier { get; } = new CodeMapDocChangesClassifier();

		/// <summary>
		/// Internal visibility flag for code map control. Serves as a workaround to hacky VS SDK which displays "Visible" in all other visibility properties for a hidden window.
		/// </summary>
		private bool IsVisible
		{
			get;
			set;
		}

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

		public Command RefreshCodeMapCommand { get; }

		private CodeMapWindowViewModel(IWpfTextView wpfTextView, Document document)
		{
			_documentModel = new DocumentModel(wpfTextView, document);
			Tree = new TreeViewModel(this);

			RefreshCodeMapCommand = new Command(p => RefreshCodeMapAsync().Forget());

			Workspace = _documentModel.Document.Project.Solution.Workspace;
			Workspace.WorkspaceChanged += OnWorkspaceChanged;

			if (ThreadHelper.CheckAccess())
			{
				EnvDTE.DTE dte = AcuminatorVSPackage.Instance.GetService<EnvDTE.DTE>();
				_windowEvents = dte?.Events?.WindowEvents;                                  //Store reference to DTE WindowEvents to prevent it from being GCed

				if (_windowEvents != null)
				{
					_windowEvents.WindowActivated += WindowEvents_WindowActivated;
				}

				_visibilityEvents = (dte?.Events as EnvDTE80.Events2)?.WindowVisibilityEvents;

				if (_visibilityEvents != null)
				{
					_visibilityEvents.WindowShowing += VisibilityEvents_WindowShowing;
					_visibilityEvents.WindowHiding += VisibilityEvents_WindowHiding;
				}
			}		
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

			if (Workspace != null)
			{
				Workspace.WorkspaceChanged -= OnWorkspaceChanged;
			}

			if (_windowEvents != null)
			{
				_windowEvents.WindowActivated -= WindowEvents_WindowActivated;
			}

			if (_visibilityEvents != null)
			{
				_visibilityEvents.WindowHiding -= VisibilityEvents_WindowHiding;
				_visibilityEvents.WindowShowing -= VisibilityEvents_WindowShowing;
			}
		}

		private async Task RefreshCodeMapAsync()
		{
			if (IsCalculating)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			ClearCodeMap();
			var currentWorkspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();

			if (currentWorkspace == null)
				return;

			Workspace = currentWorkspace;
			IWpfTextView activeWpfTextView = await AcuminatorVSPackage.Instance.GetWpfTextViewAsync();
			Document activeDocument = activeWpfTextView?.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (activeDocument == null)
				return;

			_documentModel = new DocumentModel(activeWpfTextView, activeDocument);
			BuildCodeMapAsync().Forget();
		}

		private void VisibilityEvents_WindowHiding(EnvDTE.Window window)
		{
			IsVisible = false;
		}

		private void VisibilityEvents_WindowShowing(EnvDTE.Window window)
		{
			IsVisible = true;
		}

		private void WindowEvents_WindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus) =>
			WindowEventsWindowActivatedAsync(gotFocus, lostFocus)
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(WindowEvents_WindowActivated)}");

		private async Task WindowEventsWindowActivatedAsync(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus)
		{
			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken ?? AcuminatorVSPackage.Instance.DisposalToken);
			}

			if (Equals(gotFocus, lostFocus) || gotFocus.Document == null)
				return;
			else if (gotFocus.Document.Language != LegacyLanguageNames.CSharp)
			{
				ClearCodeMap();
				return;
			}
			else if (gotFocus.Document.FullName == lostFocus?.Document?.FullName ||
					(lostFocus?.Document == null && Document != null && gotFocus.Document.FullName == Document.FilePath))
			{
				return;
			}

			ClearCodeMap();
			var currentWorkspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();

			if (currentWorkspace == null)
				return;

			Workspace = currentWorkspace;
			IWpfTextView activeWpfTextView = await AcuminatorVSPackage.Instance.GetWpfTextViewByFilePathAsync(gotFocus.Document.FullName);
			Document activeDocument = activeWpfTextView?.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (activeDocument == null)
				return;

			_documentModel = new DocumentModel(activeWpfTextView, activeDocument);
			await BuildCodeMapAsync();
		}


		private async void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
		{
			if (e == null || !(sender is Workspace newWorkspace) || Document == null)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(CancellationToken ?? AcuminatorVSPackage.Instance.DisposalToken);
			}

			if (!IsVisible || e.IsActiveDocumentCleared(Document))
			{
				ClearCodeMap();
				return;
			}
			else if (e.IsActiveDocumentChanged(Document))
			{
				return;
			}
			else if (e.IsDocumentTextChanged(Document))
			{
				await HandleDocumentTextChangesAsync(newWorkspace, e).ConfigureAwait(false);
			}		
		}

		private async Task HandleDocumentTextChangesAsync(Workspace newWorkspace, WorkspaceChangeEventArgs e)
		{
			Workspace = newWorkspace;
			Document changedDocument = e.NewSolution.GetDocument(e.DocumentId);
			Document oldDocument = Document;
			SyntaxNode oldRoot = _documentModel?.Root;

			if (changedDocument == null || oldDocument == null || oldRoot == null)
			{
				ClearCodeMap();
				return;
			}

			var newRoot = await changedDocument.GetSyntaxRootAsync(CancellationToken ?? default).ConfigureAwait(false);
			CodeMapRefreshMode recalculateCodeMap = newRoot == null
				? CodeMapRefreshMode.Clear
				: CodeMapRefreshMode.Recalculate;

			if (newRoot != null && Tree != null)
			{
				recalculateCodeMap = await DocChangesClassifier.ShouldRefreshCodeMapAsync(oldDocument, newRoot, 
																						  changedDocument, CancellationToken ?? default);
			}

			if (recalculateCodeMap == CodeMapRefreshMode.NoRefresh)
				return;

			ClearCodeMap();

			if (recalculateCodeMap == CodeMapRefreshMode.Recalculate)
			{
				_documentModel = new DocumentModel(_documentModel.WpfTextView, changedDocument);
				BuildCodeMapAsync().Forget();
			}	
		}

		private void ClearCodeMap()
		{
			_cancellationTokenSource?.Cancel();
			Tree?.RootItems.Clear();
			Tree = null;
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

			foreach (GraphSemanticModelForCodeMap graph in _documentModel.GraphModels)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var graphNodeVM = GraphNodeViewModel.Create(graph, tree, isExpanded: true, expandChildren: false);

				if (graphNodeVM != null)
				{
					tree.RootItems.Add(graphNodeVM);
				}
			}

			cancellationToken.ThrowIfCancellationRequested();
			return tree;
		}
	}
}
