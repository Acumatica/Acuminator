using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Coloriser
{
    public class TagsCacheAsync<TTag> : ITagsCache<TTag>
    where TTag : ITag
    {
        private object syncLock = new object();
        private const int defaultCapacity = 64;

        protected CancellationToken CancellationToken { get; set;  }

        public bool IsCompleted { get; private set; }

        private readonly List<ITagSpan<TTag>> resultTagsList;
        private readonly ConcurrentQueue<ITagSpan<TTag>> tagsQueue = new ConcurrentQueue<ITagSpan<TTag>>();

        public IReadOnlyCollection<ITagSpan<TTag>> ProcessedTags
        {
            get
            {
                lock (syncLock)
                {
                    return resultTagsList.ToList();
                }
            }
        }
        public int Count => ProcessedTags.Count;

        public TagsCacheAsync(int? capacity = null)
        {
            resultTagsList = new List<ITagSpan<TTag>>(capacity ?? defaultCapacity);
            CancellationToken = CancellationToken.None;
        }

        internal void SetCancellation(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public void PersistIntermediateResult()
        {
            if (CancellationToken.IsCancellationRequested)
                return;

            lock (syncLock)
            {
                int counter = 0;

                while (tagsQueue.TryDequeue(out ITagSpan<TTag> tag) && counter < ColoringConstants.ChunkSize)
                {
                    if (CancellationToken.IsCancellationRequested)
                        return;

                    resultTagsList.Add(tag);
                    counter++;
                }
            }
        }

        public void CompleteProcessing()
        {
            if (CancellationToken.IsCancellationRequested)
                return;

            lock (syncLock)
            {
                while (tagsQueue.TryDequeue(out ITagSpan<TTag> tag))
                {
                    if (CancellationToken.IsCancellationRequested)
                        return;

                    resultTagsList.Add(tag);
                }
            }

            IsCompleted = true;
        }

        public void Reset()
        {
            lock (syncLock)
            {
                resultTagsList.Clear();
                resultTagsList.Capacity = defaultCapacity;
            }

            tagsQueue.Clear();
            IsCompleted = false;
        }

        public void AddTag(ITagSpan<TTag> tag)
        {
            if (tag == null || CancellationToken.IsCancellationRequested)
                return;

            tagsQueue.Enqueue(tag);
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

                tagsQueue.Enqueue(tag);
            }
        }

        public IEnumerator<ITagSpan<TTag>> GetEnumerator() => ProcessedTags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
