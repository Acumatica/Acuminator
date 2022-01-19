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
			public bool SubscribedOnVsEventsSuccessfully { get; } 

			private readonly EnvDTE.SolutionEvents _solutionEvents;
			private readonly EnvDTE.WindowEvents _windowEvents;
			private readonly EnvDTE.DocumentEvents _documentEvents;
			private readonly EnvDTE80.WindowVisibilityEvents _visibilityEvents;

			private readonly CodeMapWindowViewModel _codeMapViewModel;

			public CodeMapDteEventsObserver(CodeMapWindowViewModel codeMapViewModel)
			{
				ThreadHelper.ThrowIfNotOnUIThread();

				_codeMapViewModel = codeMapViewModel;
				bool subscribedOnAll = true;

				try
				{
					var dte2 = GetDTE2();

					//Store reference to DTE SolutionEvents and WindowEvents to prevent them from being GC-ed
					if (dte2?.Events != null)
					{
						_solutionEvents = dte2.Events.SolutionEvents;
						_windowEvents = dte2.Events.WindowEvents;
						_documentEvents = dte2.Events.DocumentEvents;
						_visibilityEvents = GetWindowVisibilityEvents(dte2);
					}
					else
					{
						subscribedOnAll = false;
					}

					if (subscribedOnAll)
						subscribedOnAll = TrySubscribeOnVisualStudioEvents();

					SubscribedOnVsEventsSuccessfully = subscribedOnAll;
				}
				catch (Exception e)	//Handling exceptions in VS events subscription
				{
					SubscribedOnVsEventsSuccessfully = false;
					AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: true, LogMode.Error);
				}
			}

			private EnvDTE80.DTE2 GetDTE2()
			{
				// Extra variables exist for the ease of debugging
				var dte = AcuminatorVSPackage.Instance.GetService<EnvDTE.DTE>();
				EnvDTE80.DTE2 dte2 = dte as EnvDTE80.DTE2;
				return dte2;
			}

			private EnvDTE80.WindowVisibilityEvents GetWindowVisibilityEvents(EnvDTE80.DTE2 dte2)
			{
				if (SharedVsSettings.VSVersion.VS2022OrNewer)
				{
					// In VS 2022 there are changes in VS events interfaces implemented by DTE.Events service. 
					// However, COM interop works fine via dynamic (its one of the well-known use cases of dynamic)
					dynamic events = dte2.Events;
					dynamic windowVisibilityEvents = events.WindowVisibilityEvents;
					return windowVisibilityEvents;
				}
				else
				{
					var events2 = dte2.Events as EnvDTE80.Events2;
					return events2?.WindowVisibilityEvents;
				}
			}

			private bool TrySubscribeOnVisualStudioEvents()
			{
				bool subscribedOnAll = true;

				if (_solutionEvents != null)
					_solutionEvents.AfterClosing += SolutionEvents_AfterClosing;
				else
					subscribedOnAll = false;

				if (_windowEvents != null)
					_windowEvents.WindowActivated += WindowEvents_WindowActivated;
				else
					subscribedOnAll = false;

				if (_documentEvents != null)
					_documentEvents.DocumentClosing += DocumentEvents_DocumentClosing;
				else
					subscribedOnAll = false;

				if (_visibilityEvents != null)
				{
					_visibilityEvents.WindowShowing += VisibilityEvents_WindowShowing;
					_visibilityEvents.WindowHiding += VisibilityEvents_WindowHiding;
				}
				else
					subscribedOnAll = false;

				return subscribedOnAll;
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
