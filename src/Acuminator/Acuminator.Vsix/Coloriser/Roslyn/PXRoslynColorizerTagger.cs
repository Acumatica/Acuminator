﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Acuminator.Vsix.Coloriser
{
    public partial class PXRoslynColorizerTagger : PXColorizerTaggerBase
    {
        protected internal override bool UseAsyncTagging => true;

        public override TaggerType TaggerType => TaggerType.Roslyn;

        private readonly TagsCacheAsync<IClassificationTag> _classificationTagsCache;

        protected internal override ITagsCache<IClassificationTag> ClassificationTagsCache => _classificationTagsCache;

        private readonly TagsCacheAsync<IOutliningRegionTag> _outliningTagsCache;

        protected internal override ITagsCache<IOutliningRegionTag> OutliningsTagsCache => _outliningTagsCache;

        internal PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider aProvider, bool subscribeToSettingsChanges,
                                         bool useCacheChecking) :
                                    base(buffer, aProvider, subscribeToSettingsChanges, useCacheChecking)
        {
            _classificationTagsCache = new TagsCacheAsync<IClassificationTag>();
            _outliningTagsCache = new TagsCacheAsync<IOutliningRegionTag>();

            //Buffer.Changed += Buffer_Changed;
        }

        #region Commented Parsing optimizations
        //private bool isParsed;
        //private ParsedDocument documentCache;
        //private volatile static int walking;

        //private async void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        //{
        //    if (TagsChangedIsNull() || Buffer.CurrentSnapshot == null || e.Changes.IsNullOrEmpty())
        //        return;

        //    if (e.After != Buffer.CurrentSnapshot)
        //        return;

        //    try
        //    {
        //        // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).               
        //        int min = Int32.MaxValue, max = Int32.MinValue;

        //        foreach (var change in e.Changes)
        //        {
        //            min = Math.Min(min, change.NewPosition);
        //            max = Math.Max(max, change.NewPosition + change.NewLength);
        //        }

        //        TextSpan span = new TextSpan(min, max); 
        //        var parsedDoc = await ParsedDocument.Resolve(Buffer, Buffer.CurrentSnapshot).ConfigureAwait(false);

        //        documentCache = parsedDoc;

        //        if (System.Threading.Interlocked.CompareExchange(ref walking, 1, comparand: 0) == 0)
        //        {
        //            WalkDocumentSyntaxTreeForTags(parsedDoc);
        //            RaiseTagsChanged();
        //            walking = 0;
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        #endregion

        protected internal override IEnumerable<ITagSpan<IClassificationTag>> GetTagsSynchronousImplementation(ITextSnapshot snapshot)
        {
            _classificationTagsCache.SetCancellation(CancellationToken.None);
            _outliningTagsCache.SetCancellation(CancellationToken.None);

            Task<ParsedDocument?>? getDocumentTask = ParsedDocument.ResolveAsync(snapshot, CancellationToken.None);

            if (getDocumentTask == null)    // Razor cshtml returns a null document for some reason.        
                return ClassificationTagsCache.ProcessedTags; 

            try
            {
				//This method is deliberately synchronous so we ignore warnings
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				getDocumentTask.Wait();

			}
            catch (Exception)
            {
                return ClassificationTagsCache.ProcessedTags;     // TODO: report this to someone.
            }

            ParsedDocument? document = getDocumentTask.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

			if (document != null)
				WalkDocumentSyntaxTreeForTags(document, CancellationToken.None);
            //documentCache = document;
            //isParsed = true;
          
            return ClassificationTagsCache.ProcessedTags;
        }

        protected internal async override Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementationAsync(ITextSnapshot snapshot,
                                                                                                                     CancellationToken cToken)
        {
            _classificationTagsCache.SetCancellation(cToken);
            _outliningTagsCache.SetCancellation(cToken);

            Task<ParsedDocument?> getDocumentTask = ParsedDocument.ResolveAsync(snapshot, cToken);

            if (cToken.IsCancellationRequested || getDocumentTask == null)              // Razor cshtml returns a null document for some reason.        
                return ClassificationTagsCache.ProcessedTags;

            var documentTaskResult = await getDocumentTask.TryAwait();

            if (!documentTaskResult.IsSuccess)
                return ClassificationTagsCache.ProcessedTags;

            ParsedDocument? document = documentTaskResult.Result;            
            
            if (document == null || cToken.IsCancellationRequested)
                return ClassificationTagsCache.ProcessedTags;

            await WalkDocumentSyntaxTreeForTagsAsync(document, cToken).TryAwait();
            return ClassificationTagsCache.ProcessedTags;
        }    

        private void WalkDocumentSyntaxTreeForTags(ParsedDocument document, CancellationToken cancellationToken)
        {
            var syntaxWalker = new PXColoriserSyntaxWalker(this, document, cancellationToken);

            syntaxWalker.Visit(document.SyntaxRoot);
            ClassificationTagsCache.CompleteProcessing();
            OutliningsTagsCache.CompleteProcessing();
        }

        private Task WalkDocumentSyntaxTreeForTagsAsync(ParsedDocument document, CancellationToken cancellationToken)
        {
            return Task.Run(() => WalkDocumentSyntaxTreeForTags(document, cancellationToken));
        }      
    }
}
