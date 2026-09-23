using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression.BuildAction;
using Acuminator.Utilities.DiagnosticSuppression.IO;
using Acuminator.Utilities.Roslyn.ProjectSystem;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
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

		private static readonly ConditionalWeakTable<Compilation, CompilationSuppressionInfo>
			_suppressionInfoByCompilation = new();

		internal ICustomBuildActionSetter? BuildActionSetter { get; }

		
		private readonly ISuppressionFileSystemService _fileSystemService;
		private readonly SuppressionFileCreator _suppressionFileCreator;

		private SuppressionManager(ISuppressionFileSystemService fileSystemService, ICustomBuildActionSetter? buildActionSetter)
		{
			_fileSystemService = fileSystemService.CheckIfNull();
			_suppressionFileCreator = new SuppressionFileCreator(this);

			BuildActionSetter = buildActionSetter;
		}

		public static void InitOrReset(IEnumerable<GlobalSuppressionFileInitInfo>? additionalFiles,
									   Func<ISuppressionFileSystemService>? fileSystemServiceFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null) =>
			InitOrReset(additionalFiles, fileSystemServiceFabric, errorProcessorFabric: null, buildActionSetterFabric);

		public static void InitOrReset(IEnumerable<GlobalSuppressionFileInitInfo>? additionalFiles,
									   Func<IIOErrorProcessor>? errorProcessorFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null) =>
			InitOrReset(additionalFiles, fileSystemServiceFabric: null, errorProcessorFabric, buildActionSetterFabric);

		public static void InitOrReset(Workspace? workspace, AcuminatorWorkMode workMode,
									   Func<ISuppressionFileSystemService>? fileSystemServiceFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null) =>
			InitOrReset(workspace?.CurrentSolution?.GetSuppressionInfo(workMode),
						fileSystemServiceFabric, errorProcessorFabric: null, buildActionSetterFabric);

		public static void InitOrReset(Workspace? workspace, AcuminatorWorkMode workMode,
									   Func<IIOErrorProcessor>? errorProcessorFabric = null,
									   Func<ICustomBuildActionSetter>? buildActionSetterFabric = null)
		{
			var suppressionFileInfos = workspace?.CurrentSolution?.GetSuppressionInfo(workMode);
			InitOrReset(suppressionFileInfos, fileSystemServiceFabric: null, errorProcessorFabric, buildActionSetterFabric);
		}

		private static void InitOrReset(IEnumerable<GlobalSuppressionFileInitInfo>? suppressionFileInfos,
										Func<ISuppressionFileSystemService>? fileSystemServiceFabric,
										Func<IIOErrorProcessor>? errorProcessorFabric,
										Func<ICustomBuildActionSetter>? buildActionSetterFabric)
		{
			suppressionFileInfos ??= [];

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

		private void LoadSuppressionFiles(IEnumerable<GlobalSuppressionFileInitInfo> suppressionFiles)
		{
			foreach (GlobalSuppressionFileInitInfo fileInfo in suppressionFiles)
			{
				if (!fileInfo.Path.IsSuppressionFile(checkFileExists: false))
				{
					throw new ArgumentException($"File {fileInfo.Path} is not a suppression file");
				}

				var file = LoadFileAndTrackItsChanges(fileInfo.Path, fileInfo.WorkMode);

				if (!_fileByAssembly.TryAdd(file.AssemblyName, file))
				{
					throw new InvalidOperationException($"Suppression information for assembly {file.AssemblyName} has been already loaded");
				}
			}
		}

		private SuppressionFile LoadFileAndTrackItsChanges(string suppressionFilePath, AcuminatorWorkMode workMode)
		{
			lock (_fileSystemService)
			{
				SuppressionFile suppressionFile = SuppressionFile.Load(_fileSystemService, suppressionFilePath, workMode);
				suppressionFile.Changed += ReloadFile;
				return suppressionFile;
			}
		}

		public void ReloadFile(object sender, FileSystemEventArgs e)
		{
			string assembly = _fileSystemService.GetFileName(e.FullPath);
			var oldFile = GetSuppressionFile(assembly);

			// We need to unsubscribe from the old file's event because it can be fired until the link to the file will be collected by GC
			ICollection<SuppressMessage>? newMessagesInOldFile = null;

			if (oldFile != null)
			{
				newMessagesInOldFile = oldFile.GetNewSuppressions();
				oldFile.Changed -= ReloadFile;
				oldFile.Dispose();
			}

			var workMode = oldFile?.WorkMode ?? AcuminatorWorkMode.ReportUnsuppressedErrors;
			var newFile = LoadFileAndTrackItsChanges(suppressionFilePath: e.FullPath, workMode);
			
			if (newMessagesInOldFile?.Count > 0)
			{
				foreach (SuppressMessage newMessage in newMessagesInOldFile)
				{
					newFile.AddGeneratedSuppressionMessage(newMessage);
				}
			}

			_fileByAssembly[assembly] = newFile;
		}

		public static void SaveSuppressionFiles(bool saveOnlyGeneratedFiles)
		{
			CheckIfInstanceIsInitialized(throwOnNotInitialized: true);

			lock (Instance._fileSystemService)
			{
				var filesStore = Instance._fileByAssembly;
				var filesWithGeneratedSuppression = saveOnlyGeneratedFiles 
					? filesStore.Files.Where(f => f.WorkMode.HasFlag(AcuminatorWorkMode.GenerateSuppressionFile)) 
					: filesStore.Files;

				//Create local copy in order to avoid concurency problem when the collection is changed during the iteration
				var filesListSnapshot = filesWithGeneratedSuppression.ToList(filesStore.Count);

				foreach (var file in filesListSnapshot)
				{
					XDocument newSuppressionXmlFile = file.ReloadSuppressionFileWithNewMessagesFromMemory(Instance._fileSystemService);
					Instance._fileSystemService.Save(newSuppressionXmlFile, file.Path);
				}
			}
		}

		internal SuppressionFile LoadSuppressionFileFrom(string filePath, AcuminatorWorkMode workMode)
		{
			SuppressionFile suppressionFile = LoadFileAndTrackItsChanges(filePath, workMode);
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

		public static bool SuppressDiagnosticInSuppressionFile(SemanticModel semanticModel, string diagnosticID, TextSpan diagnosticSpan,
															   CancellationToken cancellation = default)
		{
			CheckIfInstanceIsInitialized(throwOnNotInitialized: true);
			var (fileAssemblyName, suppressMessage) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnosticID,
																						 diagnosticSpan, cancellation);
			if (fileAssemblyName.IsNullOrWhiteSpace() || !suppressMessage.IsValid)
				return false;

			lock (Instance._fileSystemService)
			{
				if (!Instance._fileByAssembly.TryGetValue(fileAssemblyName, out var file) || file == null)
					return false;

				file.AddGeneratedSuppressionMessage(suppressMessage);
				XDocument newSuppressionXmlFile = file.ReloadSuppressionFileWithNewMessagesFromMemory(Instance._fileSystemService);
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

		public static IReadOnlyCollection<SuppressionDiffResult> ValidateSuppressionBaseDiff()
		{
			if (Instance == null)
				return [];

			var diffList = new List<SuppressionDiffResult>();

			lock (Instance._fileSystemService)
			{
				foreach (SuppressionFile currentFile in Instance._fileByAssembly.Files)
				{
					var oldFile = SuppressionFile.Load(Instance._fileSystemService, suppressionFilePath: currentFile.Path,
													   AcuminatorWorkMode.ReportUnsuppressedErrors);

					diffList.Add(CompareFiles(oldFile, currentFile));
				}
			}

			return diffList;
		}

		private static SuppressionDiffResult CompareFiles(SuppressionFile oldFile, SuppressionFile newFile)
		{
			var oldMessages = oldFile.GetAllSuppressions();
			var newMessages = newFile.GetAllSuppressions();

			var addedMessages = new HashSet<SuppressMessage>(newMessages);

			if (oldMessages.Count > 0)
				addedMessages.ExceptWith(oldMessages);

			var deletedMessages = new HashSet<SuppressMessage>(oldMessages);

			if (newMessages.Count > 0)
				deletedMessages.ExceptWith(newMessages);

			return new SuppressionDiffResult(oldFile.AssemblyName, oldFile.Path, addedMessages, deletedMessages);
		}

		public static void ReportDiagnosticWithSuppressionCheck(SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic,
																Diagnostic diagnostic, CodeAnalysisSettings settings, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();
			reportDiagnostic.ThrowOnNull();

			// Do not report or generate suppression for info diagnostics if the InfoDiagnosticsEnabled flag is disabled
			if (!settings.InfoDiagnosticsEnabled &&
				(diagnostic.Severity is DiagnosticSeverity.Info or DiagnosticSeverity.Hidden))
			{
				return;
			}

			bool isSuppressionEnabled  = settings.SuppressionMechanismEnabled;
			bool hasSuppressionComment = CheckSuppressedComment(diagnostic, cancellation);

			// Always check suppression with a comment first
			if (isSuppressionEnabled && hasSuppressionComment)
				return;
			
			// Then check suppression with a global suppression file.
			// If a suppression file generation mode is enabled, then the suppression file will be updated with the new suppression message.
			if (Instance != null && 
				Instance.IsSuppressedInSuppressionFileAndAddToSuppressionFileInGenerationMode(semanticModel, diagnostic, 
																							  isSuppressionEnabled, hasSuppressionComment, cancellation))
			{
				return;
			}

			if (Instance == null &&
				isSuppressionEnabled &&
				IsSuppressedByCompilationSuppressionInfo(semanticModel, diagnostic, cancellation))
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

				if (SuppressionExtensions.ShouldStopSearchForSuppressionComment(node) || containsComment)
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

			var successfulMatch = trivias.Where((in SyntaxTrivia x) => x.IsKind(SyntaxKind.SingleLineCommentTrivia))
										 .Select(trivia => _suppressPattern.Match(trivia.ToString()))
										 .FirstOrDefault(match => match.Success && diagnostic.Id == match.Groups[1].Value &&
																  (diagnosticShortName == null || diagnosticShortName == match.Groups[2].Value));
			return successfulMatch != null;
		}

		private bool IsSuppressedInSuppressionFileAndAddToSuppressionFileInGenerationMode(SemanticModel semanticModel, Diagnostic diagnostic, 
																						  bool isSuppressionEnabled, bool hasSuppressionComment, 
																						  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var (assembly, message) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnostic, cancellation);

			if (assembly == null)
				return false;

			SuppressionFile? file = GetSuppressionFile(assembly);

			if (file == null)
				return false;

			// Add new suppression info in suppression file generation mode to the suppression file only if it satisfies two conditions:
			// - New info isn't already present among the loaded suppressions, we don't want duplicates
			// - There is no Acuminator suppression comment for the suppressed diagnostic, we don't want to allow creation of suppressions
			//   for diagnostics suppressed with a comment.
			bool addSuppressionToSuppressionFile = file.WorkMode.HasFlag(AcuminatorWorkMode.GenerateSuppressionFile) && !hasSuppressionComment;

			if (addSuppressionToSuppressionFile)
			{
				file.AddGeneratedSuppressionMessage(message);   // The check for presence in loaded suppressions will be done by the called method 
			}

			if (file.WorkMode.HasFlag(AcuminatorWorkMode.ReportUnsuppressedErrors))
			{
				// Check whether the diagnostic is suppressed by checking if the suppression is enabled.
				// If it is enabled, check if the suppression file contains the suppressed message.
				// Also to be consistent with the algorithm and allow to call this method from anywhere,
				// make a cheap check if the suppression comment is present even though it was already verified in the calling method.
				bool isSuppressed = isSuppressionEnabled &&
									(hasSuppressionComment || file.ContainsLoadedSuppressedMessage(message));
				return isSuppressed;
			}
			else
			{
				// If the suppression work mode does not include reporting errors, return true for all diagnostics to consider them suppressed and not report them.
				return true;
			}
		}

		private static bool IsSuppressedByCompilationSuppressionInfo(SemanticModel semanticModel, Diagnostic diagnostic,
			CancellationToken cancellation)
		{
			if (semanticModel is null
				|| !_suppressionInfoByCompilation.TryGetValue(semanticModel.Compilation,
					out CompilationSuppressionInfo suppressionInfo)
				|| suppressionInfo.IsEmpty)
			{
				return false;
			}

			var (assembly, message) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnostic, cancellation);

			return assembly is not null && suppressionInfo.IsSuppressed(assembly, message);
		}

		public static void RegisterSuppressionInfoForCompilation(CompilationStartAnalysisContext compilationStartContext)
		{
			compilationStartContext.ThrowOnNull();

			RegisterSuppressionInfoForCompilation(compilationStartContext.Compilation,
				() => CompilationSuppressionInfo.CreateFromAdditionalFiles(compilationStartContext));
		}

		private static void RegisterSuppressionInfoForCompilation(Compilation compilation,
			Func<CompilationSuppressionInfo> suppressionInfoFactory)
		{
			compilation.ThrowOnNull();
			suppressionInfoFactory.ThrowOnNull();

			_suppressionInfoByCompilation.GetValue(compilation, _ => suppressionInfoFactory());
		}
	}
}
