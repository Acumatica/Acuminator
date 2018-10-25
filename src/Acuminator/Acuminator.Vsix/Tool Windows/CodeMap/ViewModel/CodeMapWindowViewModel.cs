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

		public TreeViewModel Tree { get; }

		public CodeMapWindowViewModel()
		{
			_documentModel = new DocumentModel();
			Tree = new TreeViewModel(this);

			var root = new TreeNodeViewModel(Tree, "Root");
			root.Children.Add(new TreeNodeViewModel(Tree, "Child1"));
			root.Children.Add(new TreeNodeViewModel(Tree, "Child2"));
			root.Children.Last().Children.Add(new TreeNodeViewModel(Tree, "Descendant"));

			Tree.RootItems.Add(root);
		}

		public CodeMapWindowViewModel(IWpfTextView wpfTextView, Document document)
		{
			_documentModel = new DocumentModel(wpfTextView, document);
			Tree = new TreeViewModel(this);

			var root = new TreeNodeViewModel(Tree, "Root");
			root.Children.Add(new TreeNodeViewModel(Tree, "Child1"));
			root.Children.Add(new TreeNodeViewModel(Tree, "Child2"));
			root.Children.Last().Children.Add(new TreeNodeViewModel(Tree, "Descendant"));

			Tree.RootItems.Add(root);
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

		public async Task BuildCodeMapAsync()
		{
			try
			{
				using (_cancellationTokenSource = new CancellationTokenSource())
				{
					CancellationToken cancellationToken = _cancellationTokenSource.Token;

					await TaskScheduler.Default;
					await BuildCodeMapCoreAsync(cancellationToken).ConfigureAwait(false);
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

				}
			}
			finally
			{
				_cancellationTokenSource = null;
			}
		}

		private async Task BuildCodeMapCoreAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			await _documentModel.LoadCodeFileDataAsync(cancellationToken)
							    .ConfigureAwait(false);
		}
	}
}
