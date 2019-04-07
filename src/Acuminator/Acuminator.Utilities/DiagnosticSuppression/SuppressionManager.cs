using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionManager
	{
		private readonly ConcurrentDictionary<string, SuppressionFile> _fileByAssembly = new ConcurrentDictionary<string, SuppressionFile>();

		private static HashSet<SyntaxKind> _targetKinds = new HashSet<SyntaxKind>(new[] {
			SyntaxKind.ClassDeclaration,
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
			SyntaxKind.PropertyDeclaration,
			SyntaxKind.FieldDeclaration
		});

		private readonly ISuppressionFileSystemService _fileSystemService;

		private static SuppressionManager Instance { get; set; }

		private SuppressionManager(ISuppressionFileSystemService fileSystemService,
			IEnumerable<(string path, bool generateSuppressionBase)> suppressionFiles)
		{
			fileSystemService.ThrowOnNull(nameof(fileSystemService));

			_fileSystemService = fileSystemService;

			foreach (var fileInfo in suppressionFiles)
			{
				if (!SuppressionFile.IsSuppressionFile(fileInfo.path))
				{
					throw new ArgumentException($"File {fileInfo.path} is not a suppression file");
				}

				var (file, assembly) = CreateFileTrackChanges(fileInfo);

				if (!_fileByAssembly.TryAdd(assembly, file))
				{
					throw new InvalidOperationException($"Suppression information for assembly {assembly} has been already loaded");
				}
			}
		}

		private (SuppressionFile File, string Assembly) CreateFileTrackChanges((string Path, bool GenerateSuppressionBase) fileInfo)
		{
			var suppressionFile = SuppressionFile.Load(_fileSystemService, fileInfo);
			var assemblyName = suppressionFile.AssemblyName;

			suppressionFile.Changed += ReloadFile;

			return (suppressionFile, assemblyName);
		}

		public void ReloadFile(object sender, SuppressionFileEventArgs e)
		{
			var fileInfo = (path: e.FullPath, generateSuppressionBase: false);
			var (newFile, assembly) = CreateFileTrackChanges(fileInfo);
			var oldFile = _fileByAssembly.GetOrAdd(assembly, (SuppressionFile)null);

			// We need to unsubscribe from the old file's event because it can be fired until the link to the file will be collected by GC
			if (oldFile != null)
			{
				oldFile.Changed -= ReloadFile;
			}

			_fileByAssembly[assembly] = newFile;
		}

		public static void Init(ISuppressionFileSystemService fileSystemService,
			IEnumerable<(string Path, bool GenerateSuppressionBase)> additionalFiles)
		{
			additionalFiles.ThrowOnNull(nameof(additionalFiles));

			var suppressionFiles = additionalFiles.Where(f => SuppressionFile.IsSuppressionFile(f.Path));

			if (Instance != null)
			{
				throw new InvalidOperationException($"{typeof(SuppressionManager).Name} has been already initialized");
			}

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

			var file = _fileByAssembly.GetOrAdd(assembly, (SuppressionFile)null);

			if (file == null)
			{
				return false;
			}

			if (file.GenerateSuppressionBase)
			{
                if (diagnostic?.Descriptor.DefaultSeverity != DiagnosticSeverity.Info)
                {
                    file.AddMessage(message);
                }

				return true;
			}

			if (!file.ContainsMessage(message))
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
			var oldMessages = oldFile.CopyMessages();
			var newMessages = newFile.CopyMessages();

			var addedMessages = new HashSet<SuppressMessage>(newMessages);
			addedMessages.ExceptWith(oldMessages);

			var deletedMessages = new HashSet<SuppressMessage>(oldMessages);
			deletedMessages.ExceptWith(newMessages);

			return new SuppressionDiffResult(oldFile.AssemblyName, oldFile.Path, addedMessages, deletedMessages);
		}

		public static void ReportDiagnosticWithSuppressionCheck(SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic,
																Diagnostic diagnostic, CodeAnalysisSettings settings, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			if (settings.SuppressionMechanismEnabled && Instance?.IsSuppressed(semanticModel, diagnostic, cancellation) == true)
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
