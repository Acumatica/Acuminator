using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers.Vsix.Formatter
{
	static class DictionaryHelpers
	{
		public static Dictionary<TKey, TValue> MergeWith<TKey, TValue>(this Dictionary<TKey, TValue> dict,
			IReadOnlyDictionary<TKey, TValue> values)
		{
			if (dict == null) throw new ArgumentNullException(nameof (dict));
			if (values == null) throw new ArgumentNullException(nameof (values));

			foreach (var item in values)
			{
				dict[item.Key] = item.Value;
			}

			return dict;
		}
	}
}
