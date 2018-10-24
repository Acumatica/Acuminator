using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.Common
{
	public static class EnumerableExtensions
	{
		[DebuggerStepThrough]
		public static IEnumerable<T> ToEnumerable<T>(this T item)
		{
			if (item != null)
				yield return item;
		}

		/// <summary>
		/// Perfrom an <paramref name="action"/> on all items of the <paramref name="source"/> collection.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="action">The action.</param>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			source.ThrowOnNull(nameof(source));
			action.ThrowOnNull(nameof(action));

			// perf optimization. try to not use enumerator if possible
			switch (source)
			{
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
		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source) => source != null
			? new ReadOnlyCollection<T>(source.ToList())
			: throw new ArgumentNullException(nameof(source));

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlyCollection<T> ToReadOnlyCollectionShallow<T>(this IList<T> list) => list != null
			? new ReadOnlyCollection<T>(list)
			: throw new ArgumentNullException(nameof(list));

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
		public static Stack<T> ToStack<T>(this IEnumerable<T> source) => source != null 
			? new Stack<T>(source)
			: throw new ArgumentNullException(nameof(source));

		/// <summary>
		/// Adds a single element to the end of an IEnumerable.
		/// </summary>
		/// <typeparam name="T">Type of enumerable to return.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="element">The element.</param>
		/// <returns>
		/// IEnumerable containing all the input elements, followed by the specified additional element.
		/// </returns>
		public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T element) => ConcatIterator(element, source, false);

		/// <summary>
		/// Adds a single element to the start of an IEnumerable.
		/// </summary>
		/// <typeparam name="T">Type of enumerable to return.</typeparam>
		/// <param name="tail">The tail to act on.</param>
		/// <param name="head">The head.</param>
		/// <returns>
		/// IEnumerable containing the specified additional element, followed by all the input elements.
		/// </returns>
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> tail, T head) => ConcatIterator(head, tail, true);

		private static IEnumerable<T> ConcatIterator<T>(T extraElement, IEnumerable<T> source, bool insertAtStart)
		{
			if (insertAtStart)
				yield return extraElement;

			if (source != null)
			{
				foreach (var e in source)
				{
					yield return e;
				}
			}

			if (!insertAtStart)
				yield return extraElement;
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
				foreach (TItem item in source)
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
		public static bool IsEmpty<T>(this IReadOnlyCollection<T> source) => source != null
			? source.Count == 0
			: throw new ArgumentNullException(nameof(source));

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty<T>(this ICollection<T> source) => source != null
			? source.Count == 0
			: throw new ArgumentNullException(nameof(source));

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty(this ICollection source) => source != null
			? source.Count == 0
			: throw new ArgumentNullException(nameof(source));

		public static bool Contains<T>(this IEnumerable<T> sequence, Func<T, bool> predicate)
		{
			sequence.ThrowOnNull(nameof(sequence));
			predicate.ThrowOnNull(nameof(predicate));

			return sequence.Any(predicate);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this T[] array, T item) => Array.IndexOf(array, item) >= 0;

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> source) => source == null
			? true
			: source.IsEmpty();

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>(this T[] array) => array == null || array.Length == 0;
	}
}
