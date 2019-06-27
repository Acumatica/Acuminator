using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// A DTO wchich stores info about some item. The item which can be overridable, and the info about base item is also stored.
	/// </summary>
	/// <typeparam name="T">Generic type parameter.</typeparam>
	public class OverridableItem<T>
	{
		public OverridableItem<T> Base { get; }

		public T Item { get; }

		public int DeclarationOrder { get; }

		public OverridableItem(T item, int declarationOrder, OverridableItem<T> baseItem = null)
		{
			item.ThrowOnNull(nameof(item));

			Item = item;
			Base = baseItem;
			DeclarationOrder = declarationOrder;
		}
	}
}
