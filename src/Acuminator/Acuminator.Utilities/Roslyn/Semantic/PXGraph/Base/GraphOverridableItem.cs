using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public class GraphOverridableItem<T>
	{
		public GraphOverridableItem<T> Base { get; }

		public T Item { get; }

		public int DeclarationOrder { get; }

		public GraphOverridableItem(T item, int declarationOrder, GraphOverridableItem<T> baseItem = null)
		{
			item.ThrowOnNull(nameof(item));

			Item = item;
			Base = baseItem;
			DeclarationOrder = declarationOrder;
		}
	}
}
