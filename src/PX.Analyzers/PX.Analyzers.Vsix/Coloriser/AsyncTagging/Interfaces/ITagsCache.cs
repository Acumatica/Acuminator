using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Coloriser
{
    public interface ITagsCache<TTag> : IReadOnlyCollection<ITagSpan<TTag>>
    where TTag : ITag
    {      
        bool IsCompleted { get; }
      
        IReadOnlyCollection<ITagSpan<TTag>> ProcessedTags { get; }
      
        void CompleteProcessing();

        void Reset();

        void AddTag(ITagSpan<TTag> tag);

        void AddTags(IEnumerable<ITagSpan<TTag>> tags);
    }
}
