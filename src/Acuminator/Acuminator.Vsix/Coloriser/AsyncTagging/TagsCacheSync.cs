using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using Acuminator.Utilities;


namespace Acuminator.Vsix.Coloriser
{
    public class TagsCacheSync<TTag> : ITagsCache<TTag>
    where TTag : ITag
    {
        private const int DefaultCapacity = 64;

        public bool IsCompleted { get; private set; }

        private readonly List<ITagSpan<TTag>> _resultTagsList;
        
        public IReadOnlyCollection<ITagSpan<TTag>> ProcessedTags { get; }

        public int Count => ProcessedTags.Count;

        public TagsCacheSync(int? capacity = null)
        {
            _resultTagsList = new List<ITagSpan<TTag>>(capacity ?? DefaultCapacity);
            ProcessedTags = _resultTagsList.AsReadOnly();
        }

        public void CompleteProcessing()
        {          
            IsCompleted = true;
        }

        public void Reset()
        {
            _resultTagsList.Clear();
            IsCompleted = false;
        }

        public void AddTag(ITagSpan<TTag> tag)
        {
            if (tag == null)
                return;

            _resultTagsList.Add(tag);
        }

        public void AddTags(IEnumerable<ITagSpan<TTag>> tags) => _resultTagsList.AddRange(tags);
        
        public IEnumerator<ITagSpan<TTag>> GetEnumerator() => ProcessedTags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();       
    }
}
