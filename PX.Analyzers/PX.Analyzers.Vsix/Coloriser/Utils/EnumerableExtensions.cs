using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PX.Analyzers.Coloriser
{
    internal static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source.ThrowOnNull(nameof(source));
            action.ThrowOnNull(nameof(action));
            
            // perf optimization. try to not use enumerator if possible
            switch (source)
            {
                case IList<T> list:
                    for (int i = 0, count = list.Count; i < count; i++)
                    {
                        action(list[i]);
                    }

                    return;
                default:
                    foreach (var value in source)
                    {
                        action(value);
                    }

                    break;
            }                    
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            source.ThrowOnNull(nameof(source));
            return comparer != null 
                ? new HashSet<T>(source, comparer) 
                : new HashSet<T>(source);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            switch (source)
            {
                case IReadOnlyCollection<T> readOnlyCollection:
                    return readOnlyCollection.Count == 0;
                case ICollection<T> genericCollection:
                    return genericCollection.Count == 0;
                case ICollection collection:
                    return collection.Count == 0;
                default:
                    string str = source as string;
                    return str != null
                        ? str.Length == 0
                        : !source.Any();
            }
        }

        public static bool IsEmpty<T>(this IReadOnlyCollection<T> source) => source.Count == 0;    
        
        public static bool IsEmpty<T>(this ICollection<T> source) => source.Count == 0;
      
        public static bool IsEmpty(this string source) => source.Length == 0;
        
        /// <remarks>
        /// This method is necessary to avoid an ambiguity between <see cref="IsEmpty{T}(IReadOnlyCollection{T})"/> and <see cref="IsEmpty{T}(ICollection{T})"/>.
        /// </remarks>
        public static bool IsEmpty<T>(this T[] source) => source.Length == 0;
        
        /// <remarks>
        /// This method is necessary to avoid an ambiguity between <see cref="IsEmpty{T}(IReadOnlyCollection{T})"/> and <see cref="IsEmpty{T}(ICollection{T})"/>.
        /// </remarks>
        public static bool IsEmpty<T>(this List<T> source) => source.Count == 0;
               
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => source == null ? true : source.IsEmpty();     
    }
}
