using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionManager
	{
		private readonly Dictionary<string, SuppressionFile> _fileByAssembly = new Dictionary<string, SuppressionFile>();
		private static HashSet<SyntaxKind> _targetKinds = new HashSet<SyntaxKind>(new[] {
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
			if (Instance == null)
			{
				throw new InvalidOperationException($"{nameof(SuppressionManager)} instance was not initialized");
			}

			var filesWithGeneratedSuppression = Instance._fileByAssembly.Values.Where(f => f.GenerateSuppressionBase);

			foreach (var file in filesWithGeneratedSuppression)
			{
				Instance._fileSystemService.Save(file.MessagesToDocument(), file.Path);
			}
		}

		private bool IsSuppressed(SemanticModel semanticModel, Diagnostic diagnostic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var (assembly, message) = GetSuppressionInfo(semanticModel, diagnostic, cancellation);

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

		public static IEnumerable<SuppressionDiffResult> ValidateSuppressionBaseDiff()
		{
			if (Instance == null)
			{
				return Enumerable.Empty<SuppressionDiffResult>();
			}

			var diffList = new List<SuppressionDiffResult>();

			foreach (var entry in Instance._fileByAssembly)
			{
				var currentFile = entry.Value;
				var oldFile = SuppressionFile.Load(Instance._fileSystemService, (currentFile.Path, false));

				diffList.Add(CompareFiles(oldFile, currentFile));
			}

			return diffList;
		}

		private static SuppressionDiffResult CompareFiles(SuppressionFile oldFile, SuppressionFile newFile)
		{
			var oldMessages = oldFile.MessagesCopy;
			var newMessages = newFile.MessagesCopy;

			var addedMessages = new HashSet<SuppressMessage>(newMessages);
			addedMessages.ExceptWith(oldMessages);

			var deletedMessages = new HashSet<SuppressMessage>(oldMessages);
			deletedMessages.ExceptWith(newMessages);

			return new SuppressionDiffResult(oldFile.AssemblyName, oldFile.Path, addedMessages, deletedMessages);
		}

		public static void ReportDiagnosticWithSuppressionCheck(SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic,
			Diagnostic diagnostic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (Instance?.IsSuppressed(semanticModel, diagnostic, cancellation) == true)
			{
				return;
			}

			reportDiagnostic(diagnostic);
		}

		private static (string Assembly, SuppressMessage Message) GetSuppressionInfo(
			SemanticModel semanticModel, Diagnostic diagnostic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (semanticModel == null || diagnostic?.Location == null)
			{
				return (null, default);
			}

			var rootNode = semanticModel.SyntaxTree.GetRoot(cancellation);
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

			var targetSymbol = semanticModel.GetDeclaredSymbol(targetNode, cancellation);
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

			// Try to obtain token in case of member declaration syntax as we do not want to store the text
			// of the entire declaration node
			var token = default(SyntaxToken?);
			if (diagnosticNode is MemberDeclarationSyntax memberDeclaration)
			{
				try
				{
					token = memberDeclaration.FindToken(diagnostic.Location.SourceSpan.Start);
				}
				catch (ArgumentOutOfRangeException)
				{
					token = null;
				}
			}

			var syntaxNode = token != null ?
				token.ToString() :
				// Replace \r symbol as XDocument does not preserve it in suppression file
				diagnosticNode.ToString().Replace("\r", "");
			var message = new SuppressMessage(id, target, syntaxNode);

			return (assemblyName, message);
		}

		private static SyntaxNode FindTargetNode(SyntaxNode node)
		{
			var targetNode = node
				.AncestorsAndSelf()
				.Where(a => _targetKinds.Contains(a.Kind()))
				.FirstOrDefault();

			// Use first variable in case of field declaration as it may contain multiple variables and therefore
			// cannot be used to obtain declared symbol
			return targetNode is FieldDeclarationSyntax fieldDeclaration ?
				fieldDeclaration.Declaration?.Variables[0] :
				targetNode;
		}
	}
}
