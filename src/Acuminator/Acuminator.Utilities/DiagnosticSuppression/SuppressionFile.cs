using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public partial class SuppressionFile : IDisposable
	{
		public const string SuppressionFileExtension = ".acuminator";

		internal string AssemblyName { get; }

		internal string Path { get; }

		private readonly ISuppressionFileWatcherService? _fileWatcher;

		/// <summary>
		/// Acuminator work mode for this suppression file.
		/// </summary>
		internal AcuminatorWorkMode WorkMode { get; }

		/// <summary>
		/// The suppression messages loaded from existing suppression file. These suppressions are checked against diagnostics.
		/// </summary>
		private readonly ConcurrentDictionary<SuppressMessage, object?> _loadedExistingMessages = new();

		/// <summary>
		/// The suppression messages that were added to the suppression file during the suppression file generation but not yet saved to the file system.<br/>
		/// These messages are not checked against diagnostics.
		/// </summary>
		private readonly ConcurrentDictionary<SuppressMessage, object?> _addedGeneratedMessages = new();

		public event FileSystemEventHandler Changed
		{
			add 
			{
				if (_fileWatcher != null)
				{
					_fileWatcher.Changed += value;
				}
			}
			remove
			{
				if (_fileWatcher != null)
				{
					_fileWatcher.Changed -= value;
				}
			}
		}

		internal ICollection<SuppressMessage> GetAllSuppressions()
		{
			var loadedSuppressionKeys = _loadedExistingMessages.Keys;
			var addedSuppressionKeys = _addedGeneratedMessages.Keys;

			if (addedSuppressionKeys.Count == 0)
				return loadedSuppressionKeys;
			else if (loadedSuppressionKeys.Count == 0)
				return addedSuppressionKeys;
			else
			{
				return loadedSuppressionKeys.Concat(addedSuppressionKeys)
											.ToList(loadedSuppressionKeys.Count + addedSuppressionKeys.Count);
			}
		}

		internal ICollection<SuppressMessage> GetLoadedSuppressions() => _loadedExistingMessages.Keys;

		internal ICollection<SuppressMessage> GetNewSuppressions() => _addedGeneratedMessages.Keys;

		private SuppressionFile(string assemblyName, string path, AcuminatorWorkMode workMode,
								IReadOnlyCollection<SuppressMessage> loadedMessages, ISuppressionFileWatcherService? watcher)
		{
			AssemblyName = assemblyName;
			Path = path;
			WorkMode = workMode;

			if (loadedMessages.Count > 0)
			{
				foreach (SuppressMessage message in loadedMessages)
				{
					_loadedExistingMessages.TryAdd(message, value: null);
				}
			}

			_fileWatcher = watcher;
		}

		internal bool ContainsLoadedSuppressedMessage(in SuppressMessage message) => _loadedExistingMessages.ContainsKey(message);

		internal static SuppressionFile Load(ISuppressionFileSystemService fileSystemService, string suppressionFilePath,
											 AcuminatorWorkMode workMode)
		{
			fileSystemService.ThrowOnNull();
			suppressionFilePath.ThrowOnNullOrWhiteSpace();

			string assemblyName = fileSystemService.GetFileName(suppressionFilePath).NullIfWhiteSpace() ??
								  throw new FormatException("Acuminator suppression file name cannot be empty");

			IReadOnlyCollection<SuppressMessage> loadedMessages =
				workMode.HasFlag(AcuminatorWorkMode.ReportUnsuppressedErrors)
					? LoadMessages(fileSystemService, suppressionFilePath)
					: [];
			ISuppressionFileWatcherService? fileWatcher;

			lock (fileSystemService)
			{
				fileWatcher = fileSystemService.CreateWatcher(suppressionFilePath);
			}
			
			return new SuppressionFile(assemblyName, suppressionFilePath, workMode, loadedMessages, fileWatcher);
		}

		public void Dispose() => _fileWatcher?.Dispose();

		/// <summary>
		/// Adds a generated suppression message to the suppression file.
		/// </summary>
		/// <param name="message">The suppression message.</param>
		/// <returns>
		/// True if the message was added successfully.
		/// </returns>
		internal bool AddGeneratedSuppressionMessage(in SuppressMessage message)
		{
			if (!message.IsValid || ContainsLoadedSuppressedMessage(message))
				return false;													// Do not add suppression if it is already among existing suppressions loaded from file

			return _addedGeneratedMessages.TryAdd(message, value: null);
		}

		internal XDocument ReloadSuppressionFileWithNewMessagesFromMemory(ISuppressionFileSystemService fileSystemService)
		{
			fileSystemService.ThrowOnNull();

			var newSuppressionMessages  = GetAllSuppressions();
			var documentWithNewMessages = XmlUtils.LoadSuppressionFileAndReplaceItsContent(fileSystemService, Path, newSuppressionMessages);
			return documentWithNewMessages;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static XDocument NewDocumentFromMessages(IEnumerable<SuppressMessage> messages) =>
			XmlUtils.NewDocumentFromMessages(messages);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void AddMessagesToDocument(XDocument document, IEnumerable<SuppressMessage> messages) =>
			XmlUtils.AddMessagesToDocument(document, messages);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static HashSet<SuppressMessage> LoadMessages(ISuppressionFileSystemService fileSystemService, string path) =>
			XmlUtils.LoadMessages(fileSystemService, path);
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static HashSet<SuppressMessage> LoadMessagesFromString(string suppressionFileContent) =>
			XmlUtils.LoadMessagesFromString(suppressionFileContent);
	}
}
