using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.DiagnosticSuppression
{
	public class SuppressionManager
	{
		private const string SuppressionFileExtension = ".acuminator";

		//private readonly Dictionary<string, HashSet<SuppressMessage>> _messagesByAssembly = new Dictionary<string, HashSet<SuppressMessage>>();
		//private readonly Dictionary<string, bool> _generateSuppressionBaseByAssembly = new Dictionary<string, bool>();
		private readonly Dictionary<string, SuppressionFile> _fileByAssembly = new Dictionary<string, SuppressionFile>();
		private readonly HashSet<SyntaxKind>  _targetKinds = new HashSet<SyntaxKind>(new[] {
			SyntaxKind.ClassDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.FieldDeclaration
		});

		public SuppressionManager(AnalyzerOptions options)
		{
			LoadSuppressMessages(options);
		}

		public void LoadSuppressMessages(AnalyzerOptions options)
		{
			if (options == null)
			{
				return;
			}

			foreach (var file in options.AdditionalFiles)
			{
				if (!SuppressionFile.IsSuppressionFile(file.Path))
				{
					continue;
				}

				var suppressionFile = SuppressionFile.Load(file.Path);
				var assemblyName = suppressionFile.AssemblyName;
				var storeFile = suppressionFile.GenerateSuppressionBase;

				if (/*_messagesByAssembly.ContainsKey(assemblyName) ||
					_generateSuppressionBaseByAssembly.ContainsKey(assemblyName) ||
					(storeFile && */_fileByAssembly.ContainsKey(assemblyName))//)
				{
					throw new InvalidOperationException($"Suppression information for assembly {assemblyName} has been already loaded");
				}

				//_messagesByAssembly.Add(assemblyName, suppressionFile.Messages);
				//_generateSuppressionBaseByAssembly.Add(assemblyName, suppressionFile.GenerateSuppressionBase);
				_fileByAssembly.Add(assemblyName, suppressionFile);
			}
		}

		public bool IsSuppressed(SemanticModel semanticModel, Diagnostic diagnostic)
		{
			var (assembly, message) = GetSuppressionInfo(semanticModel, diagnostic);

			if (assembly == null)
			{
				return false;
			}

			if (!_fileByAssembly.TryGetValue(assembly, out SuppressionFile file) ||
				!file.Messages.Contains(message))
			{
				return false;
			}

			return true;
		}

		private (string Assembly, SuppressMessage Message) GetSuppressionInfo(SemanticModel semanticModel, Diagnostic diagnostic)
		{
			if (semanticModel == null || diagnostic?.Location == null)
			{
				return (null, default);
			}

			var rootNode = semanticModel.SyntaxTree.GetRoot();
			if (rootNode == null)
			{
				return (null, default);
			}

			var diagnosticNode = rootNode.FindNode(diagnostic.Location.SourceSpan);
			if (diagnosticNode == null)
			{
				return (null, default);
			}

			var targetNode = FindTargetNode(diagnosticNode);
			if (targetNode == null)
			{
				return (null, default);
			}

			var targetSymbol = semanticModel.GetDeclaredSymbol(targetNode);
			if (targetSymbol == null)
			{
				return (null, default);
			}

			var assemblyName = targetSymbol.ContainingAssembly?.Name;
			if (string.IsNullOrEmpty(assemblyName))
			{
				return (null, default);
			}

			var id = diagnostic.Id;
			var target = targetSymbol.ToDisplayString();
			var syntaxNode = diagnosticNode.ToString();
			var message = new SuppressMessage(id, target, syntaxNode);

			return (assemblyName, message);
		}

		private SyntaxNode FindTargetNode(SyntaxNode node)
		{
			return node
				.Ancestors()
				.Where(a => _targetKinds.Contains(a.Kind()))
				.FirstOrDefault();
		}
	}
}
