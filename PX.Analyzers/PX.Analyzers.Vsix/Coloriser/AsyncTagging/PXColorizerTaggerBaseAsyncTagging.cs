
// <summary> The asynchronous tagging part of the PXColorizerTaggerBase class</summary>


using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// Gets the tags asynchronous in this collection.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process the tags asynchronous in this collection.
        /// </returns>
        protected virtual IEnumerable<ITagSpan<IClassificationTag>> GetTagsAsync(ITextSnapshot snapshot)
        {
            throw new NotImplementedException();
        }     
    }
}
