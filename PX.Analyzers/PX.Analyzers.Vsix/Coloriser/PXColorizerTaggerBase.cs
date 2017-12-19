using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace PX.Analyzers.Coloriser
{
    /// <summary>
    /// A colorizer tagger base class.
    /// </summary>
    public abstract class PXColorizerTaggerBase : ITagger<IClassificationTag>, IDisposable
    {
#pragma warning disable CS0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

        protected ITextBuffer Buffer { get; }

        protected PXColorizerTaggerProvider Provider { get; }

        protected ITextSnapshot Cache { get; set; }

        protected List<ITagSpan<IClassificationTag>> TagsList { get; } = new List<ITagSpan<IClassificationTag>>();

        public abstract IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans);

        protected PXColorizerTaggerBase(ITextBuffer buffer, PXColorizerTaggerProvider aProvider)
        {
            buffer.ThrowOnNull(nameof(buffer));
            aProvider.ThrowOnNull(nameof(aProvider));

            Buffer = buffer;
            Provider = aProvider;

            var genOptionsPage = Provider.Package?.GeneralOptionsPage;

            if (genOptionsPage != null)
            {
                genOptionsPage.ColoringSettingChanged += ColoringSettingChangedHandler;
            }
        }

        private void ColoringSettingChangedHandler(object sender, Vsix.SettingChangedEventArgs e)
        {
            RaiseTagsChanged();
        }

        protected bool TagsChangedIsNull() => TagsChanged == null;

        protected void RaiseTagsChanged() => TagsChanged?.Invoke(this, 
            new SnapshotSpanEventArgs(
                new SnapshotSpan(Buffer.CurrentSnapshot,
                    new Span(0, Buffer.CurrentSnapshot.Length))));

        void IDisposable.Dispose()
        {
            var genOptionsPage = Provider.Package?.GeneralOptionsPage;

            if (genOptionsPage != null)
            {
                genOptionsPage.ColoringSettingChanged -= ColoringSettingChangedHandler;
            }
        }
    }
}
