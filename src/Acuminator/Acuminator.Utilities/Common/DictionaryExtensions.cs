using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Common
{
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Tries to add the value to the dictionary by the provided key if the key is not null, and item doesn't already exist.
		/// </summary>
		public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
		{
			dict.ThrowOnNull(nameof (dict));

			if (key != null && !dict.ContainsKey(key))
			{
				dict.Add(key, value);
				return true;
			}

			return false;
		}
	}
}
