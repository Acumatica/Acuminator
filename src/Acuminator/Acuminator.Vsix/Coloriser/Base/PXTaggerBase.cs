#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using Microsoft.VisualStudio.Text;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Coloriser
{
	public abstract class PXTaggerBase : IDisposable
	{
#pragma warning disable CS0067
		public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;
#pragma warning restore CS0067

		protected ITextBuffer Buffer { get; }

		protected internal ITextSnapshot? Snapshot { get; private set; }

		protected bool ColoringSettingsChanged { get; private set; }

		protected bool SubscribedToSettingsChanges { get; private set; }

		internal abstract bool LastTaggingWasSuccessful { get; set; }

		public abstract bool HasReferenceToAcumaticaPlatform { get; }

		protected bool CacheCheckingEnabled { get; }

		protected PXTaggerBase(ITextBuffer buffer, bool subscribeToSettingsChanges, bool useCacheChecking)
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
		}

		private void ColoringSettingChangedHandler(object sender, SettingChangedEventArgs e)
		{
			ColoringSettingsChanged = true;
			LastTaggingWasSuccessful = false;

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
				return;

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
			LastTaggingWasSuccessful = false;
			Snapshot = newSnapshotToCache;
		}

		public virtual void Dispose()
		{
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
