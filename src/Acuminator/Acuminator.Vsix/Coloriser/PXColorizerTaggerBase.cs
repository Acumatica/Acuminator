
// <summary> The asynchronous tagging part of the PXColorizerTaggerBase class</summary>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Acuminator.Utilities;



namespace Acuminator.Vsix.Coloriser
{
    /// <content>
    /// A colorizer tagger base class.
    /// </content>
    public abstract class PXColorizerTaggerBase : PXTaggerBase, ITagger<IClassificationTag>, IDisposable
    {      
        public BackgroundTagging BackgroundTagging { get; protected set; }

        protected internal abstract ITagsCache<IClassificationTag> ClassificationTagsCache { get; }

        protected internal abstract ITagsCache<IOutliningRegionTag> OutliningsTagsCache { get; }

        protected internal abstract bool UseAsyncTagging { get; }

        protected PXColorizerTaggerProvider Provider => ProviderBase as PXColorizerTaggerProvider;

        protected PXColorizerTaggerBase(ITextBuffer buffer, PXColorizerTaggerProvider aProvider, bool subscribeToSettingsChanges,
                                        bool useCacheChecking) :
                                   base(buffer, aProvider, subscribeToSettingsChanges, useCacheChecking)
        {
        }    

        protected internal override void ResetCacheAndFlags(ITextSnapshot newCache)
        {
            base.ResetCacheAndFlags(newCache);
            ClassificationTagsCache.Reset();
            OutliningsTagsCache.Reset();
        }

        public virtual IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0 || !Provider.Package.ColoringEnabled)
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();

            ITextSnapshot snapshot = spans[0].Snapshot;

            if (CheckIfRetaggingIsNotNecessary(snapshot))
            {
                return ClassificationTagsCache.ProcessedTags;
            }

            if (UseAsyncTagging)
            {
                return GetTagsAsync(snapshot);
            }
            else
            {
                ResetCacheAndFlags(snapshot);
                return GetTagsSynchronousImplementation(snapshot);
            }                      
        }

		#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods - this method is async by its nature.
		/// <summary>
		/// Gets the tags asynchronous in this collection.
		/// </summary>
		/// <param name="snapshot">The snapshot.</param>
		/// <returns>
		/// An enumerator that allows foreach to be used to process the tags asynchronous in this collection.
		/// </returns>
		protected virtual IEnumerable<ITagSpan<IClassificationTag>> GetTagsAsync(ITextSnapshot snapshot)

		{
            if (BackgroundTagging != null)
            {
                BackgroundTagging.CancelTagging();   //Cancel currently running task
                BackgroundTagging = null;
            }

            ResetCacheAndFlags(snapshot);
            BackgroundTagging = BackgroundTagging.StartBackgroundTagging(this);

            return ClassificationTagsCache.ProcessedTags;
        }
		#pragma warning restore VSTHRD200

		protected internal abstract IEnumerable<ITagSpan<IClassificationTag>> GetTagsSynchronousImplementation(ITextSnapshot snapshot);

        protected internal abstract Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementationAsync(ITextSnapshot snapshot, 
																													CancellationToken cancellationToken);

        public override void Dispose()
        {
            BackgroundTagging?.Dispose();
            ClassificationTagsCache?.Reset();
            OutliningsTagsCache?.Reset();

            base.Dispose();
        }
    }
}
