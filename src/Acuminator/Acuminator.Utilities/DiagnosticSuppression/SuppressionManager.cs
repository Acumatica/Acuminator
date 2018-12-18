using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionManager
	{
		private readonly Dictionary<string, SuppressionFile> _fileByAssembly = new Dictionary<string, SuppressionFile>();
		private readonly HashSet<SyntaxKind> _targetKinds = new HashSet<SyntaxKind>(new[] {
			SyntaxKind.ClassDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.FieldDeclaration
		});
		private readonly ISuppressionFileSystemService _fileSystemService;

		private static SuppressionManager Instance { get; set; }

		public SuppressionManager(ISuppressionFileSystemService fileSystemService,
			IEnumerable<(string path, bool generateSuppressionBase)> suppressionFiles)
		{
			fileSystemService.ThrowOnNull(nameof(fileSystemService));

			_fileSystemService = fileSystemService;

			foreach (var file in suppressionFiles)
			{
				if (!SuppressionFile.IsSuppressionFile(file.path))
				{
					throw new ArgumentException($"File {file} is not a suppression file");
				}

				var suppressionFile = SuppressionFile.Load(_fileSystemService, file);
				var assemblyName = suppressionFile.AssemblyName;

				if (_fileByAssembly.ContainsKey(assemblyName))
				{
					throw new InvalidOperationException($"Suppression information for assembly {assemblyName} has been already loaded");
				}

				_fileByAssembly.Add(assemblyName, suppressionFile);
			}
		}

		public static void Init(ISuppressionFileSystemService fileSystemService,
			IEnumerable<(string path, bool generateSuppressionBase)> additionalFiles)
		{
			var suppressionFiles = additionalFiles.Where(f => SuppressionFile.IsSuppressionFile(f.path));

			Instance = new SuppressionManager(fileSystemService, suppressionFiles);
		}

		public static void SaveSuppressionBase()
		{
			foreach (var file in Instance._fileByAssembly.Values)
			{
				Instance._fileSystemService.Save(file.MessagesToDocument(), file.Path);
			}
		}

		private bool IsSuppressed(SemanticModel semanticModel, Diagnostic diagnostic)
		{
			var (assembly, message) = GetSuppressionInfo(semanticModel, diagnostic);

			if (assembly == null)
			{
				return false;
			}

			var fileExists = _fileByAssembly.TryGetValue(assembly, out SuppressionFile file);

			if (fileExists && file.GenerateSuppressionBase)
			{
				file.AddMessage(message);

				return true;
			}

			if (!fileExists || !file.ContainsMessage(message))
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
