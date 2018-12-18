using Acuminator.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionFile
	{
		private const string SuppressMessageElement = "suppressMessage";
		private const string IdAttribute = "id";
		private const string TargetElement = "target";
		private const string SyntaxNodeElement = "syntaxNode";
		private const string SuppressionFileExtension = ".acuminator";
		private static readonly char[] TrimCharacters = { ' ', '\t', '\n' };

		internal string AssemblyName { get; }

		internal string Path { get; }

		internal bool GenerateSuppressionBase { get; }

		private HashSet<SuppressMessage> Messages { get; }

		private SuppressionFile(string assemblyName, bool generateSuppressionBase, HashSet<SuppressMessage> messages)
		{
			AssemblyName = assemblyName;
			GenerateSuppressionBase = generateSuppressionBase;
			Messages = messages;
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
			(string path, bool generateSuppressionBase) suppressionFile)
		{
			suppressionFile.path.ThrowOnNull(nameof(suppressionFile.path));

			string assemblyName = fileSystemService.GetFileName(suppressionFile.path);

			if (string.IsNullOrEmpty(assemblyName))
			{
				throw new FormatException("Acuminator suppression file name cannot be empty");
			}

			var messages = new HashSet<SuppressMessage>();

			if (!suppressionFile.generateSuppressionBase)
			{
				messages = LoadMessages(fileSystemService, suppressionFile.path);
			}

			return new SuppressionFile(assemblyName, suppressionFile.generateSuppressionBase, messages);
		}

		internal void AddMessage(SuppressMessage message)
		{
			Messages.Add(message);
		}

		public XDocument MessagesToDocument()
		{
			var document = XDocument.Load(Path);
			var root = document.Root;

			root.RemoveNodes();

			foreach (var message in Messages)
			{
				root.Add(ElementFromMessage(message));
			}

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
			var target = targetElement.Value?.Trim(TrimCharacters);
			var syntaxNode = element.Element(SyntaxNodeElement).Value?.Trim(TrimCharacters);

			return new SuppressMessage(id, target, syntaxNode);
		}

		private static HashSet<SuppressMessage> LoadMessages(ISuppressionFileSystemService fileSystemService, string path)
		{
			var messages = new HashSet<SuppressMessage>();
			var document = fileSystemService.Load(path);
			var messageElements = document.Root.Elements(SuppressMessageElement);

			foreach (var element in messageElements)
			{
				messages.Add(MessageFromElement(element));
			}

			return messages;
		}
	}
}
