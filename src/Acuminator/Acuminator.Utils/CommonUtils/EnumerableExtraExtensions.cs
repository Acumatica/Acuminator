using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;


namespace Acuminator.Utilities.Extra
{
    public static class EnumerableExtraExtensions
    {
        public static T? FirstOrNullable<T>(this IEnumerable<T> source)
         where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Cast<T?>().FirstOrDefault();
        }

        public static T? FirstOrNullable<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Cast<T?>().FirstOrDefault(v => predicate(v.Value));
        }

        public static T? LastOrNullable<T>(this IEnumerable<T> source)
            where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Cast<T?>().LastOrDefault();
        }

        public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> comparer)
        {
            source1.ThrowOnNull(nameof(source1));
            source2.ThrowOnNull(nameof(source2));
            return source1.ToSet(comparer).SetEquals(source2);
        }

        public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2)
        {
            source1.ThrowOnNull(nameof(source1));
            source2.ThrowOnNull(nameof(source2));

            return source1.ToSet().SetEquals(source2);
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            source.ThrowOnNull(nameof(source));
            return new HashSet<T>(source, comparer);
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> source)
        {
            source.ThrowOnNull(nameof(source));
            return source as ISet<T> ?? new HashSet<T>(source);
        }

        public static bool IsSingle<T>(this IEnumerable<T> list)
        {
            using (var enumerator = list.GetEnumerator())
            {
                return enumerator.MoveNext() && !enumerator.MoveNext();
            }
        }

        public static bool All(this IEnumerable<bool> source)
        {
            source.ThrowOnNull(nameof(source));

            foreach (bool b in source)
            {
                if (!b)
                {
                    return false;
                }
            }

            return true;
        }

        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IComparer<T> comparer)
        {
            source.ThrowOnNull(nameof(source));
            return source.OrderBy(Functions<T>.Identity, comparer);
        }

        public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> compare)
        {
            source.ThrowOnNull(nameof(source));
            return source.OrderBy(Comparer<T>.Create(compare));
        }

        public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source) 
        where T : IComparable<T>
        {
            source.ThrowOnNull(nameof(source));
            return source.OrderBy(Comparisons<T>.Comparer);
        }

        public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IComparer<T> comparer)
        {
            source.ThrowOnNull(nameof(source));
            return source.ThenBy(Functions<T>.Identity, comparer);
        }

        public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Comparison<T> compare)
        {
            source.ThrowOnNull(nameof(source));
            return source.ThenBy(Comparer<T>.Create(compare));
        }

        public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source) 
        where T : IComparable<T>
        {
            source.ThrowOnNull(nameof(source));
            return source.ThenBy(Comparisons<T>.Comparer);
        }
      
        public static bool IsSorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
        {
            using (var e = enumerable.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    return true;
                }

                var previous = e.Current;

                while (e.MoveNext())
                {
                    if (comparer.Compare(previous, e.Current) > 0)
                    {
                        return false;
                    }

                    previous = e.Current;
                }

                return true;
            }
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
        {
            Debug.Assert(comparer != null);

            if (first == second)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            using (var enumerator = first.GetEnumerator())
            using (var enumerator2 = second.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!enumerator2.MoveNext() || !comparer(enumerator.Current, enumerator2.Current))
                    {
                        return false;
                    }
                }

                if (enumerator2.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        public static IComparer<T> ToComparer<T>(this Comparison<T> comparison) => Comparer<T>.Create(comparison);      

        private static class Comparisons<T>
        where T : IComparable<T>
        {
            public static readonly Comparison<T> CompareTo = (t1, t2) => t1.CompareTo(t2);
            public static readonly IComparer<T> Comparer = Comparer<T>.Create(CompareTo);
        }
    }

    /// <summary>
    /// Cached versions of commonly used delegates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Functions<T>
    {
        public static readonly Func<T, T> Identity = t => t;
        public static readonly Func<T, bool> True = t => true;
    }
}
