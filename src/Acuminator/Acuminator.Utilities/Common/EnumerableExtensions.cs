﻿#nullable enable

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
		public static IEnumerable<T> ToEnumerable<T>(this T? item)
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
		public static void ForEach<T>(this IEnumerable<T> source, Action<T?> action)
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
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null)
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

		/// <summary>
		/// Concatenate <see cref="ImmutableArray{TItem}"/>s. This is an optimization method which allows to avoid boxing for <see cref="ImmutableArray{TItem}"/>s.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="sourceList">The source list to act on.</param>
		/// <param name="listToConcat">The list to concat.</param>
		/// <returns/>
		[DebuggerStepThrough]
		public static IEnumerable<TItem> Concat<TItem>(this ImmutableArray<TItem> sourceList, ImmutableArray<TItem> listToConcat)
		{
			for (int i = 0; i < sourceList.Length; i++)
				yield return sourceList[i];

			for (int i = 0; i < listToConcat.Length; i++)
				yield return listToConcat[i];
		}

		/// <summary>
		/// Where method for <see cref="SyntaxTokenList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="source">The <see cref="SyntaxTokenList"/> to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<SyntaxToken> Where(this SyntaxTokenList source, Func<SyntaxToken, bool> predicate)
		{
			predicate.ThrowOnNull(nameof(predicate));
			return WhereForSyntaxTokenListImplementation();

			IEnumerable<SyntaxToken> WhereForSyntaxTokenListImplementation()
			{
				for (int i = 0; i < source.Count; i++)
				{
					SyntaxToken token = source[i];

					if (predicate(token))
					{
						yield return token;
					}
				}
			}
		}

		/// <summary>
		/// FirstOrDefault method for <see cref="SyntaxTokenList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <param name="source">The <see cref="SyntaxTokenList"/> to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SyntaxToken FirstOrDefault(this SyntaxTokenList source, Func<SyntaxToken, bool> predicate)
		{
			predicate.ThrowOnNull(nameof(predicate));

			for (int i = 0; i < source.Count; i++)
			{
				SyntaxToken token = source[i];

				if (predicate(token))
					 return token;
			}

			return default;
		}

		/// <summary>
		/// Where method for <see cref="SyntaxList{TNode}"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <typeparam name="TNode">Type of the syntax node.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <param name="predicate">The predicate.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TNode> Where<TNode>(this SyntaxList<TNode> source, Func<TNode, bool> predicate)
		where TNode : SyntaxNode
		{
			predicate.ThrowOnNull(nameof(predicate));
			return WhereForStructListImplementation();

			IEnumerable<TNode> WhereForStructListImplementation()
			{
				for (int i = 0; i < source.Count; i++)
				{
					TNode item = source[i];

					if (predicate(item))
					{
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Select method for <see cref="SyntaxTriviaList"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <typeparam name="TResult">Type of the result.</typeparam>
		/// <param name="triviaList">The triviaList to act on.</param>
		/// <param name="selector">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TResult> Select<TResult>(this SyntaxTriviaList triviaList, Func<SyntaxTrivia, TResult> selector)
		{
			selector.ThrowOnNull(nameof(selector));
			return SelectForStructListImplementation();

			IEnumerable<TResult> SelectForStructListImplementation()
			{
				for (int i = 0; i < triviaList.Count; i++)
				{
					yield return selector(triviaList[i]);
				}
			}
		}

		/// <summary>
		/// Select many implementation for <see cref="ImmutableArray{T}"/> without boxing.
		/// </summary>
		/// <typeparam name="TCollectionHolder">Type of the item with collection.</typeparam>
		/// <typeparam name="TCollectionItem">Type of the collection item.</typeparam>
		/// <param name="array">The array to act on.</param>
		/// <param name="selector">The selector.</param>
		/// <returns/>
		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TCollectionItem> SelectMany<TCollectionHolder, TCollectionItem>(this ImmutableArray<TCollectionHolder> array, 
																								  Func<TCollectionHolder, IEnumerable<TCollectionItem>> selector)
		{
			selector.ThrowOnNull(nameof(selector));
			return GeneratorMethod();


			IEnumerable<TCollectionItem> GeneratorMethod()
			{
				foreach (TCollectionHolder collectionHolder in array)
				{
					foreach (TCollectionItem item in selector(collectionHolder))
					{
						yield return item;
					}
				}
			}
		}

		/// <summary>
		/// Reverses <see cref="ImmutableArray{T}"/>. This is an optimization method which allows to avoid boxing.
		/// </summary>
		/// <typeparam name="TItem">Type of the item.</typeparam>
		/// <param name="source">The source to act on.</param>
		/// <returns/>
		[DebuggerStepThrough]
		public static ImmutableArray<TItem?> Reverse<TItem>(this ImmutableArray<TItem?> source)
		{
			if (source.Length == 0)
				return source;

			ImmutableArray<TItem?>.Builder builder = ImmutableArray.CreateBuilder<TItem?>(source.Length);

			for (int i = source.Length - 1; i >= 0; i--)
			{		
				builder.Add(source[i]);
			}

			return builder.ToImmutable();
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddRange<T>(this HashSet<T> hashset, IEnumerable<T> items)
		{
			hashset.ThrowOnNull(nameof(hashset));
			items.ThrowOnNull(nameof(items));

			foreach (var item in items)
				hashset.Add(item);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty<T>(this IEnumerable<T> source) =>
			source.CheckIfNull(nameof(source)) switch
			{
				IReadOnlyCollection<T> readOnlyCollection => readOnlyCollection.Count == 0,
				ICollection<T> genericCollection => genericCollection.Count == 0,
				ICollection collection => collection.Count == 0,
				string str => str.Length == 0,
				_ => !source.Any(),
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

		public static bool Contains<T>(this IEnumerable<T> sequence, Func<T?, bool> predicate)
		{
			sequence.ThrowOnNull(nameof(sequence));
			predicate.ThrowOnNull(nameof(predicate));

			return sequence.Any(predicate);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains<T>(this T?[] array, T? item) => Array.IndexOf(array, item) >= 0;

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this IEnumerable<T>? source) => 
			source?.IsEmpty() ?? true;

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNullOrEmpty<T>([NotNullWhen(returnValue: false)] this T[]? array) => 
			array == null || array.Length == 0;

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<T>(this ImmutableArray<T> source, Func<T, bool> condition) =>
			FindIndex(source, startInclusive: 0, endExclusive: source.Length, condition);

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<T>(this ImmutableArray<T> source, int startInclusive, Func<T, bool> condition) =>
			FindIndex(source, startInclusive, endExclusive: source.Length, condition);

		[DebuggerStepThrough]
		public static int FindIndex<T>(this ImmutableArray<T> source, int startInclusive, int endExclusive, Func<T, bool> condition)
		{
			condition.ThrowOnNull(nameof(condition));

			if (startInclusive < 0 || startInclusive >= source.Length)
				throw new ArgumentOutOfRangeException(nameof(startInclusive));
			else if (endExclusive <= 0 || endExclusive > source.Length)
				throw new ArgumentOutOfRangeException(nameof(endExclusive));

			for (int i = startInclusive; i < endExclusive; i++)
			{
				if (condition(source[i]))
					return i;
			}

			return -1;
		}

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
			source.ThrowOnNull(nameof(source));

			if (source.Count == 0)
				return ImmutableArray<T>.Empty;

			var builder = ImmutableArray.CreateBuilder<T>(initialCapacity: source.Count);
			builder.AddRange(source);

			return builder.ToImmutable();
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<TNode>(this SeparatedSyntaxList<TNode> source, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			return FindIndex(source, startInclusive: 0, endExclusive: source.Count, condition);
		}

		[DebuggerStepThrough]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FindIndex<TNode>(this SeparatedSyntaxList<TNode> source, int startInclusive, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			return FindIndex(source, startInclusive, endExclusive: source.Count, condition);
		}

		[DebuggerStepThrough]
		public static int FindIndex<TNode>(this SeparatedSyntaxList<TNode> source, int startInclusive, int endExclusive, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			condition.ThrowOnNull(nameof(condition));

			if (startInclusive < 0 || startInclusive >= source.Count)
				throw new ArgumentOutOfRangeException(nameof(startInclusive));
			else if (endExclusive <= 0 || endExclusive > source.Count)
				throw new ArgumentOutOfRangeException(nameof(endExclusive));

			for (int i = startInclusive; i < endExclusive; i++)
			{
				if (condition(source[i]))
					return i;
			}

			return -1;
		}

		[DebuggerStepThrough]
		public static bool All<TNode>(this SeparatedSyntaxList<TNode> source, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			condition.ThrowOnNull(nameof(condition));

			for (int i = 0; i < source.Count; i++)
			{
				if (!condition(source[i]))
					return false;
			}

			return true;
		}

		[DebuggerStepThrough]
		public static bool Any<TNode>(this SeparatedSyntaxList<TNode> source, Func<TNode, bool> condition)
		where TNode : SyntaxNode
		{
			condition.ThrowOnNull(nameof(condition));

			for (int i = 0; i < source.Count; i++)
			{
				if (condition(source[i]))
					return true;
			}

			return false;
		}

		[DebuggerStepThrough]
		public static bool Contains<TNode>(this SyntaxList<TNode> source, TNode node)
		where TNode : SyntaxNode
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (Equals(node, source[i]))
					return true;
			}

			return false;
		}


	}
}
