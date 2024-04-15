#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.Common
{
	public static class EnumerableExtraExtensions
	{
		public static T? FirstOrNullable<T>(this IEnumerable<T> source)
		where T : struct
		{
			return source.CheckIfNull()
						 .Cast<T?>()
						 .FirstOrDefault();
		}

		public static T? LastOrNullable<T>(this IEnumerable<T> source)
		where T : struct
		{
			return source.CheckIfNull()
						 .Cast<T?>()
						 .LastOrDefault();
		}

		public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> comparer)
		{
			source1.ThrowOnNull();
			source2.ThrowOnNull();
			return source1.ToSet(comparer).SetEquals(source2);
		}

		public static bool SetEquals<T>(this IEnumerable<T> source1, IEnumerable<T> source2)
		{
			source1.ThrowOnNull();
			source2.ThrowOnNull();

			return source1.ToSet().SetEquals(source2);
		}

		public static ISet<T> ToSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer) =>
			 new HashSet<T>(source, comparer);

		public static ISet<T> ToSet<T>(this IEnumerable<T> source) =>
			source as ISet<T> ?? new HashSet<T>(source);

		public static bool IsSingle<T>(this IEnumerable<T>? list)
		{
			if (list == null)
				return false;

			using var enumerator = list.GetEnumerator();
			return enumerator.MoveNext() && !enumerator.MoveNext();
		}

		public static bool All(this IEnumerable<bool> source)
		{
			source.ThrowOnNull();

			foreach (bool b in source)
			{
				if (!b)
				{
					return false;
				}
			}

			return true;
		}

		public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, IComparer<T> comparer) =>
			 source.CheckIfNull()
				   .OrderBy(Functions<T>.Identity, comparer);

		public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source, IComparer<T> comparer) =>
			source.CheckIfNull()
				  .OrderByDescending(Functions<T>.Identity, comparer);

		public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source, Comparison<T> compare) =>
			source.CheckIfNull()
				  .OrderBy(Comparer<T>.Create(compare));

		public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source)
		where T : IComparable<T> =>
			source.CheckIfNull()
				  .OrderBy(Comparisons<T>.Comparer);

		public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, IComparer<T> comparer) => 
			source.CheckIfNull()
				  .ThenBy(Functions<T>.Identity, comparer);

		public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source, Comparison<T> compare) =>
			 source.CheckIfNull()
				   .ThenBy(Comparer<T>.Create(compare));

		public static IOrderedEnumerable<T> ThenBy<T>(this IOrderedEnumerable<T> source)
		where T : IComparable<T> => 
			source.CheckIfNull()
				  .ThenBy(Comparisons<T>.Comparer);

		public static bool IsSorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
		{
			using var e = enumerable.CheckIfNull().GetEnumerator();

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

		public static bool SequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer)
		{
			comparer.ThrowOnNull();

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

		/// <summary>
		/// Prepends <paramref name="source"/> with an <paramref name="itemToAdd"/>.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="itemToAdd">The item to add.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TItem> PrependItem<TItem>(this IEnumerable<TItem>? source, TItem itemToAdd) =>
			source.PrependOrAppend(itemToAdd, isAppending: false);

		/// <summary>
		/// Appends <paramref name="source"/> with an <paramref name="item"/>.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="itemToAdd">The item to add.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TItem> AppendItem<TItem>(this IEnumerable<TItem>? source, TItem itemToAdd) =>
			source.PrependOrAppend(itemToAdd, isAppending: true);

		[DebuggerStepThrough]
		private static IEnumerable<TItem> PrependOrAppend<TItem>(this IEnumerable<TItem>? source, TItem itemToAdd, bool isAppending)
		{
			if (!isAppending)
				yield return itemToAdd;

			if (source != null)
			{
				foreach (var item in source)
					yield return item;
			}

			if (isAppending)
				yield return itemToAdd;
		}

		public static T[] Append<T>(this T[]? source, T element) => ConcatArray(element, source, false);

		public static T[] Prepend<T>(this T[]? tail, T head) => ConcatArray(head, tail, true);

		private static T[] ConcatArray<T>(T extraElement, T[]? source, bool insertAtStart)
		{
			source ??= [];

			T[] result = new T[source.Length + 1];
			source.CopyTo(result, insertAtStart ? 1 : 0);
			result[insertAtStart ? 0 : source.Length] = extraElement;
			return result;
		}

		public static T[] Append<T>(this T[]? source, params T[] elements) => ConcatArrays(elements, source, insertAtStart: false);

		public static T[] Prepend<T>(this T[]? tail, params T[] head) => ConcatArrays(head, tail, insertAtStart: true);

		private static T[] ConcatArrays<T>(T[] extraElements, T[]? source, bool insertAtStart)
		{
			source ??= [];

			T[] result = new T[source.Length + extraElements.Length];
			source.CopyTo(result, insertAtStart ? extraElements.Length : 0);
			extraElements.CopyTo(result, insertAtStart ? 0 : source.Length);
			return result;
		}


		public static List<TItem> ItemsWithMaxValues<TItem>(this IEnumerable<TItem> source, Func<TItem, double> selector)
		{
			source.ThrowOnNull();
			selector.ThrowOnNull();

			double maxValue = double.MinValue;
			var result = new List<TItem>(capacity: 2);

			foreach (TItem item in source)
			{
				double value = selector(item);

				if (maxValue > value)
					continue;

				if (maxValue < value)
				{
					result.Clear();
					maxValue = value;
				}
				
				result.Add(item);
			}

			return result;
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
