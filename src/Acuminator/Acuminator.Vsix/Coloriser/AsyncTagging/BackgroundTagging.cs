#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Logger;

using Shell = Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix.Coloriser
{
	internal class BackgroundTagging : IDisposable
	{
		private static TaskScheduler? _vsTaskScheduler; 
		private CancellationTokenSource _cancellationTokenSource = new();

		public CancellationToken CancellationToken => _cancellationTokenSource.Token;

		public Task? TaggingTask { get; private set; }

		private bool _isDisposed;

		private BackgroundTagging()
		{
		}

		public static BackgroundTagging StartBackgroundTagging(PXRoslynColorizerTagger tagger)
		{
			tagger.ThrowOnNull();

			var backgroundTagging = new BackgroundTagging();

			if (tagger.Snapshot is null)
				return backgroundTagging;

			var taggingTask = tagger.GetTagsAsyncImplementationAsync(tagger.Snapshot, backgroundTagging.CancellationToken);

			if (taggingTask == null)
				return backgroundTagging;

			// Use VS task scheduler from the VS synchronization context to schedule task continuation immediately to main thread
			// No need for synchronization because FromCurrentSynchronizationContext creates schedulers which wrap around the same synchronization context
			// Therefore all schedulers should be identical and nothing wrong will happen if different thread will create multiple instance of the scheduler in a race condition
			_vsTaskScheduler = _vsTaskScheduler ?? TaskScheduler.FromCurrentSynchronizationContext();
			var continuationTask = taggingTask.ContinueWith(task => AfterTaggingActionAsync(task, tagger, backgroundTagging.CancellationToken),  //continuation should be on the UI thread
																	 backgroundTagging.CancellationToken,
																	 TaskContinuationOptions.NotOnCanceled,
																	 _vsTaskScheduler);

			// ContinueWith schedules the lambda on the VS UI thread scheduler. The lambda runs on the UI thread and calls AfterTaggingActionAsync(...).
			// Inside AfterTaggingActionAsync, the important path calls ThreadHelper.JoinableTaskFactory.RunAsync(tagger.RaiseTagsChangedAsync).Task 
			// this starts RaiseTagsChangedAsync and immediately returns the underlying Task representing it (still running).
			// The lambda returns that inner Task immediately — it does not await it.
			// The outer Task<Task> stored in TaggingTask is marked as Completed (RanToCompletion) at this point, because the lambda has returned. 
			// The outer task's result is the still-running inner task, but the outer task itself is done.
			// RaiseTagsChangedAsync may still be running in the background raising tags-changed notifications. 
			// 
			// Thus, we need to keep the nested unwrapped task as the tagging task to be able to correctly calculate IsTaskRunning() and 
			// handle exceptions thrown in the AfterTaggingActionAsync.
			backgroundTagging.TaggingTask = continuationTask.Unwrap();
			return backgroundTagging;
		}


		public void CancelTagging()
		{
			if (!IsTaskRunning() || CancellationToken.IsCancellationRequested)
				return;

			_cancellationTokenSource.Cancel();
		}

		public bool IsTaskRunning() => TaggingTask != null && !TaggingTask.IsCanceled && !TaggingTask.IsCompleted && !TaggingTask.IsFaulted;

		public void Dispose()
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			try
			{
				_cancellationTokenSource.Cancel();
			}
			catch (OperationCanceledException)
			{
			}
			catch (AggregateException aggregateException)
			{
				var flattened = aggregateException.Flatten();
				var exceptionsToLog = flattened.InnerExceptions.Where(ex => ex is not OperationCanceledException);

				foreach (var exception in exceptionsToLog)
				{
					AcuminatorVSPackage.Instance?.AcuminatorLogger?.LogException(exception, logOnlyFromAcuminatorAssemblies: false, LogMode.Warning);
				}
			}
			catch (Exception ex)
			{
				AcuminatorVSPackage.Instance?.AcuminatorLogger?.LogException(ex, logOnlyFromAcuminatorAssemblies: false, LogMode.Warning);
			}
			finally
			{
				_cancellationTokenSource.Dispose();
			}
		}

		private static Task AfterTaggingActionAsync(Task taggingTask, PXRoslynColorizerTagger tagger, CancellationToken cancellationToken)
		{
			if (taggingTask.IsCanceled || cancellationToken.IsCancellationRequested)
			{
				tagger.LastTaggingWasSuccessful = false;
				return Task.FromCanceled(cancellationToken);
			}
			
			if (taggingTask.IsFaulted)
			{
				tagger.LastTaggingWasSuccessful = false;
				return Task.FromException(taggingTask.Exception!);
			}

			// We should be on UI thread here but the tagger.RaiseTagsChangedAsync switches to UI thread from non UI threads internally if needed
			return AcuminatorVSPackage.JTF.RunAsync(async () => await tagger.RaiseTagsChangedAsync(cancellationToken)).Task;
		}
	}
}
