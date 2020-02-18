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
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private readonly EnvDTE.SolutionEvents _solutionEvents;
		private readonly EnvDTE.WindowEvents _windowEvents;
		private readonly EnvDTE80.WindowVisibilityEvents _visibilityEvents;

		private CancellationTokenSource _cancellationTokenSource;

		public TreeBuilderBase TreeBuilder
		{
			get;
			internal set;
		}

		public TreeNodesSorter TreeSorter { get; } = new TreeNodesSorter();

		public IRootCandidateSymbolsRetriever RootSymbolsRetriever
		{
			get;
			internal set;
		}

		public ISemanticModelFactory SemanticModelFactory
		{
			get;
			internal set;
		}

		public DocumentModel DocumentModel
		{
			get;
			private set;
		}

		public Workspace Workspace
		{
			get;
			private set;
		}

		public Document Document => DocumentModel?.Document;

		private CodeMapDocChangesClassifier DocChangesClassifier { get; }

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

		public Command ExpandOrCollapseAllCommand { get; }

		public Command SortNodeChildrenByNameCommand { get; }

		public Command SortNodeChildrenByDeclarationOrderCommand { get; }

		public Command SortNodeDescendantsByNameCommand { get; }

		public Command SortNodeDescendantsByDeclarationOrderCommand { get; }

		private CodeMapWindowViewModel(IWpfTextView wpfTextView, Document document)
		{
			DocumentModel = new DocumentModel(wpfTextView, document);

			RootSymbolsRetriever = new RootCandidateSymbolsRetrieverDefault();
			SemanticModelFactory = new SemanticModelFactoryDefault();
			TreeBuilder = new DefaultCodeMapTreeBuilder();
			DocChangesClassifier = new CodeMapDocChangesClassifier(this);
			IsVisible = true;
			Tree = TreeBuilder.CreateEmptyCodeMapTree(this);
			
			RefreshCodeMapCommand = new Command(p => RefreshCodeMapAsync().Forget());
			ExpandOrCollapseAllCommand = new Command(p => ExpandOrCollapseNodeDescendants(p as TreeNodeViewModel));

			SortNodeChildrenByNameCommand = new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Alphabet, sortDescendants: false));
			SortNodeChildrenByDeclarationOrderCommand = new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Declaration, sortDescendants: false));
			SortNodeDescendantsByNameCommand = new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Alphabet, sortDescendants: true));
			SortNodeDescendantsByDeclarationOrderCommand = new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Declaration, sortDescendants: true));

			Workspace = DocumentModel.Document.Project.Solution.Workspace;
			Workspace.WorkspaceChanged += OnWorkspaceChanged;

			if (ThreadHelper.CheckAccess())
			{
				EnvDTE.DTE dte = AcuminatorVSPackage.Instance.GetService<EnvDTE.DTE>();

				//Store reference to DTE SolutionEvents and WindowEvents to prevent them from being GCed
				_solutionEvents = dte?.Events?.SolutionEvents;
				_windowEvents = dte?.Events?.WindowEvents;

				if (_solutionEvents != null)
				{
					_solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
				}

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

		public override void FreeResources()
		{
			base.FreeResources();

			_cancellationTokenSource?.Dispose();

			if (Workspace != null)
			{
				Workspace.WorkspaceChanged -= OnWorkspaceChanged;
			}

			if (_solutionEvents != null)
			{
				_solutionEvents.AfterClosing -= SolutionEvents_AfterClosing;
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

		internal async Task RefreshCodeMapOnWindowOpeningAsync(IWpfTextView activeWpfTextView = null, Document activeDocument = null)
		{
			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			IsCalculating = false;
			await RefreshCodeMapAsync(activeWpfTextView, activeDocument);
		}

		private void VisibilityEvents_WindowHiding(EnvDTE.Window window) => SetVisibilityForCodeMapWindow(window, isVisible: false);

		private void VisibilityEvents_WindowShowing(EnvDTE.Window window) => SetVisibilityForCodeMapWindow(window, isVisible: true);

		private void SetVisibilityForCodeMapWindow(EnvDTE.Window window, bool isVisible)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (Guid.TryParse(window?.ObjectKind, out Guid windowId) && windowId == CodeMapWindow.CodeMapWindowGuid)
			{
				IsVisible = isVisible;
			}
		}

		/// <summary>
		/// Solution events after closing. Clear up the document data.
		/// </summary>
		private void SolutionEvents_AfterClosing()
		{
			ClearCodeMap();
			DocumentModel = null;
			NotifyPropertyChanged(nameof(Document));
		}

		private void WindowEvents_WindowActivated(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus) =>
			WindowEventsWindowActivatedAsync(gotFocus, lostFocus)
				.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(WindowEvents_WindowActivated)}");

		private async Task WindowEventsWindowActivatedAsync(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus)
		{
			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			if (!IsVisible || Equals(gotFocus, lostFocus) || gotFocus.Document == null)
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

			DocumentModel = new DocumentModel(activeWpfTextView, activeDocument);
			await BuildCodeMapAsync();
		}

		private async Task RefreshCodeMapAsync(IWpfTextView activeWpfTextView = null, Document activeDocument = null)
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
			activeWpfTextView = activeWpfTextView ?? await AcuminatorVSPackage.Instance.GetWpfTextViewAsync();
			activeDocument = activeDocument ?? activeWpfTextView?.TextSnapshot.GetOpenDocumentInCurrentContextWithChanges();

			if (activeDocument == null)
				return;

			DocumentModel = new DocumentModel(activeWpfTextView, activeDocument);
			await BuildCodeMapAsync();
		}

		private async void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
		{
			if (e == null || !(sender is Workspace newWorkspace) || Document == null)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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
			SyntaxNode oldRoot = DocumentModel?.Root;

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
				DocumentModel = new DocumentModel(DocumentModel.WpfTextView, changedDocument);
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
						await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
					}
				
					IsCalculating = true;
					await TaskScheduler.Default;

					bool isSuccess = await DocumentModel.LoadCodeFileDataAsync(RootSymbolsRetriever, SemanticModelFactory, cancellationToken)
														.ConfigureAwait(false);

					if (!isSuccess || cancellationToken.IsCancellationRequested || !DocumentModel.IsCodeFileDataLoaded)
						return;

					TreeViewModel newTreeVM = TreeBuilder?.BuildCodeMapTree(this, expandRoots: true, expandChildren: false, cancellationToken);

					if (newTreeVM == null)
						return;

					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					Tree = newTreeVM;			
				}
			}
			catch (OperationCanceledException)
			{

			}
			finally
			{
				IsCalculating = false;
				_cancellationTokenSource = null;
			}
		}

		private void ExpandOrCollapseNodeDescendants(TreeNodeViewModel node)
		{
			if (node != null)
			{
				node.ExpandOrCollapseAll(expand: !node.IsExpanded);
			}
		}

		public void SortNodes(TreeNodeViewModel node, SortType sortType, bool sortDescendants)
		{
			if (node == null)
				return;

			SortDirection sortDirection;

			if (node.ChildrenSortType == sortType)
			{
				sortDirection = node.ChildrenSortDirection == SortDirection.Ascending
					? SortDirection.Descending
					: SortDirection.Ascending;
			}
			else
			{
				sortDirection = SortDirection.Ascending;
			}

			node.AcceptSorter(TreeSorter, sortType, sortDirection, sortDescendants);

			if (sortDescendants)
				node.ExpandOrCollapseAll(expand: true);
			else
				node.IsExpanded = true;
		}
	}
}
