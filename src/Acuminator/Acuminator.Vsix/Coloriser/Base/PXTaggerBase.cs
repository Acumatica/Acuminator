#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using Shell = Microsoft.VisualStudio.Shell;

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

		protected PXTaggerProviderBase ProviderBase { get; }

		/// <summary>
		/// The type of the tagger.
		/// </summary>      
		public abstract TaggerType TaggerType { get; }

		protected bool CacheCheckingEnabled { get; }

		protected PXTaggerBase(ITextBuffer buffer, PXTaggerProviderBase provider, bool subscribeToSettingsChanges, bool useCacheChecking)
		{
			Buffer = buffer.CheckIfNull();
			ProviderBase = provider.CheckIfNull();
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

		protected virtual void ColoringSettingChangedHandler(object sender, SettingChangedEventArgs e)
		{
			ColoringSettingsChanged = true;
			LastTaggingWasSuccessful = false;
			Shell.ThreadHelper.JoinableTaskFactory.Run(RaiseTagsChangedAsync);
		}

		internal async virtual Task RaiseTagsChangedAsync()
		{
			if (!Shell.ThreadHelper.CheckAccess())
			{
				await Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
			}

			TagsChanged?.Invoke(this,
			  new SnapshotSpanEventArgs(
				  new SnapshotSpan(Buffer.CurrentSnapshot,
					  new Span(0, Buffer.CurrentSnapshot.Length))));
		}

		protected internal virtual void ResetCacheAndFlags(ITextSnapshot newSnapshotToCache)
		{
			ColoringSettingsChanged = false;
			LastTaggingWasSuccessful = false;
			Snapshot = newSnapshotToCache;
		}

		protected virtual bool CheckIfRetaggingIsNotNecessary(ITextSnapshot snapshot) =>
			CacheCheckingEnabled && Snapshot != null && Snapshot == snapshot && !ColoringSettingsChanged && LastTaggingWasSuccessful;

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
