﻿using System;
using System.Collections.Generic;
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
	public partial class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private readonly CodeMapDteEventsObserver _dteEventsObserver;
		private CancellationTokenSource _cancellationTokenSource;

		public TreeBuilderBase TreeBuilder
		{
			get;
			internal set;
		}

		private CodeMapSubtreeSorter TreeSorter { get; } = new CodeMapSubtreeSorter();

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

		private DocumentModel _documentModel;

		public DocumentModel DocumentModel
		{
			get => _documentModel;
			private set
			{
				if (!ReferenceEquals(_documentModel, value))
				{
					_documentModel = value;
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(Document));
				}
			}
		}

		public Document Document => DocumentModel?.Document;

		public Workspace Workspace
		{
			get;
			private set;
		}

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

		#region Sort Node Children Commands
		public Command SortNodeChildrenByNameAscendingCommand { get; }

		public Command SortNodeChildrenByNameDescendingCommand { get; }

		public Command SortNodeChildrenByDeclarationOrderAscendingCommand { get; }

		public Command SortNodeChildrenByDeclarationOrderDescendingCommand { get; }
		#endregion

		#region Sort Node Descendants Commands
		public Command SortNodeDescendantsByNameAscendingCommand { get; }

		public Command SortNodeDescendantsByNameDescendingCommand { get; }

		public Command SortNodeDescendantsByDeclarationOrderAscendingCommand { get; }

		public Command SortNodeDescendantsByDeclarationOrderDescendingCommand { get; }
		#endregion

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

			SortNodeChildrenByNameAscendingCommand = 
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Alphabet, SortDirection.Ascending, sortDescendants: false));
			SortNodeChildrenByNameDescendingCommand =
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Alphabet, SortDirection.Descending, sortDescendants: false));
			SortNodeChildrenByDeclarationOrderAscendingCommand = 
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Declaration, SortDirection.Ascending, sortDescendants: false));
			SortNodeChildrenByDeclarationOrderDescendingCommand =
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Declaration, SortDirection.Descending, sortDescendants: false));

			SortNodeDescendantsByNameAscendingCommand = 
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Alphabet, SortDirection.Ascending, sortDescendants: true));
			SortNodeDescendantsByNameDescendingCommand =
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Alphabet, SortDirection.Descending, sortDescendants: true));
			SortNodeDescendantsByDeclarationOrderAscendingCommand = 
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Declaration, SortDirection.Ascending, sortDescendants: true));
			SortNodeDescendantsByDeclarationOrderDescendingCommand =
				new Command(p => SortNodes(p as TreeNodeViewModel, SortType.Declaration, SortDirection.Descending, sortDescendants: true));

			Workspace = DocumentModel.Document.Project.Solution.Workspace;
			Workspace.WorkspaceChanged += OnWorkspaceChanged;

			_dteEventsObserver = new CodeMapDteEventsObserver(this);
		}

		public static CodeMapWindowViewModel InitCodeMap(IWpfTextView wpfTextView, Document document)
		{
			if (wpfTextView == null || document == null)
				return null;

			var codeMapViewModel = new CodeMapWindowViewModel(wpfTextView, document);
			codeMapViewModel.BuildCodeMapAsync().Forget();
			return codeMapViewModel;
		}

		public void SortNodes(TreeNodeViewModel node, SortType sortType, SortDirection sortDirection, bool sortDescendants)
		{
			if (node == null)
				return;

			if (sortDescendants)
			{
				TreeSorter.SortSubtree(node, sortType, sortDirection);
				node.ExpandOrCollapseAll(expand: true);
			}
			else
			{
				TreeSorter.SortChildren(node, sortType, sortDirection);
				node.IsExpanded = true;
			}
		}

		public override void FreeResources()
		{
			base.FreeResources();
			_cancellationTokenSource?.Dispose();

			if (Workspace != null)
			{
				Workspace.WorkspaceChanged -= OnWorkspaceChanged;
			}

			_dteEventsObserver.UnsubscribeEvents();
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

		private async Task RefreshCodeMapAsync(IWpfTextView activeWpfTextView = null, Document activeDocument = null)
		{
			if (IsCalculating)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			var activeWpfTextViewTask = activeWpfTextView != null 
				? Task.FromResult(activeWpfTextView)
				: AcuminatorVSPackage.Instance.GetWpfTextViewAsync();

			await RefreshCodeMapInternalAsync(activeWpfTextViewTask, activeDocument);
		}

		private async Task RefreshCodeMapInternalAsync(Task<IWpfTextView> activeWpfTextViewTask, Document activeDocument = null)
		{
			ClearCodeMap();
			var currentWorkspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();

			if (currentWorkspace == null)
				return;

			Workspace = currentWorkspace;
			IWpfTextView activeWpfTextView = activeWpfTextViewTask != null
				? await activeWpfTextViewTask
				: null;
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
			Tree?.Clear();
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
	}
}
