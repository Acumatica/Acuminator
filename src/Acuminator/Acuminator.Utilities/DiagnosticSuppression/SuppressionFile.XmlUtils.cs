using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.IO;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public partial class SuppressionFile : IDisposable
	{
		private const string RootEmelent = "suppressions";
		public const string SuppressMessageElement = "suppressMessage";

		private static class XmlUtils
		{
			public static XDocument LoadSuppressionFileAndReplaceItsContent(ISuppressionFileSystemService fileSystemService,
																			string filePath, IEnumerable<SuppressMessage> newMessages)
			{
				XDocument? document;

				lock (fileSystemService)
				{
					document = fileSystemService.Load(filePath);
				}

				if (document == null)
					throw new InvalidOperationException("Failed to open suppression file for edit");

				document.Root.RemoveNodes();
				AddMessagesToDocument(document, newMessages);

				return document;
			}

			public static XDocument NewDocumentFromMessages(IEnumerable<SuppressMessage> messages)
			{
				var root = new XElement(RootEmelent);
				var document = new XDocument(root);

				AddMessagesToDocument(document, messages);

				return document;
			}

			public static void AddMessagesToDocument(XDocument document, IEnumerable<SuppressMessage> messages)
			{
				var comparer = new SuppressionMessageComparer();
				var sortedMessages = messages.OrderBy(m => m, comparer);

				foreach (var message in sortedMessages)
				{
					var xmlMessage = message.ToXml();

					if (xmlMessage != null)
						document.Root.Add(xmlMessage);
				}
			}

			public static HashSet<SuppressMessage> LoadMessages(ISuppressionFileSystemService fileSystemService, string path)
			{
				XDocument? document;

				lock (fileSystemService)
				{
					document = fileSystemService.Load(path);
				}

				if (document == null)
				{
					return [];
				}

				return LoadMessagesFromDocument(document);
			}

			public static ImmutableHashSet<SuppressMessage> LoadMessagesFromString(string suppressionFileContent)
			{
				XDocument document;

				try
				{
					document = XDocument.Parse(suppressionFileContent);
				}
				catch (Exception e) when (e is System.Xml.XmlException or ArgumentNullException)
				{
					return [];
				}

				return LoadMessagesFromDocument(document).ToImmutableHashSet();
			}

			private static HashSet<SuppressMessage> LoadMessagesFromDocument(XDocument document)
			{
				var suppressionMessages = new HashSet<SuppressMessage>();

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
}
