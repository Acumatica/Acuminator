using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public class OverridableItemsCollection<T> : Dictionary<string, OverridableItem<T>>
	{
		public IEnumerable<OverridableItem<T>> Items => Values;

		public OverridableItemsCollection()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		/// <summary>
		/// Adds a range of items maintaining the declaration order. Returns the number following the last assigned declaration order.
		/// </summary>
		/// <param name="itemsToAdd">The items to add.</param>
		/// <param name="startingOrder">The starting order.</param>
		/// <param name="keySelector">The key selector.</param>
		/// <returns/>
		public int AddRangeWithDeclarationOrder(IEnumerable<T> itemsToAdd, int startingOrder, Func<T, string> keySelector)
		{
			keySelector.ThrowOnNull(nameof(keySelector));
			itemsToAdd.ThrowOnNull(nameof(itemsToAdd));

			int order = startingOrder;

			foreach (T item in itemsToAdd)
			{
				string key = keySelector(item);
				Add(key, item, order);
				order++;
			}

			return order;
		}

		public void Add(string key, T value, int declarationOrder)
		{
			if (TryGetValue(key, out OverridableItem<T> existingValue))
			{
				if (!existingValue.Item.Equals(value))
				{
					base[key] = new OverridableItem<T>(value, declarationOrder, existingValue);
				}
			}
			else
			{
				Add(key, new OverridableItem<T>(value, declarationOrder));
			}
		}
	}
}
