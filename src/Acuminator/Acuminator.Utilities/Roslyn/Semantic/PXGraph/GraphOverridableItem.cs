using PX.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class GraphOverridableItem<T>
    {
        public GraphOverridableItem<T> Base { get; }

        public T Item { get; }

        public GraphOverridableItem(T item, GraphOverridableItem<T> baseItem = null)
        {
            item.ThrowOnNull();

            Item = item;
            Base = baseItem;
        }
    }
}
