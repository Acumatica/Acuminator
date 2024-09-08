#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Logger;

namespace Acuminator.Vsix.BannedApi
{
	/// <summary>
	/// An API file setting watcher. Currently unused by may be useful in the future.
	/// </summary>
	internal class ApiFileSettingWatcher : IDisposable
	{
		internal delegate void UpdateApiFilePathAction(GeneralOptionsPage optionsPage, string? newFilePath, bool raiseEvent);

		private const int MaxRecreationAttempts = 5;

		private const int NotDisposed = 0, Disposed = 1;
		private int _isDisposed = NotDisposed;

		private int _recreationAttempts = 0;
		private FileSystemWatcher? _fileWatcher;
		private string? _filePath;

		private readonly string _watcherDescription;
		private readonly GeneralOptionsPage _optionsPage;
		private readonly UpdateApiFilePathAction? _setPathSettingOption;

		public bool IsDisposed => _isDisposed == NotDisposed;

		public ApiFileSettingWatcher(string description, GeneralOptionsPage optionsPage, UpdateApiFilePathAction? setPathSettingOption)
		{
			_watcherDescription   = description.CheckIfNullOrWhiteSpace();
			_optionsPage 		  = optionsPage.CheckIfNull();
			_setPathSettingOption = setPathSettingOption;
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _isDisposed, value: Disposed, comparand: NotDisposed) == NotDisposed)
			{
				DisposeWatcherSafely();
				_fileWatcher = null;
				_filePath = null;
			}
		}

		public void UpdateApiFileWatcher(string? filePath)
		{
			if (IsDisposed)
				throw new ObjectDisposedException($"{nameof(ApiFileSettingWatcher)} - {_watcherDescription}");

			DisposeWatcherSafely();

			_filePath = filePath.NullIfWhiteSpace()?.Trim();
			_fileWatcher = CreateFileWatcher(_filePath);
			SubscribeToWatcherEvents();
		}

		private static FileSystemWatcher? CreateFileWatcher(string? filePath)
		{
			if (filePath.IsNullOrWhiteSpace())
				return null;

			try
			{
				var directory = Path.GetDirectoryName(filePath);
				var file 	  = Path.GetFileName(filePath);
				var watcher   = new FileSystemWatcher(directory, file)
				{
					NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
					EnableRaisingEvents = true
				};

				return watcher;
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogMessage($"An error happened on attempt to create file watcher for a file \"{filePath}\".", LogMode.Error);
				AcuminatorLogger.LogException(e);
				return null;
			}
		}

		private void SubscribeToWatcherEvents()
		{
			if (_fileWatcher != null)
			{
				_fileWatcher.Deleted += FileWatcher_Deleted;
				_fileWatcher.Renamed += FileWatcher_FileRenamed;
				_fileWatcher.Error += FileWatcher_ErrorHandler;
			}
		}

		private void DisposeWatcherSafely()
		{
			try
			{
				UnsubscribeFromWatcherEvents();
				_fileWatcher?.Dispose();
			}
			catch (Exception e)
			{
				AcuminatorLogger.LogMessage($"An error happened on attempt to dispose {_watcherDescription} file watcher.", LogMode.Error);
				AcuminatorLogger.LogException(e);
			}
		}

		private void UnsubscribeFromWatcherEvents()
		{
			if (_fileWatcher != null)
			{
				_fileWatcher.Deleted -= FileWatcher_Deleted;
				_fileWatcher.Error -= FileWatcher_ErrorHandler;
			}
		}

		private void FileWatcher_ErrorHandler(object sender, ErrorEventArgs e)
		{
			AcuminatorLogger.LogException(e.GetException());

			if (_fileWatcher == null)
				return;

			_recreationAttempts++;

			if (_recreationAttempts <= MaxRecreationAttempts)
			{
				UpdateApiFileWatcher(_filePath);
			}
			else
			{
				_recreationAttempts = 0;
				UpdateApiFileWatcher(null);
				_setPathSettingOption?.Invoke(_optionsPage, null, raiseEvent: false);
			}
		}

		private void FileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (_fileWatcher == null || !string.Equals(_filePath, e.FullPath, StringComparison.OrdinalIgnoreCase)
				|| !IsChangeType(e, WatcherChangeTypes.Deleted))
			{
				return;
			}

			UpdateApiFileWatcher(null);
			_setPathSettingOption?.Invoke(_optionsPage, null, raiseEvent: false);
		}

		private void FileWatcher_FileRenamed(object sender, RenamedEventArgs e)
		{
			if (_fileWatcher == null || !string.Equals(_filePath, e.OldFullPath, StringComparison.OrdinalIgnoreCase)
				|| !IsChangeType(e, WatcherChangeTypes.Renamed))
			{
				return;
			}

			_setPathSettingOption?.Invoke(_optionsPage, e.FullPath, raiseEvent: false);
			UpdateApiFileWatcher(e.FullPath);
		}

		private static bool IsChangeType(FileSystemEventArgs e, WatcherChangeTypes changeTypeToCheck) =>
			(e.ChangeType & changeTypeToCheck) == changeTypeToCheck;
	}
}
