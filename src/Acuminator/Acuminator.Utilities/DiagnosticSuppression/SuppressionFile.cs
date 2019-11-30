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
		private const string SuppressMessageElement = "suppressMessage";
		private const string IdAttribute = "id";
		private const string TargetElement = "target";
		private const string SyntaxNodeElement = "syntaxNode";
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

		private static void AddMessagesToDocument(XDocument document, IEnumerable<SuppressMessage> messages)
        {
            var sortedMessages = messages.Where(m => m.IsValid).Order();

            foreach (var message in sortedMessages)
            {
                document.Root.Add(ElementFromMessage(message));
            }
        }

        public static XDocument NewDocumentFromMessages(IEnumerable<SuppressMessage> messages)
        {
            var root = new XElement(RootEmelent);
            var document = new XDocument(root);

            AddMessagesToDocument(document, messages);

            return document;
        }

		internal XDocument MessagesToDocument()
		{
			var document = XDocument.Load(Path);

			document.Root.RemoveNodes();
			AddMessagesToDocument(document, Messages);

			return document;
		}

		private static XElement ElementFromMessage(SuppressMessage message)
		{
			return new XElement(SuppressMessageElement,
				new XAttribute(IdAttribute, message.Id),
				new XElement(TargetElement, message.Target),
				new XElement(SyntaxNodeElement, message.SyntaxNode));
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
				SuppressMessage? suppressMessage = MessageFromElement(suppressionMessageXml);

				if (suppressMessage != null)
				{
					suppressionMessages.Add(suppressMessage.Value);
				}
			}

			return suppressionMessages;
		}

		private static SuppressMessage? MessageFromElement(XElement element)
		{
			string id = element.Attribute(IdAttribute)?.Value;
			if (id.IsNullOrWhiteSpace())
				return null;

			string target = element.Element(TargetElement)?.Value;
			if (target.IsNullOrWhiteSpace())
				return null;

			string syntaxNode = element.Element(SyntaxNodeElement)?.Value;
			if (syntaxNode.IsNullOrWhiteSpace())
				return null;

			return new SuppressMessage(id, target, syntaxNode);
		}
	}
}
