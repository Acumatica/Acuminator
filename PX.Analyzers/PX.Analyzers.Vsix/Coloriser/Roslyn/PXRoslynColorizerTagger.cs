using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
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


namespace PX.Analyzers.Coloriser
{
    public partial class PXRoslynColorizerTagger : PXColorizerTaggerBase
    {
        //private bool isParsed;
        //private ParsedDocument documentCache;
        //private volatile static int walking;

        internal PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider aProvider) :
                                    base(buffer, aProvider)
        {
            //Buffer.Changed += Buffer_Changed;
        }

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

        public override IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {           
            if (spans == null || spans.Count == 0)
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();

            //if (isParsed)
            //    return TagsList;

            if (Cache != null && Cache == spans[0].Snapshot)
                return TagsList;
         
           
            TagsList.Clear();
            Cache = spans[0].Snapshot;
            GetTagsFromSnapshot(spans);
            return TagsList;
        }

        private void GetTagsFromSnapshot(NormalizedSnapshotSpanCollection spans)
        {
            Task<ParsedDocument> getDocumentTask = ParsedDocument.Resolve(Buffer, Cache);

            if (getDocumentTask == null)    // Razor cshtml returns a null document for some reason.        
                return;

            try
            {
                getDocumentTask.Wait();
            }
            catch (Exception e)
            {         
                return;     // TODO: report this to someone.
            }

            ParsedDocument document = getDocumentTask.Result;
            WalkDocumentSyntaxTreeForTags(document);
            //documentCache = document;
            //isParsed = true;
        }

        private void WalkDocumentSyntaxTreeForTags(ParsedDocument document)
        {
           var syntaxWalker = new PXColoriserSyntaxWalker(this, document);          
           syntaxWalker.Visit(document.SyntaxRoot);
        }
    }
}
