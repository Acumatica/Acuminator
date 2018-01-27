using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Coloriser
{
    public partial class PXRoslynColorizerTagger : PXColorizerTaggerBase
    {
        protected internal override bool UseAsyncTagging => true;

        public override TaggerType TaggerType => TaggerType.Roslyn;

        private readonly TagsCacheAsync<IClassificationTag> classificationTagsCache;

        protected internal override ITagsCache<IClassificationTag> ClassificationTagsCache => classificationTagsCache;

        private readonly TagsCacheAsync<IOutliningRegionTag> outliningTagsCache;

        protected internal override ITagsCache<IOutliningRegionTag> OutliningsTagsCache => outliningTagsCache;

        internal PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider aProvider, bool subscribeToSettingsChanges,
                                         bool useCacheChecking) :
                                    base(buffer, aProvider, subscribeToSettingsChanges, useCacheChecking)
        {
            classificationTagsCache = new TagsCacheAsync<IClassificationTag>();
            outliningTagsCache = new TagsCacheAsync<IOutliningRegionTag>();

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
            classificationTagsCache.SetCancellation(CancellationToken.None);
            outliningTagsCache.SetCancellation(CancellationToken.None);

            Task<ParsedDocument> getDocumentTask = ParsedDocument.Resolve(Buffer, Snapshot, CancellationToken.None);

            if (getDocumentTask == null)    // Razor cshtml returns a null document for some reason.        
                return ClassificationTagsCache.ProcessedTags; 

            try
            {
                getDocumentTask.Wait();
            }
            catch (Exception e)
            {
                return ClassificationTagsCache.ProcessedTags;     // TODO: report this to someone.
            }

            ParsedDocument document = getDocumentTask.Result;
            WalkDocumentSyntaxTreeForTags(document, CancellationToken.None);
            //documentCache = document;
            //isParsed = true;
          
            return ClassificationTagsCache.ProcessedTags;
        }

        protected internal async override Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementation(ITextSnapshot snapshot,
                                                                                                                     CancellationToken cToken)
        {
            classificationTagsCache.SetCancellation(cToken);
            outliningTagsCache.SetCancellation(cToken);

            Task<ParsedDocument> getDocumentTask = ParsedDocument.Resolve(Buffer, Snapshot, cToken);

            if (cToken.IsCancellationRequested || getDocumentTask == null)              // Razor cshtml returns a null document for some reason.        
                return ClassificationTagsCache.ProcessedTags;

            var documentTaskResult = await getDocumentTask.TryAwait();
            bool isSuccess = documentTaskResult.Key;

            if (!isSuccess)
                return ClassificationTagsCache.ProcessedTags;

            ParsedDocument document = documentTaskResult.Value;            
            
            if (document == null || cToken.IsCancellationRequested)
                return ClassificationTagsCache.ProcessedTags;

            isSuccess = await WalkDocumentSyntaxTreeForTagsAsync(document, cToken).TryAwait();
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
