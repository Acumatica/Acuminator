#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;
using Acuminator.Vsix.Utilities;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Acuminator.Vsix.Coloriser
{
	public abstract class PXTaggerBase
	{
		private const int MaxOptionsRetryCount = 10;

		private int _optionsRetryCount = 0;

		private readonly TextDocumentDisposedNotification _disposedNotification;

#pragma warning disable CS0067
		public event EventHandler<SnapshotSpanEventArgs>? TagsChanged;
#pragma warning restore CS0067

		protected bool Disposed { get; private set; }

		protected ITextBuffer Buffer { get; }

		protected internal ITextSnapshot? Snapshot { get; private set; }

		protected bool ColoringSettingsChanged { get; private set; }

		protected bool SubscribedToSettingsChanges { get; private set; }

		public abstract bool HasReferenceToAcumaticaPlatform { get; }

		protected bool CacheCheckingEnabled { get; }

		protected GeneralOptionsPage? GeneralOptionsPage { get; private set; }

		protected PXTaggerBase(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, 
							   bool subscribeToSettingsChanges, bool useCacheChecking)
		{
			Buffer = buffer.CheckIfNull();
			SubscribedToSettingsChanges = subscribeToSettingsChanges;
			CacheCheckingEnabled = useCacheChecking;
			GeneralOptionsPage = AcuminatorVSPackage.Instance?.TryGetGeneralOptionsPage();

			if (GeneralOptionsPage == null)
				StartAttemptToInitializeOptionsPageAsync(raiseTagsChanged: true);
			else
				SubscribeOnColoringSettingChangedEvent();

			_disposedNotification = new TextDocumentDisposedNotification(textDocumentFactory, Buffer);
			_disposedNotification.CurrentTextDocumentDisposed += CleanupOnTextDocumentDisposed;
		}

		protected void StartAttemptToInitializeOptionsPageAsync(bool raiseTagsChanged)
		{
			var initializeOptionsAsync = () => InitializeOptionsPageAsync(raiseTagsChanged);
			initializeOptionsAsync
				.FileAndForgetAcuminatorTask($"vs/{AcuminatorVSPackage.PackageName}/{GetType().Name}/{nameof(InitializeOptionsPageAsync)}");
		}

		private async Task InitializeOptionsPageAsync(bool raiseTagsChanged)
		{
			if (GeneralOptionsPage != null || AcuminatorVSPackage.Instance == null || Disposed)
				return;

			// A cheap extra check to always do without using retry attempts
			GeneralOptionsPage = AcuminatorVSPackage.Instance.TryGetGeneralOptionsPage();

			if (GeneralOptionsPage != null)
				return;

			int currentRetryCount = Interlocked.Increment(ref _optionsRetryCount);

			if (currentRetryCount > MaxOptionsRetryCount)
				return;

			GeneralOptionsPage = await AcuminatorVSPackage.Instance.GetGeneralOptionsPageAsync();

			SubscribeOnColoringSettingChangedEvent();
			ColoringSettingsChanged = true;     // Set ColoringSettingsChanged flag to indicate that settings have changed and we need to re-evaluate the tags.

			// Call RaiseTagsChangedAsync to ensure that the tags are updated after the options page is initialized.
			// But do it only if we are not already inside GetTags call.
			if (raiseTagsChanged)
				await RaiseTagsChangedAsync();
		}

		private void SubscribeOnColoringSettingChangedEvent()
		{
			if (GeneralOptionsPage != null && SubscribedToSettingsChanges && !Disposed)
			{
				GeneralOptionsPage.ColoringSettingChanged += ColoringSettingChangedHandler;
			}
		}

		protected static IEnumerable<ITagSpan<TTag>> GetIntersectionWithRequestedTags<TTag>(
																				IReadOnlyCollection<ITagSpan<TTag>> tags,
																				NormalizedSnapshotSpanCollection requestedSpans)
		where TTag : ITag
		{
			return tags?.Count > 0
				? tags.Where(tag => requestedSpans.IntersectsWith(tag.Span))
				: [];
		}

		protected virtual void ColoringSettingChangedHandler(object sender, SettingChangedEventArgs e)
		{
			ColoringSettingsChanged = true;

			// Coloring setting should be called from the UI thread and there is a safety check in RaiseTagsChanged
			// It should be OK to make a sync call
			RaiseTagsChanged();
		}

		/// <summary>
		/// Raises the tags changed asynchronously and do not observe the raised task.
		/// </summary>
		/// <remarks>
		/// The method is intended to be called from void-returning event handlers.
		/// </remarks>
		/// <param name="calledFrom">(Optional) The method raising the tag changed event.</param>
		protected void RaiseTagsChangedAsyncAndForget([CallerMemberName] string? calledFrom = null)
		{
			if (Disposed)
				return;

			if (ThreadHelper.CheckAccess())
				RaiseTagsChanged();
			else
			{
				string taggerName = this.GetType().Name;
				calledFrom = calledFrom.NullIfWhiteSpace() ?? nameof(RaiseTagsChangedAsyncAndForget);

				// See the VS cookbook for file and forget methods
				// https://github.com/microsoft/vs-threading/blob/main/docfx/docs/cookbook_vs.md#task-returning-fire-and-forget-methods
				var raiseTaggerChanged = () => RaiseTagsChangedAsync();
				raiseTaggerChanged.FileAndForgetAcuminatorTask($"vs/{AcuminatorVSPackage.PackageName}/{taggerName}/{calledFrom}");
			}
		}

		internal async Task RaiseTagsChangedAsync()
		{
			if (Disposed)
				return;

			if (!ThreadHelper.CheckAccess())
			{
				await AcuminatorVSPackage.JTF.SwitchToMainThreadAsync();

				// check after switch to the main thread
				if (Disposed)
					return;
			}

			RaiseTagsChangedImpl();
		}

		internal void RaiseTagsChanged()
		{
			if (Disposed)
				return;

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
			Disposed = true;
			_disposedNotification.CurrentTextDocumentDisposed -= CleanupOnTextDocumentDisposed;

			if (GeneralOptionsPage != null && SubscribedToSettingsChanges)
			{
				GeneralOptionsPage.ColoringSettingChanged -= ColoringSettingChangedHandler;
			}
		}
	}
}
