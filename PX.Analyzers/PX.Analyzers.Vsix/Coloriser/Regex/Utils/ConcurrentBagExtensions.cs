using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Analyzers.Coloriser
{
    public static class ConcurrentBagExtensions
    {
        public static void Clear<T>(this ConcurrentBag<T> bag)
        {
            if (bag == null)
                return;

            T someItem;

            while (!bag.IsEmpty)
            {
                bag.TryTake(out someItem);
            }
        }
    }
}
