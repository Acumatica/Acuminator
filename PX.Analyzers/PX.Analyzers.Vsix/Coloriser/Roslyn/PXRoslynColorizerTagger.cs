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
            if (spans == null || spans.Count == 0)
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();

            var workspace = theBuffer.GetWorkspace();
            Document document = spans[0].Snapshot.GetOpenDocumentInCurrentContextWithChanges();

            if (document == null)
            {
                // Razor cshtml returns a null document for some reason.
                return null;
            }

            Task task = GetSemanticModel(document);

            try
            {
                task.Wait();
            }
            catch (Exception e)
            {
                // TODO: report this to someone.
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();
            }

            return Enumerable.Empty<ITagSpan<IClassificationTag>>();
        }

        private async Task GetSemanticModel(Document document)
        {
            // the ConfigureAwait() calls are important,
            // otherwise we'll deadlock VS
            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
            var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
        }
    }
}
