using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public class OverridableItemsCollection<TInfo> : Dictionary<string, TInfo>
	where TInfo : IOverridableItem<TInfo>
	{
		public IEnumerable<TInfo> Items => Values;

		public OverridableItemsCollection() : base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public OverridableItemsCollection(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
		{
		}

		/// <summary>
		/// Adds a range of DTO info items from the raw datas maintaining the declaration order. Returns the number following the last assigned declaration order.
		/// </summary>
		/// <typeparam name="TRawData">Type of the raw data.</typeparam>
		/// <param name="itemsToAdd">The items to add.</param>
		/// <param name="startingOrder">The starting order.</param>
		/// <param name="infoConstructor">The DTO info constructor.</param>
		/// <returns/>
		public int AddRangeWithDeclarationOrder<TRawData>(IEnumerable<TRawData> itemsToAdd, int startingOrder, Func<TRawData, int, TInfo> infoConstructor)
		{
			itemsToAdd.ThrowOnNull(nameof(itemsToAdd));

			int order = startingOrder;

			foreach (TRawData rawData in itemsToAdd)
			{
				Add(rawData, order, infoConstructor);
				order++;
			}

			return order;
		}

		public void Add<TRawData>(TRawData rawData, int declarationOrder, Func<TRawData, int, TInfo> infoConstructor)
		{
			infoConstructor.ThrowOnNull(nameof(infoConstructor));
			TInfo info = infoConstructor(rawData, declarationOrder);
			Add(info);
		}

		public void Add(TInfo info)
		{
			if (info?.Name == null)
				return;

			if (TryGetValue(info.Name, out TInfo existingValue))
			{
				if (!existingValue.Equals(info) && info is IWriteableBaseItem<TInfo> infoWithWriteableBase)
				{
					infoWithWriteableBase.Base = existingValue;
					base[info.Name] = info;
				}
			}
			else
			{
				Add(info.Name, info);
			}
		}
	}
}
