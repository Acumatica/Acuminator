using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Acuminator.Utilities;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;

namespace Acuminator.Vsix.Coloriser
{
    /// <summary>
    /// A colorizer tagger base class.
    /// </summary>
    public class PXColorizerMainTagger : PXColorizerTaggerBase
    {
        public override TaggerType TaggerType => TaggerType.General;

        private readonly Dictionary<TaggerType, PXColorizerTaggerBase> _taggersByType;

        protected internal override bool UseAsyncTagging
        {
            get
            {
                TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();
                return _taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase tagger) 
                    ? tagger.UseAsyncTagging
                    : false;
            }
        }

        protected internal override ITagsCache<IClassificationTag> ClassificationTagsCache
        {
            get
            {
                TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();
                return _taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase tagger)
                    ? tagger.ClassificationTagsCache
                    : throw new NotSupportedException($"Tagger type {currentTaggerType} not supported");
            }
        }

        protected internal override ITagsCache<IOutliningRegionTag> OutliningsTagsCache
        {
            get
            {
                TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();
                return _taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase tagger)
                    ? tagger.OutliningsTagsCache
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

            _taggersByType = new Dictionary<TaggerType, PXColorizerTaggerBase>(capacity: 2)
            {
                { roslynTagger.TaggerType, roslynTagger },
                { regexTagger.TaggerType, regexTagger }
            };
        }

        protected internal override IEnumerable<ITagSpan<IClassificationTag>> GetTagsSynchronousImplementation(ITextSnapshot snapshot)
        {
            TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();

            if (!_taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase activeTagger))
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();
                    
            return activeTagger.GetTagsSynchronousImplementation(snapshot);
        }

        protected internal override Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementationAsync(ITextSnapshot snapshot, 
                                                                                                               CancellationToken cancellationToken)
        {
            TaggerType currentTaggerType = GetCurrentTaggerTypeFromSettings();

            if (!_taggersByType.TryGetValue(currentTaggerType, out PXColorizerTaggerBase activeTagger))
                return Task.FromResult(Enumerable.Empty<ITagSpan<IClassificationTag>>());

            if (activeTagger.UseAsyncTagging)
            {
                return activeTagger.GetTagsAsyncImplementationAsync(snapshot, cancellationToken);
            }
            else
            {
                var tags = activeTagger.GetTagsSynchronousImplementation(snapshot);
                return Task.FromResult(tags);
            }
        }

        public override void Dispose()
        {
            _taggersByType.Values.Distinct()
                                .ForEach(tagger => tagger.Dispose());
            base.Dispose();
        }

        protected internal override void ResetCacheAndFlags(ITextSnapshot newCache)
        {
            base.ResetCacheAndFlags(newCache);
            _taggersByType.Values.ForEach(tagger => tagger.ResetCacheAndFlags(newCache));
        }

        protected TaggerType GetCurrentTaggerTypeFromSettings()
        {
            return AcuminatorVSPackage.Instance?.UseRegexColoring == true
                ? TaggerType.RegEx
                : TaggerType.Roslyn;
        }       
    }
}
