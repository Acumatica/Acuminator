using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Utilities;
using Acuminator.Utilities.Common;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal delegate void WindowActivatedDelegate(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus);
	internal delegate void WindowShowingOrHidingDelegate(EnvDTE.Window window);
	internal delegate void DocumentClosingDelegate(EnvDTE.Document document);


    /// <summary>
    /// A VS events adapter class required to support subscription on VS events for several VS versions.
    /// </summary>
	internal abstract partial class VSEventsAdapter : IDisposable
	{
		public bool SubscribedOnVsEventsSuccessfully { get; private set; }

		public event Action AfterSolutionClosing;
		public event DocumentClosingDelegate DocumentClosing;
		public event WindowActivatedDelegate WindowActivated;
		public event WindowShowingOrHidingDelegate WindowHiding;
		public event WindowShowingOrHidingDelegate WindowShowing;

		protected VSEventsAdapter()
		{
		}

		public static VSEventsAdapter CreateAndSubscribe()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			VSEventsAdapter adapter;

			if (SharedVsSettings.VSVersion.VS2022OrNewer)
				adapter = new VSEventsAdapterVS2022();
			else
				adapter = new VSEventsAdapterVS2019();

			try
			{
				if (!adapter.TryInitialize())
				{
					adapter.SubscribedOnVsEventsSuccessfully = false;
					return adapter;
				}

				adapter.SubscribedOnVsEventsSuccessfully = adapter.TrySubscribeAdapterOnVSEvents();
			}
			catch (Exception e) //Handling exceptions in VS events subscription
			{
				adapter.SubscribedOnVsEventsSuccessfully = false;
				AcuminatorVSPackage.Instance.AcuminatorLogger.LogException(e, logOnlyFromAcuminatorAssemblies: true, LogMode.Error);
			}

			return adapter;
		}

		protected abstract bool TryInitialize();

		protected abstract bool TrySubscribeAdapterOnVSEvents();

		public void UnsubscribeAdapterFromVSEvents()
		{
			if (SubscribedOnVsEventsSuccessfully && !TryUnsubscribeAdapterFromVSEvents())
			{
				string errorMsg = VSIXResource.CodeMap_FailedToUnsubscribeFromVsEvents_ErrorMessage + Environment.NewLine +
								  VSIXResource.CreateIssue_Message;
				AcuminatorVSPackage.Instance.AcuminatorLogger.LogMessage(errorMsg, LogMode.Error);
			}
		}

		public abstract bool TryUnsubscribeAdapterFromVSEvents();

		protected void RaiseAfterSolutionClosingEvent() => AfterSolutionClosing?.Invoke();

		protected void RaiseDocumentClosingEvent(EnvDTE.Document document) => DocumentClosing?.Invoke(document);

		protected void RaiseWindowActivatedEvent(EnvDTE.Window gotFocus, EnvDTE.Window lostFocus) => 
			WindowActivated?.Invoke(gotFocus, lostFocus);
		
		protected void RaiseWindowShowingEvent(EnvDTE.Window window) => WindowShowing?.Invoke(window);
		protected void RaiseWindowHidingEvent(EnvDTE.Window window) => WindowHiding?.Invoke(window);

		public void Dispose() => TryUnsubscribeAdapterFromVSEvents();
	}
}
