using Acuminator.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionFile
	{
        private const string RootEmelent = "suppressions";
		private const string SuppressMessageElement = "suppressMessage";
		private const string IdAttribute = "id";
		private const string TargetElement = "target";
		private const string SyntaxNodeElement = "syntaxNode";
		private const string SuppressionFileExtension = ".acuminator";

		internal string AssemblyName { get; }

		internal string Path { get; }

		/// <summary>
		/// Indicates whether to generate errors suppression base to suppression file or not
		/// </summary>
		internal bool GenerateSuppressionBase { get; }

		private HashSet<SuppressMessage> Messages { get; }

		public HashSet<SuppressMessage> CopyMessages() => new HashSet<SuppressMessage>(Messages);

		public event Action<object, SuppressionFileEventArgs> Changed;

		private SuppressionFile(string assemblyName, string path, bool generateSuppressionBase,
			HashSet<SuppressMessage> messages, ISuppressionFileWatcherService watcher)
		{
			AssemblyName = assemblyName;
			Path = path;
			GenerateSuppressionBase = generateSuppressionBase;
			Messages = messages;

			if (watcher != null)
			{
				watcher.Changed += OnChanged;
			}
		}

		private void OnChanged(object sender, SuppressionFileEventArgs e)
		{
			Changed?.Invoke(sender, e);
		}

		internal bool ContainsMessage(SuppressMessage message)
		{
			return Messages.Contains(message);
		}

		internal static bool IsSuppressionFile(string path)
		{
			return SuppressionFileExtension.Equals(System.IO.Path.GetExtension(path), StringComparison.Ordinal);
		}

		internal static SuppressionFile Load(ISuppressionFileSystemService fileSystemService,
			(string Path, bool GenerateSuppressionBase) loadInfo)
		{
			fileSystemService.ThrowOnNull(nameof(fileSystemService));
			loadInfo.Path.ThrowOnNull(nameof(loadInfo.Path));

			string assemblyName = fileSystemService.GetFileName(loadInfo.Path);
			if (string.IsNullOrEmpty(assemblyName))
			{
				throw new FormatException("Acuminator suppression file name cannot be empty");
			}

			var messages = new HashSet<SuppressMessage>();

			if (!loadInfo.GenerateSuppressionBase)
			{
				messages = LoadMessages(fileSystemService, loadInfo.Path);
			}

			var fileWatcher = fileSystemService.CreateWatcher(loadInfo.Path);

			return new SuppressionFile(assemblyName, loadInfo.Path, loadInfo.GenerateSuppressionBase, messages, fileWatcher);
		}

		internal void AddMessage(SuppressMessage message)
		{
			Messages.Add(message);
		}

        private static void AddMessagesToDocument(XDocument document, IEnumerable<SuppressMessage> messages)
        {
            var sortedMessages = messages.Order();

            foreach (var message in sortedMessages)
            {
                document.Root.Add(ElementFromMessage(message));
            }
        }

        public static XDocument MessagesToDocument(IEnumerable<SuppressMessage> messages)
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

		private static SuppressMessage MessageFromElement(XElement element)
		{
			var id = element.Attribute(IdAttribute).Value;
			var targetElement = element.Element(TargetElement);
			var target = targetElement.Value;
			var syntaxNode = element.Element(SyntaxNodeElement).Value;

			return new SuppressMessage(id, target, syntaxNode);
		}

		public static HashSet<SuppressMessage> LoadMessages(ISuppressionFileSystemService fileSystemService, string path)
		{
			var document = fileSystemService.Load(path);

			if (document == null)
			{
				return new HashSet<SuppressMessage>();
			}

			return document.Root.Elements(SuppressMessageElement)
				.Select(e => MessageFromElement(e))
				.ToHashSet();
		}
	}
}
