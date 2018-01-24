using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;



namespace PX.Analyzers.Coloriser.AsyncTagging
{
    internal class TagsCache<TTag>
    where TTag : ITag
    {
        public ITextSnapshot Snapshot { get; }

        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Set of known tags
        /// </summary>
        public IReadOnlyCollection<ITagSpan<TTag>> TagList { get; } 

        public TagsCache()
        {
            
            
        }       
    }
}
