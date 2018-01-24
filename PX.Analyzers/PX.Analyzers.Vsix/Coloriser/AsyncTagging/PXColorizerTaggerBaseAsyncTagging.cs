
// <summary> The asynchronous tagging part of the PXColorizerTaggerBase class</summary>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using PX.Analyzers.Vsix.Utilities;



namespace PX.Analyzers.Coloriser
{
    /// <content>
    /// A colorizer tagger base class.
    /// </content>
    public abstract partial class PXColorizerTaggerBase : ITagger<IClassificationTag>, IDisposable
    {
        protected CancellationTokenSource cancellationTokenSource;

        protected Task<IEnumerable<ITagSpan<IClassificationTag>>> TaggingTask;

        protected internal abstract ITagsCache<IClassificationTag> TagsCache { get; }

        protected internal abstract bool UseAsyncTagging { get; }

        public virtual IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0 || !Provider.Package.ColoringEnabled)
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();

            ITextSnapshot snapshot = spans[0].Snapshot;

            if (CheckIfRetaggingIsNotNecessary(snapshot))
                return TagsCache.ProcessedTags;

            ResetCacheAndFlags(snapshot);
            return UseAsyncTagging
                ? GetTagsAsync(snapshot)
                : GetTagsSynchronousImplementation(snapshot);
        }

        internal abstract IEnumerable<ITagSpan<IClassificationTag>> GetTagsSynchronousImplementation(ITextSnapshot snapshot);

        /// <summary>
        /// Gets the tags asynchronous in this collection.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the tags asynchronous in this collection.
        /// </returns>
        protected virtual IEnumerable<ITagSpan<IClassificationTag>> GetTagsAsync(ITextSnapshot snapshot)
        {
            
        }


        public virtual void Dispose()
        {
            if (!SubscribedToSettingsChanges)
                return;

            var genOptionsPage = Provider.Package?.GeneralOptionsPage;

            if (genOptionsPage != null)
            {
                genOptionsPage.ColoringSettingChanged -= ColoringSettingChangedHandler;
            }
        }
    }
}
