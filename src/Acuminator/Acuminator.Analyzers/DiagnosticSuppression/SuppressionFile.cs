using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Acuminator.Utilities.Common;
using System.Xml.Linq;

namespace Acuminator.Analyzers.DiagnosticSuppression
{
	internal class SuppressionFile
	{
		private const string GenerateSuppressionBaseAttribute = "generateSuppressionBase";
		private const string SuppressMessageElement = "suppressMessage";
		private const string IdAttribute = "id";
		private const string TargetElement = "target";
		private const string SyntaxNodeElement = "syntaxNode";
		private const string SuppressionFileExtension = ".acuminator";
		private static readonly char[] TrimCharacters = { ' ', '\t', '\n' };

		public string AssemblyName { get; }

		public bool GenerateSuppressionBase { get; }

		public HashSet<SuppressMessage> Messages { get; }

		private SuppressionFile(string assemblyName, bool generateSuppressionBase, HashSet<SuppressMessage> messages)
		{
			AssemblyName = assemblyName;
			GenerateSuppressionBase = generateSuppressionBase;
			Messages = messages;
		}

		public static bool IsSuppressionFile(string path)
		{
			return SuppressionFileExtension.Equals(Path.GetExtension(path), StringComparison.Ordinal);
		}

		public static SuppressionFile Load(string path)
		{
			path.ThrowOnNull(nameof(path));

			string assemblyName = Path.GetFileNameWithoutExtension(path);

			if (string.IsNullOrEmpty(assemblyName))
			{
				throw new FormatException("Acuminator suppression file name cannot be empty");
			}

			var document = XDocument.Load(path);
			var generateSuppression = LoadGenerateSuppression(document);
			var messages = LoadMessages(document);

			return new SuppressionFile(assemblyName, generateSuppression, messages);
		}

		private static bool LoadGenerateSuppression(XDocument document)
		{
			var generateSuppressionString = document.Root.Attribute(GenerateSuppressionBaseAttribute)?.Value;

			if (bool.TryParse(generateSuppressionString, out bool generateSuppressionValue))
			{
				return generateSuppressionValue;
			}

			return false;
		}

		public static void AddMessage(SuppressMessage message)
		{

		}

		private static SuppressMessage ParseMessage(XElement messageElement)
		{
			var id = messageElement.Attribute(IdAttribute).Value;
			var targetElement = messageElement.Element(TargetElement);
			var target = targetElement.Value?.Trim(TrimCharacters);
			var syntaxNode = messageElement.Element(SyntaxNodeElement).Value?.Trim(TrimCharacters);

			return new SuppressMessage(id, target, syntaxNode);
		}

		private static HashSet<SuppressMessage> LoadMessages(XDocument document)
		{
			var messages = new HashSet<SuppressMessage>();
			var messageElements = document.Root.Elements(SuppressMessageElement);

			foreach (var element in messageElements)
			{
				messages.Add(ParseMessage(element));
			}

			return messages;
		}
	}
}
