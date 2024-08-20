#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal abstract partial class VSEventsAdapter
	{
		/// <summary>
		/// A VS events adapter class required to support subscription on VS events for VS 2019.
		/// </summary>
		private class VSEventsAdapterVS2019 : VSEventsAdapter
		{
			private EnvDTE.SolutionEvents? _solutionEvents;
			private EnvDTE.WindowEvents? _windowEvents;
			private EnvDTE.DocumentEvents? _documentEvents;
			private EnvDTE80.WindowVisibilityEvents? _visibilityEvents;

			[MemberNotNullWhen(returnValue: true, 
							   nameof(_solutionEvents), nameof(_windowEvents), nameof(_documentEvents), nameof(_visibilityEvents))]
			protected override bool TryInitialize()
			{
				ThreadHelper.ThrowIfNotOnUIThread();
				var dte = AcuminatorVSPackage.Instance.GetService<EnvDTE.DTE>();

				//Store reference to DTE SolutionEvents and WindowEvents to prevent them from being GC-ed
				if (dte?.Events != null)
				{
					_solutionEvents = dte.Events.SolutionEvents;
					_windowEvents = dte.Events.WindowEvents;
					_documentEvents = dte.Events.DocumentEvents;
					_visibilityEvents = (dte.Events as EnvDTE80.Events2)?.WindowVisibilityEvents;
				}
				else
					return false;

				return _solutionEvents != null && _windowEvents != null && _documentEvents != null && _visibilityEvents != null;
			}

			protected override bool TrySubscribeAdapterOnVSEvents()
			{
				if (_solutionEvents != null)
					_solutionEvents.AfterClosing += RaiseAfterSolutionClosingEvent;

				if (_documentEvents != null)
					_documentEvents.DocumentClosing += RaiseDocumentClosingEvent;

				if (_windowEvents != null)
					_windowEvents.WindowActivated += RaiseWindowActivatedEvent;

				if (_visibilityEvents != null)
				{
					_visibilityEvents.WindowShowing += RaiseWindowShowingEvent;
					_visibilityEvents.WindowHiding += RaiseWindowHidingEvent;
				}

				return true;
			}

			public override bool TryUnsubscribeAdapterFromVSEvents()
			{
				if (_solutionEvents != null)
					_solutionEvents.AfterClosing -= RaiseAfterSolutionClosingEvent;

				if (_documentEvents != null)
					_documentEvents.DocumentClosing -= RaiseDocumentClosingEvent;

				if (_windowEvents != null)
					_windowEvents.WindowActivated -= RaiseWindowActivatedEvent;

				if (_visibilityEvents != null)
				{
					_visibilityEvents.WindowShowing -= RaiseWindowShowingEvent;
					_visibilityEvents.WindowHiding -= RaiseWindowHidingEvent;
				}

				return true;
			}	
		}
	}
}
