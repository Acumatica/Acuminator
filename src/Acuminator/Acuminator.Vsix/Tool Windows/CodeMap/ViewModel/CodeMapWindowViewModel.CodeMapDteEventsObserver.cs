﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.CodeAnalysis;
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

			public CodeMapDteEventsObserver(CodeMapWindowViewModel codeMapViewModel)
			{
				_codeMapViewModel = codeMapViewModel;

				if (ThreadHelper.CheckAccess())
				{
					EnvDTE.DTE dte = AcuminatorVSPackage.Instance.GetService<EnvDTE.DTE>();

					//Store reference to DTE SolutionEvents and WindowEvents to prevent them from being GC-ed
					if (dte?.Events != null)
					{
						#pragma warning disable VSTHRD010			// ThreadHelper.CheckAccess() was called already
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

				if (documentKind == null || documentKind == emptyObjectKind || documentLanguage != LegacyLanguageNames.CSharp ||
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
					documentLanguage == LegacyLanguageNames.CSharp && !windowDocumentPath.IsNullOrWhiteSpace();
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
