using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionManager
	{
		//private readonly Dictionary<string, bool> _generateSuppressionBaseByAssembly = new Dictionary<string, bool>();
		private readonly Dictionary<string, SuppressionFile> _fileByAssembly = new Dictionary<string, SuppressionFile>();
		private readonly HashSet<SyntaxKind> _targetKinds = new HashSet<SyntaxKind>(new[] {
			SyntaxKind.ClassDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.FieldDeclaration
		});

		private static SuppressionManager Instance { get; set; }

		public SuppressionManager(IEnumerable<string> suppressionFiles)
		{
			foreach (var file in suppressionFiles)
			{
				if (!SuppressionFile.IsSuppressionFile(file))
				{
					throw new ArgumentException($"File {file} is not a suppression file");
				}

				var suppressionFile = SuppressionFile.Load(file);
				var assemblyName = suppressionFile.AssemblyName;
				var storeFile = suppressionFile.GenerateSuppressionBase;

				if (//_generateSuppressionBaseByAssembly.ContainsKey(assemblyName) ||
					_fileByAssembly.ContainsKey(assemblyName))
				{
					throw new InvalidOperationException($"Suppression information for assembly {assemblyName} has been already loaded");
				}

				//_generateSuppressionBaseByAssembly.Add(assemblyName, suppressionFile.GenerateSuppressionBase);
				_fileByAssembly.Add(assemblyName, suppressionFile);
			}
		}

		public static void Init(IEnumerable<string> additionalFiles)
		{
			var suppressionFiles = additionalFiles.Where(f => SuppressionFile.IsSuppressionFile(f));

			Instance = new SuppressionManager(suppressionFiles);
		}

		private bool IsSuppressed(SemanticModel semanticModel, Diagnostic diagnostic)
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

		public static void ReportDiagnosticWithSuppressionCheck(SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, Diagnostic diagnostic)
		{
			if (Instance?.IsSuppressed(semanticModel, diagnostic) == true)
			{
				return;
			}

			reportDiagnostic(diagnostic);
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
