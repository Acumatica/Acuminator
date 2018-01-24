using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers.Coloriser.AsyncTagging
{
    internal class TagsCache
    {
        internal BackgroundCacheData? BackgroundCacheData;
        internal TrackingCacheData? TrackingCacheData;

        internal bool IsEmpty
        {
            get { return !BackgroundCacheData.HasValue && !TrackingCacheData.HasValue; }
        }

        internal TagCache(BackgroundCacheData? backgroundCacheData, TrackingCacheData? trackingCacheData)
        {
            BackgroundCacheData = backgroundCacheData;
            TrackingCacheData = trackingCacheData;
        }

        internal static TagCache Empty
        {
            get { return new TagCache(null, null); }
        }
    }

    internal struct BackgroundCacheData
    {
        internal readonly ITextSnapshot Snapshot;

        /// <summary>
        /// Set of line ranges for which tags are known
        /// </summary>
        internal readonly NormalizedLineRangeCollection VisitedCollection;

        /// <summary>
        /// Set of known tags
        /// </summary>
        internal readonly ReadOnlyCollection<ITagSpan<TTag>> TagList;

        internal SnapshotSpan Span
        {
            get
            {
                var range = VisitedCollection.OverarchingLineRange;
                if (!range.HasValue)
                {
                    return new SnapshotSpan(Snapshot, 0, 0);
                }

                var lineRange = new SnapshotLineRange(Snapshot, range.Value.StartLineNumber, range.Value.Count);
                return lineRange.ExtentIncludingLineBreak;
            }
        }

        internal BackgroundCacheData(SnapshotLineRange lineRange, ReadOnlyCollection<ITagSpan<TTag>> tagList)
        {
            Snapshot = lineRange.Snapshot;
            VisitedCollection = new NormalizedLineRangeCollection();
            VisitedCollection.Add(lineRange.LineRange);
            TagList = tagList;
        }

        internal BackgroundCacheData(ITextSnapshot snapshot, NormalizedLineRangeCollection visitedCollection, ReadOnlyCollection<ITagSpan<TTag>> tagList)
        {
            Snapshot = snapshot;
            VisitedCollection = visitedCollection;
            TagList = tagList;
        }

        /// <summary>
        /// Determine tag cache state we have for the given SnapshotSpan
        /// </summary>
        internal TagCacheState GetTagCacheState(SnapshotSpan span)
        {
            // If the requested span doesn't even intersect with the overarching SnapshotSpan
            // of the cached data in the background then a more exhaustive search isn't needed
            // at this time
            var cachedSpan = Span;
            if (!cachedSpan.IntersectsWith(span))
            {
                return TagCacheState.None;
            }

            var lineRange = SnapshotLineRange.CreateForSpan(span);
            var unvisited = VisitedCollection.GetUnvisited(lineRange.LineRange);
            return unvisited.HasValue
                ? TagCacheState.Partial
                : TagCacheState.Complete;
        }

        /// <summary>
        /// Create a TrackingCacheData instance from this BackgroundCacheData
        /// </summary>
        internal TrackingCacheData CreateTrackingCacheData()
        {
            // Create the list.  Initiate an ITrackingSpan for every SnapshotSpan present
            var trackingList = TagList.Select(
                tagSpan =>
                {
                    var snapshot = tagSpan.Span.Snapshot;
                    var trackingSpan = snapshot.CreateTrackingSpan(tagSpan.Span, SpanTrackingMode.EdgeExclusive);
                    return Tuple.Create(trackingSpan, tagSpan.Tag);
                })
                .ToReadOnlyCollection();

            return new TrackingCacheData(
                Snapshot.CreateTrackingSpan(Span, SpanTrackingMode.EdgeInclusive),
                trackingList);
        }
    }\
}
