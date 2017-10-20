using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Coloriser
{
    /// <summary>
    /// A colorizer tagger base class.
    /// </summary>
    internal abstract class PXColorizerTaggerBase : ITagger<IClassificationTag>
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
        }
    }
}
