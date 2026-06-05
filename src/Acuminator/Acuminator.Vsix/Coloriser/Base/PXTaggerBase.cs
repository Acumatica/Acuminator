#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using Microsoft.VisualStudio.Text;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Coloriser
{
	public abstract class PXTaggerBase
	{
		private readonly TextDocumentDisposedNotification _disposedNotification;

#pragma warning disable CS0067
		public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;
#pragma warning restore CS0067

		protected ITextBuffer Buffer { get; }

		protected internal ITextSnapshot? Snapshot { get; private set; }

		protected bool ColoringSettingsChanged { get; private set; }

		protected bool SubscribedToSettingsChanges { get; private set; }

		public abstract bool HasReferenceToAcumaticaPlatform { get; }

		protected bool CacheCheckingEnabled { get; }

		protected PXTaggerBase(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, 
							   bool subscribeToSettingsChanges, bool useCacheChecking)
		{
			Buffer = buffer.CheckIfNull();
			SubscribedToSettingsChanges = subscribeToSettingsChanges;
			CacheCheckingEnabled = useCacheChecking;

			if (SubscribedToSettingsChanges)
			{
				var genOptionsPage = AcuminatorVSPackage.Instance?.GeneralOptionsPage;

				if (genOptionsPage != null)
				{
					genOptionsPage.ColoringSettingChanged += ColoringSettingChangedHandler;
				}
			}

			_disposedNotification = new TextDocumentDisposedNotification(textDocumentFactory, Buffer);
			_disposedNotification.CurrentTextDocumentDisposed += CleanupOnTextDocumentDisposed;
		}

		protected virtual void ColoringSettingChangedHandler(object sender, SettingChangedEventArgs e)
		{
			ColoringSettingsChanged = true;

			// Coloring setting should be called from the UI thread and there is a safety check in RaiseTagsChanged
			// It should be OK to make a sync call
			RaiseTagsChanged();
		}

		internal async Task RaiseTagsChangedAsync()
		{
			if (!ThreadHelper.CheckAccess())
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			RaiseTagsChangedImpl();
		}

		internal void RaiseTagsChanged()
		{
			if (!ThreadHelper.CheckAccess())
			{
				Debug.Fail("RaiseTagsChanged should be called from the UI thread. " + 
							"Call RaiseTagsChangedAsync if you need to raise tags changed from a background thread.");
				return;
			}

			RaiseTagsChangedImpl();
		}

		private void RaiseTagsChangedImpl()
		{
			TagsChanged?.Invoke(this,
			  new SnapshotSpanEventArgs(
				  new SnapshotSpan(Buffer.CurrentSnapshot,
					  new Span(0, Buffer.CurrentSnapshot.Length))));
		}

		protected internal virtual void ResetCacheAndFlags(ITextSnapshot? newSnapshotToCache)
		{
			ColoringSettingsChanged = false;
			Snapshot = newSnapshotToCache;
		}

		protected virtual void CleanupOnTextDocumentDisposed(object sender, EventArgs e)
		{
			Type taggerType = GetType();
			Buffer.Properties.RemoveProperty(taggerType);
			_disposedNotification.CurrentTextDocumentDisposed -= CleanupOnTextDocumentDisposed;

			if (!SubscribedToSettingsChanges)
				return;

			var genOptionsPage = AcuminatorVSPackage.Instance?.GeneralOptionsPage;

			if (genOptionsPage != null)
			{
				genOptionsPage.ColoringSettingChanged -= ColoringSettingChangedHandler;
			}
		}
	}
}
