using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.Coloriser
{
    public class TagsCacheAsync<TTag> : ITagsCache<TTag>
    where TTag : ITag
    {        
        protected CancellationToken CancellationToken { get; set;  }

        public bool IsCompleted { get; private set; }

        private readonly ConcurrentQueue<ITagSpan<TTag>> _tagsQueue = new ConcurrentQueue<ITagSpan<TTag>>();

        public IReadOnlyCollection<ITagSpan<TTag>> ProcessedTags => _tagsQueue;
        
        public int Count => ProcessedTags.Count;

        public TagsCacheAsync()
        {
            CancellationToken = CancellationToken.None;
        }

        internal void SetCancellation(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public void CompleteProcessing()
        {
            if (CancellationToken.IsCancellationRequested)
                return;

            IsCompleted = true;            
        }

        public void Reset()
        {
            _tagsQueue.Clear();

            IsCompleted = false;                    
        }

        public void AddTag(ITagSpan<TTag> tag)
        {
            if (tag == null || CancellationToken.IsCancellationRequested)
                return;

            _tagsQueue.Enqueue(tag);
        }

        public void AddTags(IEnumerable<ITagSpan<TTag>> tags)
        {
            List<ITagSpan<TTag>> tagsCopy = tags?.ToList();

            if (tagsCopy.IsNullOrEmpty() || CancellationToken.IsCancellationRequested)
                return;

            foreach (ITagSpan<TTag> tag in tagsCopy)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;

                _tagsQueue.Enqueue(tag);
            }
        }

        public IEnumerator<ITagSpan<TTag>> GetEnumerator() => ProcessedTags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
