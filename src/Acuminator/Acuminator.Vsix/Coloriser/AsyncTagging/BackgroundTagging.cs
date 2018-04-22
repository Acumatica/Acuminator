using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Tagging;
using System.Collections;
using Acuminator.Utilities;


namespace Acuminator.Vsix.Coloriser
{
    public class BackgroundTagging : IDisposable
    {
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => cancellationTokenSource.Token;

        public bool IsCancellationRequested => cancellationTokenSource.IsCancellationRequested;

        public Task TaggingTask { get; private set; }

        private bool isDisposed;

        private BackgroundTagging()
        {
            
        }

        public static BackgroundTagging StartBackgroundTagging(PXColorizerTaggerBase tagger)
        {
            tagger.ThrowOnNull(nameof(tagger));

            BackgroundTagging backgroundTagging = new BackgroundTagging();
            backgroundTagging.TaggingTask = tagger.GetTagsAsyncImplementation(tagger.Snapshot, backgroundTagging.CancellationToken);
            backgroundTagging.TaggingTask?.ConfigureAwait(false);
            backgroundTagging.TaggingTask.ContinueWith(task => tagger.RaiseTagsChanged(), 
                                                       backgroundTagging.CancellationToken, 
                                                       TaskContinuationOptions.OnlyOnRanToCompletion, 
                                                       TaskScheduler.Default)
                                         .ConfigureAwait(false);
            
            return backgroundTagging;
        }


        public void CancelTagging()
        {
            if (!IsTaskRunning() || IsCancellationRequested)
                return;

            cancellationTokenSource.Cancel();
        }

        public bool IsTaskRunning() => TaggingTask != null && !TaggingTask.IsCanceled && !TaggingTask.IsCompleted && !TaggingTask.IsFaulted;

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            CancelTagging();
            cancellationTokenSource.Dispose();
        }
    }
}
