#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Common
{
	public static class ConcurrentExtensions
	{
		public static void Clear<T>(this ConcurrentBag<T>? bag)
		{
			if (bag == null)
				return;

			while (!bag.IsEmpty)
			{
				bag.TryTake(out _);
			}
		}

		public static void Clear<T>(this ConcurrentQueue<T>? queue)
		{
			if (queue == null)
				return;

			while (!queue.IsEmpty)
			{
				queue.TryDequeue(out _);
			}
		}

		/// <summary>
		/// A Task extension method that attempts to await task which could be cancelled or faulted.
		/// </summary>
		/// <param name="task">The task to act on.</param>
		/// <param name="continueOnCapturedContext">(Optional) True to continue on captured context.</param>
		/// <returns/>      
		public async static Task<bool> TryAwait(this Task? task, bool continueOnCapturedContext = false)
		{
			if (task == null || task.IsCanceled || task.IsFaulted)
				return false;

			try
			{
				await task.ConfigureAwait(continueOnCapturedContext);
				return true;
			}
			catch (Exception exception)
			{
				return false;
			}
		}

		/// <summary>
		/// A <see cref="ValueTask"/> extension method that attempts to await task which could be cancelled or faulted.
		/// </summary>
		/// <param name="task">The task to act on.</param>
		/// <param name="continueOnCapturedContext">(Optional) True to continue on captured context.</param>
		/// <returns/>
		public async static ValueTask<bool> TryAwait(this ValueTask task, bool continueOnCapturedContext = false)
		{
			if (task.IsCanceled || task.IsFaulted)
				return false;

			try
			{
				await task.ConfigureAwait(continueOnCapturedContext);
				return true;
			}
			catch (Exception exception)
			{
				return false;
			}
		}

		/// <summary>
		/// A <see cref="Task{TResult}"/> extension method that attempts to await task which could be cancelled or faulted.
		/// </summary>
		/// <typeparam name="TResult">Type of the result.</typeparam>
		/// <param name="task">The task to act on.</param>
		/// <param name="continueOnCapturedContext">(Optional) True to continue on captured context.</param>
		/// <returns/>      
		public async static Task<TaskResult<TResult>> TryAwait<TResult>(this Task<TResult>? task,
																		bool continueOnCapturedContext = false)
		{
			if (task == null || task.IsCanceled || task.IsFaulted)
				return new TaskResult<TResult>(false, default);

			try
			{
				TResult? result = await task.ConfigureAwait(continueOnCapturedContext);
				return new TaskResult<TResult>(true, result);
			}
			catch (Exception exception)
			{
				return new TaskResult<TResult>(false, default);
			}
		}

		/// <summary>
		/// A <see cref="ValueTask{TResult}"/> extension method that attempts to await task which could be cancelled or faulted.
		/// </summary>
		/// <typeparam name="TResult">Type of the result.</typeparam>
		/// <param name="task">The task to act on.</param>
		/// <param name="continueOnCapturedContext">(Optional) True to continue on captured context.</param>
		/// <returns/>
		public async static ValueTask<TaskResult<TResult>> TryAwait<TResult>(this ValueTask<TResult> task,
																			 bool continueOnCapturedContext = false)
		{
			if (task.IsCanceled || task.IsFaulted)
				return new TaskResult<TResult>(false, default);

			try
			{
				TResult? result = await task.ConfigureAwait(continueOnCapturedContext);
				return new TaskResult<TResult>(true, result);
			}
			catch (Exception exception)
			{
				return new TaskResult<TResult>(false, default);
			}
		}
	}
}
