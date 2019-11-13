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
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

        public Task TaggingTask { get; private set; }

        private bool _isDisposed;

        private BackgroundTagging()
        {
            
        }

        public static BackgroundTagging StartBackgroundTagging(PXColorizerTaggerBase tagger)
        {
            tagger.ThrowOnNull(nameof(tagger));

            BackgroundTagging backgroundTagging = new BackgroundTagging();
			backgroundTagging.TaggingTask = tagger.GetTagsAsyncImplementationAsync(tagger.Snapshot, backgroundTagging.CancellationToken)
												 ?.ContinueWith(task => Shell.ThreadHelper.JoinableTaskFactory.Run(tagger.RaiseTagsChangedAsync),
																		backgroundTagging.CancellationToken,
																		TaskContinuationOptions.OnlyOnRanToCompletion,
																		TaskScheduler.FromCurrentSynchronizationContext());
            return backgroundTagging;
        }


        public void CancelTagging()
        {
            if (!IsTaskRunning() || IsCancellationRequested)
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
    }
}
