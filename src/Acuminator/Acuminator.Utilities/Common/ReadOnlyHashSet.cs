using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Utilities.Common
{
	public static class ReadOnlyHashSet
	{
		public static ReadOnlyHashSet<T> ToReadOnly<T>(this IEnumerable<T> source, IEqualityComparer<T>? comparer = null) =>
			new ReadOnlyHashSet<T>(source, comparer);

		public static ReadOnlyHashSet<T> WrapInReadOnly<T>(this HashSet<T> hashSet) => 
			ReadOnlyHashSet<T>.ReadOnlyCopy(hashSet);

		public static ReadOnlyHashSet<T> CopyToReadOnly<T>(this HashSet<T> hashSet, IEqualityComparer<T>? comparer = null) =>
			ReadOnlyHashSet<T>.ReadOnlyCopy(hashSet, comparer);
	}

	/// <summary>
	/// A read only hash set.
	/// </summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public sealed class ReadOnlyHashSet<T> : IReadOnlyCollection<T>, IReadOnlyDictionary<T, T>
	{
		public static ReadOnlyHashSet<T> Empty { get; } = new();

		private readonly HashSet<T> _hashSet;

		public int Count => _hashSet.Count;

#pragma warning disable CS8768 // Nullability of reference types in return type doesn't match implemented member (possibly because of nullability attributes).
		T? IReadOnlyDictionary<T, T>.this[T key] => Contains(key)
			? key
			: default;
#pragma warning restore CS8768

		IEnumerable<T> IReadOnlyDictionary<T, T>.Keys => _hashSet;

		IEnumerable<T> IReadOnlyDictionary<T, T>.Values => _hashSet;

		private ReadOnlyHashSet()
		{
			_hashSet = [];
		}

		public ReadOnlyHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer = null) =>
			_hashSet = new HashSet<T>(collection.CheckIfNull(), comparer);

		private ReadOnlyHashSet(HashSet<T> hashSet) =>
			_hashSet = hashSet.CheckIfNull();

		public static ReadOnlyHashSet<T> Wrap(HashSet<T> hashSet) => new(hashSet);

		public static ReadOnlyHashSet<T> ReadOnlyCopy(HashSet<T> hashSet, IEqualityComparer<T>? comparer = null) => 
			new(hashSet, comparer ?? hashSet.Comparer);

		public bool Contains(T item) => _hashSet.Contains(item);

		bool IReadOnlyDictionary<T, T>.TryGetValue(T key, out T value)
		{
			if (Contains(key))
			{
				value = key;
				return true;
			}
			else
			{
				value = default!;
				return false;
			}
		}

		bool IReadOnlyDictionary<T, T>.ContainsKey(T key) => Contains(key);

		public IEnumerator<T> GetEnumerator() => _hashSet.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		IEnumerator<KeyValuePair<T, T>> IEnumerable<KeyValuePair<T, T>>.GetEnumerator()
		{
			foreach (var item in _hashSet)
			{
				yield return KeyValuePair.Create(item, item);
			}
		}
	}
}
