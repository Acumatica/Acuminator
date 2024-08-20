#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Common
{
	public static class EnumerableExtensions
	{
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> ToEnumerable<T>(this T? item) => item != null ? [item] : [];

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
			action.ThrowOnNull();

			// perf optimization. try to not use enumerator if possible
			switch (source.CheckIfNull())
			{
				case IList<T> list:
					for (int i = 0; i < list.Count; i++)
						action(list[i]);

					return;

				case IReadOnlyList<T> readOnlyList:
					for (int i = 0; i < readOnlyList.Count; i++)
						action(readOnlyList[i]);

					return;

				default:
					foreach (var value in source)
						action(value);

					return;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[DebuggerStepThrough]
		public static List<T> ToList<T>(this IEnumerable<T> source, int capacity)
		{
			var list = new List<T>(capacity);
			list.AddRange(source);
			return list;
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source) =>
			new(source.CheckIfNull().ToList());

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlyCollection<T> ToReadOnlyCollectionShallow<T>(this IList<T> list) =>
			new(list.CheckIfNull());

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
		{
			source.ThrowOnNull();
			return comparer != null
				? new HashSet<T>(source, comparer)
				: new HashSet<T>(source);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Stack<T> ToStack<T>(this IEnumerable<T> source) =>
			new(source.CheckIfNull());

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddRange<T>(this HashSet<T> hashset, IEnumerable<T> items)
		{
			hashset.ThrowOnNull();
			items.ThrowOnNull();

			foreach (var item in items)
				hashset.Add(item);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty<T>(this IEnumerable<T> source) =>
			source.CheckIfNull() switch
			{
				ICollection<T> genericCollection 		  => genericCollection.Count == 0,
				IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection.Count == 0,
				ICollection collection 					  => collection.Count == 0,
				_ 										  => !source.Any(),
			};

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

		public static bool Contains<T>(this IEnumerable<T> sequence, Func<T?, bool> predicate) =>
			sequence.CheckIfNull().Any(predicate.CheckIfNull());

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this T?[] array, T? item) => Array.IndexOf(array, item) >= 0;

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this IEnumerable<T>? source) => 
			source == null || source.IsEmpty();

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this T[]? array) => 
			array?.Length is null or 0;

		/// <summary>
		/// An <see cref="IReadOnlyCollection{T}"/> extension method that converts a source to an immutable array a bit more optimally.
		/// </summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <returns>
		/// Source as an <see cref="ImmutableArray{T}"/>
		/// </returns>
		[DebuggerStepThrough]
		public static ImmutableArray<T> ToImmutableArray<T>(this IReadOnlyCollection<T> source)
		{
			source.ThrowOnNull();

			if (source.Count == 0)
				return ImmutableArray<T>.Empty;

			var builder = ImmutableArray.CreateBuilder<T>(initialCapacity: source.Count);
			builder.AddRange(source);

			return builder.ToImmutable();
		}
	}
}
