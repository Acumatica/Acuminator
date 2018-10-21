using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
    public class GraphOverridableItemsCollection<T> : Dictionary<string, GraphOverridableItem<T>>
    {
        public IEnumerable<GraphOverridableItem<T>> Items => Values;

        public GraphOverridableItemsCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void Add(string key, T value)
        {
            if (TryGetValue(key, out GraphOverridableItem<T> existingValue))
            {
                if (!existingValue.Item.Equals(value))
                {
                    base[key] = new GraphOverridableItem<T>(value, existingValue);
                }
            }
            else
            {
                Add(key, new GraphOverridableItem<T>(value));
            }
        }
    }
}
