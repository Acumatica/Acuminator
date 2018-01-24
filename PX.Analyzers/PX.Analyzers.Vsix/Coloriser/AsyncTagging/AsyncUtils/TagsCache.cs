using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Coloriser.AsyncTagging
{
    internal class TagsCache<TTag> : IReadOnlyCollection<ITagSpan<TTag>>
    where TTag : ITag
    {
        private const int defaultCapacity = 64;
        private const int defaultChunkSize = 50;
      
        public bool IsCompleted { get; private set; }

        private readonly List<ITagSpan<TTag>> resultTagsList;
        private readonly ConcurrentQueue<ITagSpan<TTag>> tagsQueue = new ConcurrentQueue<ITagSpan<TTag>>();

        public IReadOnlyCollection<ITagSpan<TTag>> ProcessedTags { get; }

        public int Count => ProcessedTags.Count;

        public TagsCache(int? capacity = null)
        {
            resultTagsList = new List<ITagSpan<TTag>>(capacity ?? defaultCapacity);
            ProcessedTags = resultTagsList.AsReadOnly();
        }

        public void PersistIntermediateResult()
        {
            int counter = 0;

            while (tagsQueue.TryDequeue(out ITagSpan<TTag> tag) && counter < defaultChunkSize)
            {
                resultTagsList.Add(tag);
                counter++;
            }
        }

        public void CompleteProcessing()
        {
            while (tagsQueue.TryDequeue(out ITagSpan<TTag> tag))
            {
                resultTagsList.Add(tag);              
            }

            IsCompleted = true;
        }

        public void Reset()
        {
            resultTagsList.Clear();
            tagsQueue.Clear();
            IsCompleted = false;
        }

        public void AddTag(ITagSpan<TTag> tag)
        {
            if (tag == null)
                return;

            tagsQueue.Enqueue(tag);
        }

        public IEnumerator<ITagSpan<TTag>> GetEnumerator() => ProcessedTags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
