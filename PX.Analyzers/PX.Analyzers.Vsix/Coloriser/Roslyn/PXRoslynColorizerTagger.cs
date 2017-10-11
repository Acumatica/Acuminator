using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PX.Analyzers.Coloriser
{
    internal class PXRoslynColorizerTagger : ITagger<IClassificationTag>
    {
        private ITextBuffer theBuffer;
        private ITextSnapshot cache;
        private readonly PXColorizerTaggerProvider provider;

#pragma warning disable CS0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

        internal PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider aProvider)
        {
            theBuffer = buffer;
            provider = aProvider;
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            return Enumerable.Empty<ITagSpan<IClassificationTag>>();
        }
    }
}
