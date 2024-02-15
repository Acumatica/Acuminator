﻿using System;
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
		/// <typeparam name="TWriteableInfo">Type of the writeable information.</typeparam>
		/// <param name="itemsToAdd">The items to add.</param>
		/// <param name="startingOrder">The starting order.</param>
		/// <param name="infoConstructor">The DTO info constructor.</param>
		/// <returns/>
		internal virtual int AddRangeWithDeclarationOrder<TRawData, TWriteableInfo>(IEnumerable<TRawData> itemsToAdd, int startingOrder, 
																					Func<TRawData, int, TWriteableInfo> infoConstructor)
		where TWriteableInfo : TInfo, IWriteableBaseItem<TInfo>
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

		internal virtual void Add<TRawData, TWriteableInfo>(TRawData rawData, int declarationOrder, Func<TRawData, int, TWriteableInfo> infoConstructor)
		where TWriteableInfo : TInfo, IWriteableBaseItem<TInfo>
		{
			infoConstructor.ThrowOnNull(nameof(infoConstructor));
			TWriteableInfo info = infoConstructor(rawData, declarationOrder);
			Add(info);
		}

		internal virtual void Add<TWriteableInfo>(TWriteableInfo info)
		where TWriteableInfo : TInfo, IWriteableBaseItem<TInfo>
		{
			if (info?.Name == null)
			{
				throw new ArgumentNullException($"{nameof(info)}.{nameof(info.Name)}");
			}

			if (TryGetValue(info.Name, out TInfo existingValue))
			{
				if (!existingValue.Equals(info))
				{
					// HACK the assignment below is required due to the issue with C# compiler, see details here:
					// https://developercommunity.visualstudio.com/t/False-compiler-Error-CS0229-with-intende/10560802
					IWriteableBaseItem<TInfo> infoWithWriteableBaseItem = info;
					infoWithWriteableBaseItem.Base = existingValue;
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
