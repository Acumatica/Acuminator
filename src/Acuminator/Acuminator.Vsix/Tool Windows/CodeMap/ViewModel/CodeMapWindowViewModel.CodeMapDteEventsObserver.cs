using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;

using Acuminator.Utilities;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private class CodeMapDteEventsObserver
		{
			private readonly VSEventsAdapter _vsEventsAdapter;

			private readonly CodeMapWindowViewModel _codeMapViewModel;

			public bool SubscribedOnVsEventsSuccessfully => _vsEventsAdapter.SubscribedOnVsEventsSuccessfully;

			public CodeMapDteEventsObserver(CodeMapWindowViewModel codeMapViewModel)
			{
				ThreadHelper.ThrowIfNotOnUIThread();

				_codeMapViewModel = codeMapViewModel;
				_vsEventsAdapter = VSEventsAdapter.CreateAndSubscribe();

				SubscribeCodeMapOnVisualStudioEvents();
			}

			private void SubscribeCodeMapOnVisualStudioEvents()
			{
				if (!SubscribedOnVsEventsSuccessfully)
					return;

				_vsEventsAdapter.AfterSolutionClosing += SolutionEvents_AfterClosing;
				_vsEventsAdapter.DocumentClosing += DocumentEvents_DocumentClosing;

				_vsEventsAdapter.WindowActivated += WindowEvents_WindowActivated;
				
				_vsEventsAdapter.WindowShowing += VisibilityEvents_WindowShowing;
				_vsEventsAdapter.WindowHiding += VisibilityEvents_WindowHiding;
			}

			public void UnsubscribeEvents()
			{
				_vsEventsAdapter.AfterSolutionClosing -= SolutionEvents_AfterClosing;
				_vsEventsAdapter.DocumentClosing -= DocumentEvents_DocumentClosing;

				_vsEventsAdapter.WindowActivated -= WindowEvents_WindowActivated;

				_vsEventsAdapter.WindowShowing -= VisibilityEvents_WindowShowing;
				_vsEventsAdapter.WindowHiding -= VisibilityEvents_WindowHiding;

				_vsEventsAdapter.UnsubscribeAdapterFromVSEvents();
			}

			[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Already checked with CheckAccess()")]
			private void DocumentEvents_DocumentClosing(EnvDTE.Document document)
			{
				if (!ThreadHelper.CheckAccess() || document == null || _codeMapViewModel.Document == null)
					return;

				string documentPath, documentKind, documentLanguage;

				try
				{
					//Sometimes COM interop exceptions popup from getter, so we need to query properties safely
					documentPath = document.FullName;
					documentKind = document.Kind;
					documentLanguage = document.Language;
				}
				catch
				{
					return;
				}

				const string emptyObjectKind = "{00000000-0000-0000-0000-000000000000}";

				if (documentKind == null || documentKind == emptyObjectKind || documentLanguage != Constants.CSharp.LegacyLanguageName ||
					!string.Equals(documentPath, _codeMapViewModel.Document.FilePath, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				// Handle the case when active document is closed while Code Map tool window is opened and not pinned.
				// In this case VS is not firing WindowActivated event. Therefore we clear Code Map in DocumentClosing DTE event
				_codeMapViewModel.ClearCodeMap();
				_codeMapViewModel.DocumentModel = null;
			}

			private void VisibilityEvents_WindowHiding(EnvDTE.Window window) => SetVisibilityForCodeMapWindow(window, windowIsVisible: false);

			private void VisibilityEvents_WindowShowing(EnvDTE.Window window) => SetVisibilityForCodeMapWindow(window, windowIsVisible: true);

			[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Already checked with CheckAccess()")]
			private void SetVisibilityForCodeMapWindow(EnvDTE.Window window, bool windowIsVisible)
			{
				if (!ThreadHelper.CheckAccess())
					return;

				string windowObjectKind, windowKind, windowDocumentPath, documentLanguage;

				try
				{
					//Sometimes COM interop exceptions popup from getter, so we need to query properties safely
					windowObjectKind = window.ObjectKind;	
					windowDocumentPath = window.Document?.FullName;
					documentLanguage = window.Document?.Language;
					windowKind = window.Kind;
				}
				catch
				{
					return;
				}

				if (windowObjectKind == null || !Guid.TryParse(windowObjectKind, out Guid windowId))
					return;

				if (windowId == CodeMapWindow.CodeMapWindowGuid)  //Case when Code Map is displayed
				{
					bool wasVisible = _codeMapViewModel.IsVisible;
					_codeMapViewModel.IsVisible = windowIsVisible;

					if (!wasVisible && _codeMapViewModel.IsVisible)   //Handle the case when WindowShowing event happens after WindowActivated event
					{
						RefreshCodeMapAsync()
							.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(SetVisibilityForCodeMapWindow)}");
					}
				}
				else if (IsSwitchingToAnotherDocumentWhileCodeMapIsEmpty())
				{				
					RefreshCodeMapAsync()
						.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(SetVisibilityForCodeMapWindow)}");			
				}	

				//-------------------------------------------Local Function----------------------------------------------------------------------------------------
				bool IsSwitchingToAnotherDocumentWhileCodeMapIsEmpty() =>
					_codeMapViewModel.IsVisible && _codeMapViewModel.Document == null && windowIsVisible && windowKind == "Document" && windowId != default &&
					documentLanguage == Constants.CSharp.LegacyLanguageName && !windowDocumentPath.IsNullOrWhiteSpace();
			}

			/// <summary>
			/// Solution events after closing. Clear up the document data.
			/// </summary>
			private void SolutionEvents_AfterClosing()
			{
				_codeMapViewModel.ClearCodeMap();
				_codeMapViewModel.DocumentModel = null;
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

				if (!_codeMapViewModel.IsVisible || Equals(gotFocus, lostFocus) || gotFocus.Document == null)
					return;
				else if (gotFocus.Document.Language != Constants.CSharp.LegacyLanguageName)
				{
					_codeMapViewModel.ClearCodeMap();
					return;
				}
				else if (gotFocus.Document.FullName == lostFocus?.Document?.FullName ||
						(lostFocus?.Document == null && _codeMapViewModel.Document != null && gotFocus.Document.FullName == _codeMapViewModel.Document.FilePath))
				{
					return;
				}

				var activeWpfTextViewTask = AcuminatorVSPackage.Instance.GetWpfTextViewByFilePathAsync(gotFocus.Document.FullName);
				await _codeMapViewModel.RefreshCodeMapInternalAsync(activeWpfTextViewTask, activeDocument: null);
			}

			private async Task RefreshCodeMapAsync(IWpfTextView activeWpfTextView = null, Document activeDocument = null)
			{
				if (_codeMapViewModel.IsCalculating)
					return;

				if (!ThreadHelper.CheckAccess())
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				}

				var activeWpfTextViewTask = activeWpfTextView != null
					? Task.FromResult(activeWpfTextView)
					: AcuminatorVSPackage.Instance.GetWpfTextViewAsync();

				await _codeMapViewModel.RefreshCodeMapInternalAsync(activeWpfTextViewTask, activeDocument);
			}
		}
	}
}
