using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;

using Shell = Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix.Coloriser
{
    public class BackgroundTagging : IDisposable
    {
        private static TaskScheduler _vsTaskScheduler;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public Task TaggingTask { get; private set; }

        private bool _isDisposed;

        private BackgroundTagging()
        {
            
        }

        public static BackgroundTagging StartBackgroundTagging(PXColorizerTaggerBase tagger)
        {
            tagger.ThrowOnNull(nameof(tagger));

            BackgroundTagging backgroundTagging = new BackgroundTagging();
            var taggingTask = tagger.GetTagsAsyncImplementationAsync(tagger.Snapshot, backgroundTagging.CancellationToken);

            if (taggingTask == null)
                return backgroundTagging;

            // Use VS task scheduler from the VS synchronization context to schedule task continuation immediately to main thread
            // No need for synchronziation because FromCurrentSynchronizationContext creates schedulers which wrap around the same synchronization context
            // Therefore all schedulers should be identical and nothing wrong will happen if multiple threads create will create a scheduler
            _vsTaskScheduler = _vsTaskScheduler ?? TaskScheduler.FromCurrentSynchronizationContext();
            backgroundTagging.TaggingTask = taggingTask.ContinueWith(task => AfterTaggingActionAsync(tagger, backgroundTagging.CancellationToken),  //continuation should be on the UI thread
                                                                     backgroundTagging.CancellationToken,
                                                                     TaskContinuationOptions.OnlyOnRanToCompletion,
                                                                     _vsTaskScheduler);
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
            CancelTagging();
            _cancellationTokenSource.Dispose();
        }

        private static Task AfterTaggingActionAsync(PXColorizerTaggerBase tagger, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            // We should be on UI thread here but the tagger.RaiseTagsChangedAsync switches to UI thread from non UI threads internally if needed         
            return Shell.ThreadHelper.JoinableTaskFactory.RunAsync(tagger.RaiseTagsChangedAsync).Task;
		}
    }
}
