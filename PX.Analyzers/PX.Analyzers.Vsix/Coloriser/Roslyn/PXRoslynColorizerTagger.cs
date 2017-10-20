using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using CSharp = Microsoft.CodeAnalysis.CSharp;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Coloriser
{
    internal class PXRoslynColorizerTagger : PXColorizerTaggerBase
    {
        internal PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider aProvider) : 
                                    base(buffer, aProvider)
        {
        }

        public override IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0)
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();

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
            
        }
    }
}
