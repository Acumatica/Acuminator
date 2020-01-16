using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionFile : IDisposable
	{
        private const string RootEmelent = "suppressions";
		public const string SuppressMessageElement = "suppressMessage";
		public const string SuppressionFileExtension = ".acuminator";

		internal string AssemblyName { get; }

		internal string Path { get; }

		private readonly ISuppressionFileWatcherService _fileWatcher;

		/// <summary>
		/// Indicates whether to generate errors suppression base to suppression file or not
		/// </summary>
		internal bool GenerateSuppressionBase { get; }

		private HashSet<SuppressMessage> Messages { get; }

		public HashSet<SuppressMessage> CopyMessages() => new HashSet<SuppressMessage>(Messages);

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

		private SuppressionFile(string assemblyName, string path, bool generateSuppressionBase,
								HashSet<SuppressMessage> messages, ISuppressionFileWatcherService watcher)
		{
			AssemblyName = assemblyName;
			Path = path;
			GenerateSuppressionBase = generateSuppressionBase;
			Messages = messages;
			_fileWatcher = watcher;
		}

		internal bool ContainsMessage(SuppressMessage message) => Messages.Contains(message);

		internal static bool IsSuppressionFile(string path)
		{
			return SuppressionFileExtension.Equals(System.IO.Path.GetExtension(path), StringComparison.Ordinal);
		}

		internal static SuppressionFile Load(ISuppressionFileSystemService fileSystemService, string suppressionFilePath, 
											 bool generateSuppressionBase)
		{
			fileSystemService.ThrowOnNull(nameof(fileSystemService));
			suppressionFilePath.ThrowOnNullOrWhiteSpace(nameof(suppressionFilePath));

			string assemblyName = fileSystemService.GetFileName(suppressionFilePath);
			if (string.IsNullOrEmpty(assemblyName))
			{
				throw new FormatException("Acuminator suppression file name cannot be empty");
			}

			var messages = new HashSet<SuppressMessage>();

			if (!generateSuppressionBase)
			{
				messages = LoadMessages(fileSystemService, suppressionFilePath);
			}

			ISuppressionFileWatcherService fileWatcher;

			lock (fileSystemService)
			{
				fileWatcher = fileSystemService.CreateWatcher(suppressionFilePath);
			}
			
			return new SuppressionFile(assemblyName, suppressionFilePath, generateSuppressionBase, messages, fileWatcher);
		}

		public void Dispose() => _fileWatcher?.Dispose();
		
		internal void AddMessage(SuppressMessage message) => Messages.Add(message);

        public static XDocument NewDocumentFromMessages(IEnumerable<SuppressMessage> messages)
        {
            var root = new XElement(RootEmelent);
            var document = new XDocument(root);

            AddMessagesToDocument(document, messages);

            return document;
        }

		internal XDocument MessagesToDocument(ISuppressionFileSystemService fileSystemService)
		{
			fileSystemService.ThrowOnNull(nameof(fileSystemService));
			XDocument document;

			lock (fileSystemService)
			{
				document = fileSystemService.Load(Path);
			}

			if (document == null)
				throw new InvalidOperationException("Failed to open suppression file for edit");

			document.Root.RemoveNodes();
			AddMessagesToDocument(document, Messages);

			return document;
		}

		private static void AddMessagesToDocument(XDocument document, IEnumerable<SuppressMessage> messages)
		{
			var comparer = new SuppressionMessageComparer();
			var sortedMessages = messages.OrderBy(m => m, comparer);

			foreach (var message in sortedMessages)
			{
				var xmlMessage = message.ElementFromMessage();

				if (xmlMessage != null)
					document.Root.Add(xmlMessage);
			}
		}

		public static HashSet<SuppressMessage> LoadMessages(ISuppressionFileSystemService fileSystemService, string path)
		{
			XDocument document;

			lock (fileSystemService)
			{
				document = fileSystemService.Load(path);
			}		

			if (document == null)
			{
				return new HashSet<SuppressMessage>();
			}

			HashSet<SuppressMessage> suppressionMessages = new HashSet<SuppressMessage>();

			foreach (XElement suppressionMessageXml in document.Root.Elements(SuppressMessageElement))
			{
				SuppressMessage? suppressMessage = SuppressMessage.MessageFromElement(suppressionMessageXml);

				if (suppressMessage != null)
				{
					suppressionMessages.Add(suppressMessage.Value);
				}
			}

			return suppressionMessages;
		}
	}
}
