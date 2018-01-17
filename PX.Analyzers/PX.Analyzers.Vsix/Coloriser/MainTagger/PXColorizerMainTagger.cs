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
    public class PXColorizerMainTagger : PXColorizerTaggerBase
    {
        public override TaggerType TaggerType => TaggerType.General;

        private readonly Dictionary<TaggerType, PXColorizerTaggerBase> taggersByType;      

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

        public override IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0 || !Provider.Package.ColoringEnabled)
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();
            
            if (CheckIfRetaggingIsNotNecessary(spans[0].Snapshot))
                return TagsList;
            
            ResetCacheAndFlags(spans[0].Snapshot);
            TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();

            if (!taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase activeTagger))
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();

            var tags = activeTagger.GetTags(spans);

            if (!tags.IsNullOrEmpty())
            {
                TagsList.AddRange(tags);
            }

            return TagsList;
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
