using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Serilog;

namespace Acuminator.Runner.Utilities
{
	internal class ConsoleCancellationSubscription : IDisposable
	{
		private bool _isDisposed;
		private readonly CancellationTokenSource? _cancellationTokenSource;
		private readonly ILogger? _logger;
		private bool _oldTreatControlCAsInput;

		public CancellationToken CancellationToken =>
			_isDisposed
				? throw new ObjectDisposedException(nameof(ConsoleCancellationSubscription))
				: _cancellationTokenSource?.Token ?? CancellationToken.None;

		public bool IsCancellationRequested => CancellationToken.IsCancellationRequested;

		public ConsoleCancellationSubscription(ILogger? logger)
		{
			_logger = logger;
			_logger?.Debug("Subscribing on Console cancellation events.");

			try
			{
				_oldTreatControlCAsInput = Console.TreatControlCAsInput;
				Console.TreatControlCAsInput = false;
				Console.CancelKeyPress += Console_CancelKeyPress;
			}
			catch (Exception subscribeToCancellationException)
			{
				_logger?.Warning(subscribeToCancellationException, "Failed to subscribe to the interactive console cancellation.");
				return;
			}

			// Initialize cancellation token source only after the successful subscription
			_cancellationTokenSource = new CancellationTokenSource();
		}

		private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			if (_cancellationTokenSource == null || _isDisposed)
				return;

			string keyCombination = e.SpecialKey == ConsoleSpecialKey.ControlC
				? "Ctrl + C"
				: "Ctrl + Break";

			_logger?.Warning("Cancelling the validation because {KeyCombination} was pressed.", keyCombination);
			
			_cancellationTokenSource.Cancel();
			e.Cancel = true;
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			try
			{
				_logger?.Debug("Disposing the console cancellation subscription.");
				Console.CancelKeyPress -= Console_CancelKeyPress;
				Console.TreatControlCAsInput = _oldTreatControlCAsInput;

				_cancellationTokenSource?.Dispose();
				_isDisposed = true;
			}
			catch (Exception e)
			{
				_logger?.Error(e, $"An error happened during the disposal of the console cancellation subscription.");
			}
		}
	}
}
