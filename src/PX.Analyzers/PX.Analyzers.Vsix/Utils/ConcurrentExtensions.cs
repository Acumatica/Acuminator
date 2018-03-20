using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PX.Analyzers.Vsix.Utilities
{
    public static class ConcurrentExtensions
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

        /// <summary>
        /// A Task extension method that attempts to await task which could be cancelled.
        /// </summary>
        /// <param name="task">The task to act on.</param>
        /// <returns/>      
        public async static Task<bool> TryAwait(this Task task)
        {
            if (task == null || task.IsCanceled || task.IsFaulted)
                return false;

            try
            {
                await task.ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// A <see cref="Task{TResult}"/> extension method that attempts to await task which could be cancelled.
        /// </summary>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="task">The task to act on.</param>
        /// <returns/>      
        public async static Task<KeyValuePair<bool, TResult>> TryAwait<TResult>(this Task<TResult> task)
        {
            if (task == null || task.IsCanceled || task.IsFaulted)
                return new KeyValuePair<bool, TResult>(false, default);

            try
            {
                TResult result = await task.ConfigureAwait(false);
                return new KeyValuePair<bool, TResult>(true, result); 
            }
            catch (OperationCanceledException)
            {
                return new KeyValuePair<bool, TResult>(false, default);
            }
        }
    }
}
