﻿#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.BuildAction;
using Acuminator.Utilities.DiagnosticSuppression.IO;
using Acuminator.Utilities.Roslyn.ProjectSystem;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public sealed partial class SuppressionManager
	{
		private static readonly Regex _suppressPattern = new Regex(@"Acuminator\s+disable\s+once\s+(\w+)\s+(\w+)", RegexOptions.Compiled);
		private static readonly object _initializationLocker = new object();

		internal static SuppressionManager? Instance
		{
			get;
			private set;
		}

		private readonly FilesStore _fileByAssembly = new FilesStore();

		internal ICustomBuildActionSetter? BuildActionSetter { get; }

		
		private readonly ISuppressionFileSystemService _fileSystemService;
		private readonly SuppressionFileCreator _suppressionFileCreator;

		private SuppressionManager(ISuppressionFileSystemService fileSystemService, ICustomBuildActionSetter? buildActionSetter)
		{
			_fileSystemService = fileSystemService.CheckIfNull();
			_suppressionFileCreator = new SuppressionFileCreator(this);

			BuildActionSetter = buildActionSetter;
		}

		public static void InitOrReset(IEnumerable<SuppressionManagerInitInfo>? additionalFiles,
									   Func<ISuppressionFileSystemService>? fileSystemServiceFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null) =>
			InitOrReset(additionalFiles, fileSystemServiceFabric, null, buildActionSetterFabric);

		public static void InitOrReset(IEnumerable<SuppressionManagerInitInfo>? additionalFiles,
									   Func<IIOErrorProcessor>? errorProcessorFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null) =>
			InitOrReset(additionalFiles, null, errorProcessorFabric, buildActionSetterFabric);

		public static void InitOrReset(Workspace? workspace, bool generateSuppressionBase, 
									   Func<ISuppressionFileSystemService>? fileSystemServiceFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null) =>
			InitOrReset(workspace?.CurrentSolution?.GetSuppressionInfo(generateSuppressionBase),
						fileSystemServiceFabric, null, buildActionSetterFabric);

		public static void InitOrReset(Workspace? workspace, bool generateSuppressionBase,
									   Func<IIOErrorProcessor>? errorProcessorFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null)
		{
			var suppressionFileInfos = workspace?.CurrentSolution?.GetSuppressionInfo(generateSuppressionBase);
			InitOrReset(suppressionFileInfos, null, errorProcessorFabric, buildActionSetterFabric);
		}

		private static void InitOrReset(IEnumerable<SuppressionManagerInitInfo>? suppressionFileInfos,
										Func<ISuppressionFileSystemService>? fileSystemServiceFabric,
										Func<IIOErrorProcessor>? errorProcessorFabric,
										Func<ICustomBuildActionSetter>? buildActionSetterFabric)
		{
			suppressionFileInfos ??= Enumerable.Empty<SuppressionManagerInitInfo>();

			lock (_initializationLocker)
			{
				if (Instance == null)
				{
					ISuppressionFileSystemService fileSystemService;

					if (fileSystemServiceFabric == null)
					{
						IIOErrorProcessor? errorProcessor = errorProcessorFabric?.Invoke();
						fileSystemService = new SuppressionFileWithChangesTrackingSystemService(errorProcessor);
					}
					else
					{
						fileSystemService = fileSystemServiceFabric();
					}

					ICustomBuildActionSetter? customBuildActionSetter = buildActionSetterFabric?.Invoke();
					Instance = new SuppressionManager(fileSystemService, customBuildActionSetter);
				}
				else
				{
					Instance.Clear();
				}

				Instance.LoadSuppressionFiles(suppressionFileInfos);
			}
		}

		private void Clear()
		{
			foreach (SuppressionFile oldFile in _fileByAssembly.Files.Where(file => file != null))
			{
				oldFile.Changed -= ReloadFile;
				oldFile.Dispose();
			}

			_fileByAssembly.Clear();
		}

		private void LoadSuppressionFiles(IEnumerable<SuppressionManagerInitInfo> suppressionFiles)
		{
			foreach (SuppressionManagerInitInfo fileInfo in suppressionFiles)
			{
				if (!SuppressionFile.IsSuppressionFile(fileInfo.Path))
				{
					throw new ArgumentException($"File {fileInfo.Path} is not a suppression file");
				}

				var file = LoadFileAndTrackItsChanges(fileInfo.Path, fileInfo.GenerateSuppressionBase);

				if (!_fileByAssembly.TryAdd(file.AssemblyName, file))
				{
					throw new InvalidOperationException($"Suppression information for assembly {file.AssemblyName} has been already loaded");
				}
			}
		}

		private SuppressionFile LoadFileAndTrackItsChanges(string suppressionFilePath, bool generateSuppressionBase)
		{
			lock (_fileSystemService)
			{
				SuppressionFile suppressionFile = SuppressionFile.Load(_fileSystemService, suppressionFilePath, generateSuppressionBase);
				suppressionFile.Changed += ReloadFile;
				return suppressionFile;
			}
		}

		public void ReloadFile(object sender, FileSystemEventArgs e)
		{
			string assembly = _fileSystemService.GetFileName(e.FullPath);
			var oldFile = GetSuppressionFile(assembly);

			// We need to unsubscribe from the old file's event because it can be fired until the link to the file will be collected by GC
			if (oldFile != null)
			{
				oldFile.Changed -= ReloadFile;
				oldFile.Dispose();
			}

			var newFile = LoadFileAndTrackItsChanges(suppressionFilePath: e.FullPath, generateSuppressionBase: false);
			_fileByAssembly[assembly] = newFile;
		}

		public static void SaveSuppressionBase()
		{
			CheckIfInstanceIsInitialized(throwOnNotInitialized: true);

			lock (Instance._fileSystemService)
			{
				//Create local copy in order to avoid concurency problem when the collection is changed during the iteration
				var filesWithGeneratedSuppression = Instance._fileByAssembly.Files.Where(f => f.GenerateSuppressionBase).ToList();

				foreach (var file in filesWithGeneratedSuppression)
				{
					XDocument newSuppressionXmlFile = file.MessagesToDocument(Instance._fileSystemService);
					Instance._fileSystemService.Save(newSuppressionXmlFile, file.Path);
				}
			}
		}

		internal SuppressionFile LoadSuppressionFileFrom(string filePath)
		{
			SuppressionFile suppressionFile = LoadFileAndTrackItsChanges(filePath, generateSuppressionBase: false);
			_fileByAssembly[suppressionFile.AssemblyName] = suppressionFile;
			return suppressionFile;
		}

		public SuppressionFile? GetSuppressionFile(string assemblyName) =>
			_fileByAssembly.TryGetValue(assemblyName.CheckIfNullOrWhiteSpace(), out var existingSuppressionFile)
				? existingSuppressionFile
				: null;

		public static SuppressionFile CreateSuppressionFileForProjectFromCommand(Project project)
		{
			CheckIfInstanceIsInitialized(throwOnNotInitialized: true);
			return Instance._suppressionFileCreator.CreateSuppressionFileForProjectFromCommand(project);
		}

		public static TextDocument? CreateRoslynAdditionalFile(Project project) =>
			CheckIfInstanceIsInitialized(throwOnNotInitialized: false)
				? Instance._suppressionFileCreator.AddAdditionalSuppressionDocumentToProject(project)
				: null;

		public static bool SuppressDiagnostic(SemanticModel semanticModel, string diagnosticID, TextSpan diagnosticSpan,
											  DiagnosticSeverity defaultDiagnosticSeverity, CancellationToken cancellation = default)
		{
			CheckIfInstanceIsInitialized(throwOnNotInitialized: true);

			if (!IsSuppressableSeverity(defaultDiagnosticSeverity))
				return false;

			var (fileAssemblyName, suppressMessage) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnosticID,
																						 diagnosticSpan, cancellation);
			if (fileAssemblyName.IsNullOrWhiteSpace() || !suppressMessage.IsValid)
				return false;

			lock (Instance._fileSystemService)
			{
				if (!Instance._fileByAssembly.TryGetValue(fileAssemblyName, out var file) || file == null)
					return false;

				file.AddMessage(suppressMessage);
				XDocument newSuppressionXmlFile = file.MessagesToDocument(Instance._fileSystemService);
				Instance._fileSystemService.Save(newSuppressionXmlFile, file.Path);
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(returnValue: true, nameof(Instance))]
		public static bool CheckIfInstanceIsInitialized([System.Diagnostics.CodeAnalysis.DoesNotReturnIf(true)] bool throwOnNotInitialized)
		{
			if (Instance == null)
			{
				lock (_initializationLocker)
				{
					if (Instance == null) //-V3054 Justification: on the current CLR the memory model it should work, fix while bringing little value will make the logic much more complex 
					{
						return throwOnNotInitialized
							? throw new InvalidOperationException($"{nameof(SuppressionManager)} instance was not initialized")
							: false;
					}
				}
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

			lock (Instance._fileSystemService)
			{
				foreach (SuppressionFile currentFile in Instance._fileByAssembly.Files)
				{
					var oldFile = SuppressionFile.Load(Instance._fileSystemService, suppressionFilePath: currentFile.Path,
													   generateSuppressionBase: false);

					diffList.Add(CompareFiles(oldFile, currentFile));
				}
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
			reportDiagnostic.ThrowOnNull();

			if (!settings.SuppressionMechanismEnabled)
			{
				reportDiagnostic(diagnostic);
				return;
			}

			// Always check suppression with a comment first.
			// Also we need to avoid modification of the suppression file when Requirement Validation tool runs the Acuminator in a special mode, 
			// in which every found diagnostic is recorded in the suppression file.
			if (CheckSuppressedComment(diagnostic, cancellation) ||
				Instance?.IsSuppressedInSuppressionFile(semanticModel, diagnostic, cancellation) == true)
			{
				return;
			}

			reportDiagnostic(diagnostic);
		}

		private static bool CheckSuppressedComment(Diagnostic diagnostic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			SyntaxNode? root = diagnostic.Location.SourceTree?.GetRoot(cancellation);
			SyntaxNode? node = root?.FindNode(diagnostic.Location.SourceSpan);
			bool containsComment = false;
			string? shortName = diagnostic.Descriptor.CustomTags.FirstOrDefault()?.NullIfWhiteSpace();

			// Climb to the hill. Looking for comment on parents nodes.
			while (node != null && node != root)
			{
				containsComment = CheckSuppressionCommentOnNode(diagnostic, shortName, node, cancellation);

				if (node is (StatementSyntax or MemberDeclarationSyntax or UsingDirectiveSyntax) || containsComment)
					break;

				node = node.Parent;
			}

			return containsComment;
		}

		private static bool CheckSuppressionCommentOnNode(Diagnostic diagnostic, string? diagnosticShortName, SyntaxNode node, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var trivias = node.GetLeadingTrivia();

			if (trivias.Count == 0)
				return false;

			var successfulMatch = trivias.Where(x => x.IsKind(SyntaxKind.SingleLineCommentTrivia))
										 .Select(trivia => _suppressPattern.Match(trivia.ToString()))
										 .FirstOrDefault(match => match.Success && diagnostic.Id == match.Groups[1].Value &&
																  (diagnosticShortName == null || diagnosticShortName == match.Groups[2].Value));
			return successfulMatch != null;
		}

		private bool IsSuppressedInSuppressionFile(SemanticModel semanticModel, Diagnostic diagnostic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var (assembly, message) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnostic, cancellation);

			if (assembly == null)
				return false;

			SuppressionFile? file = GetSuppressionFile(assembly);

			if (file == null)
				return false;

			if (file.GenerateSuppressionBase)
			{
				if (IsSuppressableSeverity(diagnostic?.Descriptor.DefaultSeverity))
				{
					file.AddMessage(message);
				}

				return true;
			}

			return file.ContainsMessage(message);
		}

		private static bool IsSuppressableSeverity(DiagnosticSeverity? diagnosticSeverity) =>
			diagnosticSeverity == DiagnosticSeverity.Error || diagnosticSeverity == DiagnosticSeverity.Warning;
	}
}