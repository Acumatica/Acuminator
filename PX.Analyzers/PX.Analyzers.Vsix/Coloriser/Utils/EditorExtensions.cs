using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;



namespace PX.Analyzers.Vsix.Utilities
{
    public static class EditorExtensions
    {
        #region Span
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span CreateOverarching(this Span left, Span right)
        {
            int start = Math.Min(left.Start, right.Start);
            int end = Math.Max(left.End, right.End);
            return Span.FromBounds(start, end);
        }

        #endregion

        #region SnapshotSpan
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan CreateOverarching(this SnapshotSpan left, SnapshotSpan right)
        {
            Span span = left.Span.CreateOverarching(right.Span);
            return new SnapshotSpan(left.Snapshot, span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITextSnapshotLine GetStartLine(this SnapshotSpan span) => span.Start.GetContainingLine();
       
        /// <summary>
        /// Get the last line included in the SnapshotSpan
        /// </summary>
        public static ITextSnapshotLine GetLastLine(this SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;
            var snapshotEndPoint = new SnapshotPoint(snapshot, snapshot.Length);

            if (snapshotEndPoint == span.End)
            {
                ITextSnapshotLine line = span.End.GetContainingLine();

                if (line.Length == 0)
                {
                    return line;
                }
            }

            return span.Length > 0
                ? span.End.Subtract(1).GetContainingLine()
                : GetStartLine(span);
        }

        #endregion

        #region ITextSnapshot
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char GetChar(this ITextSnapshot snapshot, int position) => GetPoint(snapshot, position).GetChar();


        /// <summary>
        /// Get the SnapshotSpan for the extent of the entire ITextSnapshot
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan GetExtent(this ITextSnapshot snapshot) => new SnapshotSpan(snapshot, 0, snapshot.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan GetSpan(this ITextSnapshot snapshot, int start, int length) => new SnapshotSpan(snapshot, start, length);

        /// <summary>
        /// Get the SnapshotPoint for the given position within the ITextSnapshot
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetPoint(this ITextSnapshot snapshot, int position)
        {
            return new SnapshotPoint(snapshot, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetPointInLine(this ITextSnapshot snapshot, int line, int column)
        {
            var snapshotLine = snapshot.GetLineFromLineNumber(line);
            return snapshotLine.Start.Add(column);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetStartPoint(this ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetEndPoint(this ITextSnapshot snapshot)
        {
            return new SnapshotPoint(snapshot, snapshot.Length);
        }    
        #endregion

        #region ITextBuffer

        /// <summary>
        /// Any ITextBuffer instance is possibly an IProjectionBuffer (which is a text buffer composed of parts of other ITextBuffers).  
        /// This will return all of the real ITextBuffer buffers composing the provided ITextBuffer
        /// </summary>
        public static IEnumerable<ITextBuffer> GetSourceBuffersRecursive(this ITextBuffer textBuffer)
        {
            var projectionBuffer = textBuffer as IProjectionBuffer;

            if (projectionBuffer != null)
            {
                return projectionBuffer.GetSourceBuffersRecursive();
            }

            return new [] { textBuffer };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan GetExtent(this ITextBuffer textBuffer)
        {
            return textBuffer.CurrentSnapshot.GetExtent();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetPoint(this ITextBuffer textBuffer, int position)
        {
            return textBuffer.CurrentSnapshot.GetPoint(position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetPointInLine(this ITextBuffer textBuffer, int line, int column)
        {
            return textBuffer.CurrentSnapshot.GetPointInLine(line, column);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan GetSpan(this ITextBuffer textBuffer, int start, int length)
        {
            return textBuffer.CurrentSnapshot.GetSpan(start, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetText(this ITextBuffer buffer, params string[] lines)
        {
            var text = String.Join(Environment.NewLine, lines);
            var edit = buffer.CreateEdit(EditOptions.DefaultMinimalChange, 0, null);
            edit.Replace(new Span(0, buffer.CurrentSnapshot.Length), text);
            edit.Apply();
        }

        #endregion

        #region ITextView     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotPoint GetCaretPoint(this ITextView textView)
        {
            return textView.Caret.Position.BufferPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITextSnapshotLine GetCaretLine(this ITextView textView)
        {
            return textView.Caret.Position.BufferPosition.GetContainingLine();
        }

        /// <summary>
        /// Move the caret to the given position in the ITextView
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CaretPosition MoveCaretTo(this ITextView textView, int position)
        {
            return textView.Caret.MoveTo(new SnapshotPoint(textView.TextSnapshot, position));
        }

        /// <summary>
        /// Move the caret to the given position in the ITextView with the set amount of virtual spaces
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveCaretTo(this ITextView textView, int position, int virtualSpaces)
        {
            var point = new SnapshotPoint(textView.TextSnapshot, position);
            var virtualPoint = new VirtualSnapshotPoint(point, virtualSpaces);
            textView.Caret.MoveTo(virtualPoint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CaretPosition MoveCaretToLine(this ITextView textView, int lineNumber)
        {
            var snapshotLine = textView.TextSnapshot.GetLineFromLineNumber(lineNumber);
            return MoveCaretTo(textView, snapshotLine.Start.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CaretPosition MoveCaretToLine(this ITextView textView, int lineNumber, int column)
        {
            var snapshotLine = textView.TextSnapshot.GetLineFromLineNumber(lineNumber);
            var point = snapshotLine.Start.Add(column);
            return MoveCaretTo(textView, point.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetText(this ITextView textView, params string[] lines)
        {
            textView.TextBuffer.SetText(lines);
        }

        #endregion

        #region IProjectionBuffer
        /// <summary>
        /// IProjectionBuffer instances can compose recursively. This will look recursively down the source buffers to find all of the critical ones
        /// </summary>
        public static IEnumerable<ITextBuffer> GetSourceBuffersRecursive(this IProjectionBuffer projectionBuffer)
        {
            var toVisit = new Queue<IProjectionBuffer>();
            toVisit.Enqueue(projectionBuffer);

            var found = new HashSet<ITextBuffer>();

            while (toVisit.Count > 0)
            {
                IProjectionBuffer current = toVisit.Dequeue();

                if (found.Contains(current))
                {
                    continue;
                }

                found.Add(current);

                foreach (ITextBuffer sourceBuffer in current.SourceBuffers)
                {
                    var sourceProjection = sourceBuffer as IProjectionBuffer;

                    if (sourceProjection != null)
                    {
                        toVisit.Enqueue(sourceProjection);
                    }
                    else
                    {
                        found.Add(sourceBuffer);
                    }
                }
            }

            return found.Where(x => !(x is IProjectionBuffer));
        }

        #endregion

        #region PropertyCollection
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetPropertySafe<TProperty>(this PropertyCollection propertyCollection, object key, out TProperty value)
        {
            try
            {
                return propertyCollection.TryGetProperty<TProperty>(key, out value);
            }
            catch (Exception)
            {
                // If the value exists but is not convertible to the provided type then
                // an exception will be thrown.  Collapse this into an empty option.  
                // Helps guard against cases where other extensions override our values
                // with ones of unexpected types
                value = default(TProperty);
                return false;
            }
        }

        #endregion

        #region ITrackingSpan
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan? GetSpanSafe(this ITrackingSpan trackingSpan, ITextSnapshot snapshot)
        {
            try
            {
                return trackingSpan.GetSpan(snapshot);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        #endregion

        #region NormalizedSnapshotSpanCollection
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SnapshotSpan GetOverarchingSpan(this NormalizedSnapshotSpanCollection collection)
        {
            collection.ThrowOnNull(nameof(collection));

            SnapshotSpan start = collection[0];
            SnapshotSpan end = collection[collection.Count - 1];
            return new SnapshotSpan(start.Start, end.End);
        }

        #endregion

        #region ITextBufferFactoryService

        /// <summary>
        /// Create an ITextBuffer with the specified lines
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITextBuffer CreateTextBuffer(this ITextBufferFactoryService textBufferFactoryService, params string[] lines)
        {
            textBufferFactoryService.ThrowOnNull(nameof(textBufferFactoryService));
            return CreateTextBuffer(textBufferFactoryService, null, lines);
        }

        /// <summary>
        /// Create an ITextBuffer with the specified content type and lines
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITextBuffer CreateTextBuffer(this ITextBufferFactoryService textBufferFactoryService, IContentType contentType, params string[] lines)
        {
            textBufferFactoryService.ThrowOnNull(nameof(textBufferFactoryService));

            var textBuffer = contentType != null
                ? textBufferFactoryService.CreateTextBuffer(contentType)
                : textBufferFactoryService.CreateTextBuffer();

            if (lines.Length != 0)
            {
                var text = lines.Aggregate((x, y) => x + Environment.NewLine + y);
                textBuffer.Replace(new Span(0, 0), text);
            }

            return textBuffer;
        }

        #endregion
    }
}
