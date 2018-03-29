using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.Coloriser
{
    /// <summary>
    /// Values that represent tagger types.
    /// </summary>
    public enum TaggerType
    {
        /// <summary>
        /// The general tagger which chooses other taggers according to the settings.
        /// </summary>
        General,

        /// <summary>
        /// The tagger based on Roslyn 
        /// </summary>
        Roslyn,

        /// <summary>
        /// The tagger based on regular expressions
        /// </summary>
        RegEx,

        /// <summary>
        /// The tagger used for outlining
        /// </summary>
        Outlining
    };

    public abstract class PXTaggerBase : IDisposable
    {
#pragma warning disable CS0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

        protected ITextBuffer Buffer { get; }

        protected internal ITextSnapshot Snapshot { get; private set; }

        protected bool ColoringSettingsChanged { get; private set; }

        protected bool SubscribedToSettingsChanges { get; private set; }

        protected PXTaggerProviderBase ProviderBase { get; }

        /// <summary>
        /// The type of the tagger.
        /// </summary>      
        public abstract TaggerType TaggerType { get; }

        protected bool CacheCheckingEnabled { get; }

        protected PXTaggerBase(ITextBuffer buffer, PXTaggerProviderBase provider, bool subscribeToSettingsChanges, bool useCacheChecking)
        {
            buffer.ThrowOnNull(nameof(buffer));
            provider.ThrowOnNull(nameof(provider));

            Buffer = buffer;
            ProviderBase = provider;
            SubscribedToSettingsChanges = subscribeToSettingsChanges;
            CacheCheckingEnabled = useCacheChecking;

            if (SubscribedToSettingsChanges)
            {
                var genOptionsPage = ProviderBase.Package?.GeneralOptionsPage;

                if (genOptionsPage != null)
                {
                    genOptionsPage.ColoringSettingChanged += ColoringSettingChangedHandler;
                }
            }
        }

        protected virtual void ColoringSettingChangedHandler(object sender, Vsix.SettingChangedEventArgs e)
        {
            ColoringSettingsChanged = true;
            RaiseTagsChanged();
        }

        internal virtual void RaiseTagsChanged() => TagsChanged?.Invoke(this,
            new SnapshotSpanEventArgs(
                new SnapshotSpan(Buffer.CurrentSnapshot,
                    new Span(0, Buffer.CurrentSnapshot.Length))));

        protected internal virtual void ResetCacheAndFlags(ITextSnapshot newCache)
        {
            ColoringSettingsChanged = false;
            Snapshot = newCache;
        }

        protected virtual bool CheckIfRetaggingIsNotNecessary(ITextSnapshot snapshot) =>
            CacheCheckingEnabled && Snapshot != null && Snapshot == snapshot && !ColoringSettingsChanged;

        public virtual void Dispose()
        {
            if (!SubscribedToSettingsChanges)
                return;

            var genOptionsPage = ProviderBase.Package?.GeneralOptionsPage;

            if (genOptionsPage != null)
            {
                genOptionsPage.ColoringSettingChanged -= ColoringSettingChangedHandler;
            }
        }
    }
}
