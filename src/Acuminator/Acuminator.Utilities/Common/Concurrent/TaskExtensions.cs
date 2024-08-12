#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Acuminator.Utilities.Common
{
	public static class TaskExtensions
	{
		/// <summary>
		/// Creates a new task from the <paramref name="task"/> with support for cancellation <paramref name="cancellationToken"/>.
		/// </summary>
		/// <remarks>
		/// If the cancellation is requested the underlying <paramref name="task"/> operation will continued but the returned result task will be cancelled.<br/>
		/// This helper should be used for async opartions that do not support cancellation themselves.<br/><br/>
		/// THe helpe is inspired by this article:<br/>
		/// https://devblogs.microsoft.com/pfxteam/how-do-i-cancel-non-cancelable-async-operations/
		/// </remarks>
		/// <param name="task">The task to act on.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <exception cref="ArgumentNullException">Thrown when task is null.</exception>
		/// <exception cref="OperationCanceledException">Thrown when a thread cancels a running operation.</exception>
		/// <returns>
		/// Task which will be cancelled when cancellation is requested via <paramref name="cancellationToken"/>.
		/// </returns>
		public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
		{
			task.CheckIfNull();
			return WithCancellationImpl(task, cancellationToken);

			//-------------------------------------Local Function-----------------------------------------------------------
			static async Task WithCancellationImpl(Task task, CancellationToken cancellationToken)
			{
				bool isCancelledBeforeCompletion = await IsCancelledBeforeCompletion(task, cancellationToken).ConfigureAwait(false);

				if (isCancelledBeforeCompletion)
					throw new OperationCanceledException(cancellationToken);

				//Return nothing here since if task is not cancelled it should be already completed
			}
		}

		/// <inheritdoc cref="WithCancellation(Task, CancellationToken)"/>
		/// <typeparam name="T">Task return type.</typeparam>
		public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			task.CheckIfNull();
			return WithCancellationImpl(task, cancellationToken);

			//-------------------------------------Local Function-----------------------------------------------------------
			static async Task<T> WithCancellationImpl(Task<T> task, CancellationToken cancellationToken)
			{
				bool isCancelledBeforeCompletion = await IsCancelledBeforeCompletion(task, cancellationToken).ConfigureAwait(false);

				if (isCancelledBeforeCompletion)
					throw new OperationCanceledException(cancellationToken);

				//Result is non blocking here since if task is not cancelled it should be already completed
				return task.Result;
			}
		}

		private static async Task<bool> IsCancelledBeforeCompletion(Task task, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>();

			using (var ctsRegistration = cancellationToken.Register(CancellationCallback, tcs))
			{
				var completedTaskOrObservedCancellation = await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
				return !ReferenceEquals(task, completedTaskOrObservedCancellation);
			}
		}

		private static void CancellationCallback(object state)
		{
			if (state is TaskCompletionSource<bool> tcs)
				tcs.TrySetResult(true);
		}
	}
}
