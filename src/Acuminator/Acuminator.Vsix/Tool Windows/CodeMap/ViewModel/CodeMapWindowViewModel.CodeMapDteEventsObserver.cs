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
using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;
using static Microsoft.VisualStudio.Shell.VsTaskLibraryHelper;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class CodeMapWindowViewModel : ToolWindowViewModelBase
	{
		private class CodeMapDteEventsObserver
		{
			private readonly EnvDTE.SolutionEvents _solutionEvents;
			private readonly EnvDTE.WindowEvents _windowEvents;
			private readonly EnvDTE.DocumentEvents _documentEvents;
			private readonly EnvDTE80.WindowVisibilityEvents _visibilityEvents;

			private readonly CodeMapWindowViewModel _codeMapViewModel;

			private bool IsActiveDocumentVisibilityChanging { get; set; }

			public CodeMapDteEventsObserver(CodeMapWindowViewModel codeMapViewModel)
			{
				_codeMapViewModel = codeMapViewModel;

				if (ThreadHelper.CheckAccess())
				{
					EnvDTE.DTE dte = AcuminatorVSPackage.Instance.GetService<EnvDTE.DTE>();

					//Store reference to DTE SolutionEvents and WindowEvents to prevent them from being GCed
					if (dte?.Events != null)
					{
#pragma warning disable VSTHRD010 // ThreadHelper.CheckAccess() is called
						_solutionEvents = dte.Events.SolutionEvents;
						_windowEvents = dte.Events.WindowEvents;
						_documentEvents = dte.Events.DocumentEvents;
						_visibilityEvents = (dte.Events as EnvDTE80.Events2)?.WindowVisibilityEvents;
#pragma warning restore VSTHRD010
					}

					SubscribeOnVisualStudioEvents();
				}
			}

			private void SubscribeOnVisualStudioEvents()
			{
				if (_solutionEvents != null)
				{
					_solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
				}

				if (_windowEvents != null)
				{
					_windowEvents.WindowActivated += WindowEvents_WindowActivated;
				}

				if (_documentEvents != null)
				{
					_documentEvents.DocumentClosing += DocumentEvents_DocumentClosing;
					_documentEvents.DocumentOpened += _documentEvents_DocumentOpened;
				}

				if (_visibilityEvents != null)
				{
					_visibilityEvents.WindowShowing += VisibilityEvents_WindowShowing;
					_visibilityEvents.WindowHiding += VisibilityEvents_WindowHiding;
				}
			}

			public void UnsubscribeEvents()
			{
				if (_solutionEvents != null)
				{
					_solutionEvents.AfterClosing -= SolutionEvents_AfterClosing;
				}

				if (_documentEvents != null)
				{
					_documentEvents.DocumentClosing -= DocumentEvents_DocumentClosing;
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

			private void _documentEvents_DocumentOpened(EnvDTE.Document Document)
			{

			}

			private void DocumentEvents_DocumentClosing(EnvDTE.Document document)
			{
				if (!ThreadHelper.CheckAccess() || document == null)
					return;


			}

			private void VisibilityEvents_WindowHiding(EnvDTE.Window window) => SetVisibilityForCodeMapWindow(window, windowIsVisible: false);

			private void VisibilityEvents_WindowShowing(EnvDTE.Window window) => SetVisibilityForCodeMapWindow(window, windowIsVisible: true);

			[SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "Already checked")]
			private void SetVisibilityForCodeMapWindow(EnvDTE.Window window, bool windowIsVisible)
			{
				if (!ThreadHelper.CheckAccess())
					return;

				var (windowId, windowDocumentPath) = GetWindowIdAndFilePathFromComWindow(window);
				if (windowId == null)
					return;

				if (windowId == CodeMapWindow.CodeMapWindowGuid)  //Case when Code Map is displayed
				{
					IsActiveDocumentVisibilityChanging = false;
					bool wasVisible = _codeMapViewModel.IsVisible;
					_codeMapViewModel.IsVisible = windowIsVisible;

					if (!wasVisible && _codeMapViewModel.IsVisible)   //Handle the case when WindowShowing event happens after WindowActivated event
					{
						RefreshCodeMapAsync()
							.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(SetVisibilityForCodeMapWindow)}");
					}

					return;
				}

				bool isClosingWindow = window.ObjectKind == "{00000000-0000-0000-0000-000000000000}";
				bool isDocumentWindow = window.Kind == "Document";
				bool isActiveDocumentClosed = !windowIsVisible && _codeMapViewModel.IsVisible && isDocumentWindow && isClosingWindow &&
											  !windowDocumentPath.IsNullOrWhiteSpace() && !IsActiveDocumentVisibilityChanging &&
											   string.Equals(_codeMapViewModel.Document?.FilePath, windowDocumentPath, StringComparison.OrdinalIgnoreCase);

				bool isNewDocumentShowing = windowIsVisible && _codeMapViewModel.IsVisible && _codeMapViewModel.DocumentModel == null &&
											!isClosingWindow && isDocumentWindow && IsActiveDocumentVisibilityChanging &&
											!windowDocumentPath.IsNullOrWhiteSpace();

				// Handle the case when active document is closed while Code Map tool window is opened and not pinned.
				// In this case due to VS not firing WindowActivated event we have to use simple state machine with IsActiveDocumentVisibilityChanging flag
				// to mark for the next document WindowShowing event that Code Map should be rebuild
				if (isActiveDocumentClosed)
				{
					IsActiveDocumentVisibilityChanging = true;

					try
					{
						_codeMapViewModel.ClearCodeMap();
						_codeMapViewModel.DocumentModel = null;
					}
					catch
					{
						IsActiveDocumentVisibilityChanging = false;
						throw;
					}
				}
				else
				{
					if (!isClosingWindow)
						IsActiveDocumentVisibilityChanging = false;

					if (isNewDocumentShowing)
					{
						RefreshCodeMapAsync()
							.FileAndForget($"vs/{AcuminatorVSPackage.PackageName}/{nameof(CodeMapWindowViewModel)}/{nameof(SetVisibilityForCodeMapWindow)}");
					}
				}
			}

			private (Guid? WindowID, string DocumentPath) GetWindowIdAndFilePathFromComWindow(EnvDTE.Window window)
			{
				string objectKind = null;
				string windowDocumentPath = null;

				try
				{
					//Sometimes COM interop exceptions popup from getter, so we need to obtain ObjectKind safely
					objectKind = window?.ObjectKind;

#pragma warning disable VSTHRD010
					windowDocumentPath = GetFilePathFromCOM(window);
#pragma warning restore VSTHRD010
				}
				catch (Exception)
				{
				}

				return objectKind != null && Guid.TryParse(objectKind, out Guid windowId)
					? (windowId, windowDocumentPath)
					: (default(Guid?), windowDocumentPath);
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
				else if (gotFocus.Document.Language != LegacyLanguageNames.CSharp)
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
