using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PX.Analyzers.Vsix.Utilities
{
    public static class ConcurrentCollectionsExtensions
    {
        public static void Clear<T>(this ConcurrentBag<T> bag)
        {
            if (bag == null)
                return;
        
            while (!bag.IsEmpty)
            {
                bag.TryTake(out _);
            }
        }

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            if (queue == null)
                return;

            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out _);
            }
        }
    }
}
