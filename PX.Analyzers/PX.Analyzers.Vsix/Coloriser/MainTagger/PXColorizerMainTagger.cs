using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using PX.Analyzers.Vsix.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace PX.Analyzers.Coloriser
{
    /// <summary>
    /// A colorizer tagger base class.
    /// </summary>
    public class PXColorizerMainTagger : PXColorizerTaggerBase
    {
        public override TaggerType TaggerType => TaggerType.General;

        private readonly Dictionary<TaggerType, PXColorizerTaggerBase> taggersByType;

        protected internal override bool UseAsyncTagging
        {
            get
            {
                TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();
                return taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase tagger) 
                    ? tagger.UseAsyncTagging
                    : false;
            }
        }

        protected internal override ITagsCache<IClassificationTag> TagsCache
        {
            get
            {
                TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();
                return taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase tagger)
                    ? tagger.TagsCache
                    : throw new NotSupportedException($"Tagger type {currentTaggerType} not supported");
            }
        }

        public PXColorizerMainTagger(ITextBuffer buffer, PXColorizerTaggerProvider aProvider, bool subscribeToSettingsChanges, 
                                     bool useCacheChecking) :
                                base(buffer, aProvider, subscribeToSettingsChanges, useCacheChecking)
        {
            PXColorizerTaggerBase roslynTagger = new PXRoslynColorizerTagger(buffer, aProvider, subscribeToSettingsChanges: false, 
                                                                             useCacheChecking: false);
            PXColorizerTaggerBase regexTagger = new PXRegexColorizerTagger(buffer, aProvider, subscribeToSettingsChanges: false, 
                                                                           useCacheChecking: false);

            taggersByType = new Dictionary<TaggerType, PXColorizerTaggerBase>(capacity: 2)
            {
                { roslynTagger.TaggerType, roslynTagger },
                { regexTagger.TaggerType, regexTagger }
            };
        }

        protected internal override IEnumerable<ITagSpan<IClassificationTag>> GetTagsSynchronousImplementation(ITextSnapshot snapshot)
        {
            TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();

            if (!taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase activeTagger))
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();
                    
            return activeTagger.GetTagsSynchronousImplementation(snapshot);
        }

        protected internal override Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementation(ITextSnapshot snapshot, 
                                                                                                               CancellationToken cancellationToken)
        {
            TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();

            if (!taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase activeTagger))
                return Task.FromResult(Enumerable.Empty<ITagSpan<IClassificationTag>>());

            if (activeTagger.UseAsyncTagging)
            {
                return activeTagger.GetTagsAsyncImplementation(snapshot, cancellationToken);
            }
            else
            {
                var tags = activeTagger.GetTagsSynchronousImplementation(snapshot);
                return Task.FromResult(tags);
            }
        }

        public override void Dispose()
        {
            taggersByType.Values.Distinct()
                                .ForEach(tagger => tagger.Dispose());
            base.Dispose();
        }

        protected internal override void ResetCacheAndFlags(ITextSnapshot newCache)
        {
            base.ResetCacheAndFlags(newCache);
            taggersByType.Values.ForEach(tagger => tagger.ResetCacheAndFlags(newCache));
        }

        protected TaggerType GetCurrentTaggerTypeFromSettings()
        {
            return Provider.Package?.UseRegexColoring == true
                ? TaggerType.RegEx
                : TaggerType.Roslyn;
        }       
    }
}
