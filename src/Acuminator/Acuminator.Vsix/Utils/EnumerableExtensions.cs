using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;



namespace Acuminator.Vsix.Utilities
{
    internal static class EnumerableExtensions
    {
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source.ThrowOnNull(nameof(source));
            action.ThrowOnNull(nameof(action));
            
            // perf optimization. try to not use enumerator if possible
            switch (source)
            {
                case T[] array:
                    Array.ForEach(array, action);
                    return;

                case List<T> list:
                    list.ForEach(action);
                    return;
                
                case IList<T> iList:
                    for (int i = 0; i < iList.Count; i++)
                    {
                        action(iList[i]);
                    }

                    return;
                default:
                    foreach (var value in source)
                    {
                        action(value);
                    }

                    return;
            }                    
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            source.ThrowOnNull(nameof(source));
            return comparer != null 
                ? new HashSet<T>(source, comparer) 
                : new HashSet<T>(source);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this IEnumerable<T> source)      
        {
            source.ThrowOnNull(nameof(source));

            switch (source)
            {
                case IReadOnlyCollection<T> readOnlyCollection:
                    return readOnlyCollection.Count == 0;
                case ICollection<T> genericCollection:
                    return genericCollection.Count == 0;
                case ICollection collection:
                    return collection.Count == 0;
                case string str:
                    return str.Length == 0;
                default:                                      
                    return !source.Any();
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this IReadOnlyCollection<T> source)
        {
            source.ThrowOnNull(nameof(source));

            return source.Count == 0;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty<T>(this ICollection<T> source)
        {
            source.ThrowOnNull(nameof(source));

            return source.Count == 0;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this ICollection source)
        {
            source.ThrowOnNull(nameof(source));

            return source.Count == 0;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => source == null ? true : source.IsEmpty();

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
        {
            source.ThrowOnNull(nameof(source));

            return new ReadOnlyCollection<T>(source.ToList());
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyCollection<T> ToReadOnlyCollectionShallow<T>(this IList<T> list)
        {
            list.ThrowOnNull(nameof(list));

            return new ReadOnlyCollection<T>(list);
        }

        /// <summary>
        /// Concatenate structure list to this collection. This is an optimization method which allows to avoid boxing for collections implemented as structs.
        /// </summary>
        /// <typeparam name="TItem">Type of the item.</typeparam>
        /// <typeparam name="TStructList">Type of the structure list.</typeparam>
        /// <param name="source">The source to act on.</param>
        /// <param name="structList">List implemented as structure.</param>
        /// <returns>
        /// An enumerator that allows foreach to be used to process concatenate structure list in this collection.
        /// </returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TItem> ConcatStructList<TItem, TStructList>(this IEnumerable<TItem> source, TStructList structList)
        where TStructList : struct, IReadOnlyCollection<TItem>
        {
            if (source != null)
            {
                foreach(TItem item in source)
                {
                    yield return item;                     
                }
            }

            if (structList.Count > 0)
            {
                foreach (TItem item in structList)
                {
                    yield return item;
                }
            }
        }
    }
}
