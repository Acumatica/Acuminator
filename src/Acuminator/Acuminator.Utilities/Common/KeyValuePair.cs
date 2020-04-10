using System;
using System.Collections.Generic;
using System.Text;

namespace Acuminator.Utilities.Common
{
	/// <summary>
	/// A <see cref="KeyValuePair{TKey, TValue}"/> utils.
	/// </summary>
	public static class KeyValuePair
	{
		/// <summary>
		/// Creates a new KeyValuePair{TKey,TValue}
		/// </summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <typeparam name="TValue">Type of the value.</typeparam>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>
		/// A KeyValuePair{TKey,TValue}
		/// </returns>
		public static KeyValuePair<TKey, TValue> Create<TKey, TValue>(TKey key, TValue value) =>
			new KeyValuePair<TKey, TValue>(key, value);

		/// <summary>
		/// A type deconstructor extension method that extracts the individual members from a KeyValuePair{TKey,TValue}
		/// </summary>
		/// <typeparam name="TKey">Type of the key.</typeparam>
		/// <typeparam name="TValue">Type of the value.</typeparam>
		/// <param name="pair">The pair to act on.</param>
		/// <param name="key">[out] The key.</param>
		/// <param name="value">[out] The value.</param>
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
		{
			key = pair.Key;
			value = pair.Value;
		}
	}
}
