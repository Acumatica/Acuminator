using System;
using System.Collections.Generic;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// A helper class for <see cref="IOverridableItem{T}"/>.
	/// </summary>
	public static class OverridableItemUtils
	{
		public static IEnumerable<TInfo> ThisAndOverridenItems<TInfo>(this TInfo info)
		where TInfo : IOverridableItem<TInfo>
		{
			info.ThrowOnNull(nameof(info));
			return GetOverridenItems(info, includeOriginalItem: true);
		}

		public static IEnumerable<TInfo> JustOverridenItems<TInfo>(this TInfo info)
		where TInfo : IOverridableItem<TInfo>
		{
			info.ThrowOnNull(nameof(info));
			return GetOverridenItems(info, includeOriginalItem: false);
		}

		private static IEnumerable<TInfo> GetOverridenItems<TInfo>(TInfo info, bool includeOriginalItem)
		where TInfo : IOverridableItem<TInfo>
		{
			TInfo current = includeOriginalItem
				? info
				: info.Base;

			while (current != null)
			{
				yield return current;
				current = current.Base;
			}
		}
	}
}
