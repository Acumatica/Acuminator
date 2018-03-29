using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.Coloriser
{
    public class TagsCacheSync<TTag> : ITagsCache<TTag>
    where TTag : ITag
    {
        private const int defaultCapacity = 64;

        public bool IsCompleted { get; private set; }

        private readonly List<ITagSpan<TTag>> resultTagsList;
        
        public IReadOnlyCollection<ITagSpan<TTag>> ProcessedTags { get; }

        public int Count => ProcessedTags.Count;

        public TagsCacheSync(int? capacity = null)
        {
            resultTagsList = new List<ITagSpan<TTag>>(capacity ?? defaultCapacity);
            ProcessedTags = resultTagsList.AsReadOnly();
        }

        public void CompleteProcessing()
        {          
            IsCompleted = true;
        }

        public void Reset()
        {
            resultTagsList.Clear();
            IsCompleted = false;
        }

        public void AddTag(ITagSpan<TTag> tag)
        {
            if (tag == null)
                return;

            resultTagsList.Add(tag);
        }

        public void AddTags(IEnumerable<ITagSpan<TTag>> tags) => resultTagsList.AddRange(tags);
        
        public IEnumerator<ITagSpan<TTag>> GetEnumerator() => ProcessedTags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();       
    }
}
