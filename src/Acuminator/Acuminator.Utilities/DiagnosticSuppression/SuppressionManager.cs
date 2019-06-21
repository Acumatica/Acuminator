using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public class SuppressionManager
	{
		private readonly ConcurrentDictionary<string, SuppressionFile> _fileByAssembly = new ConcurrentDictionary<string, SuppressionFile>();
		private readonly ISuppressionFileSystemService _fileSystemService;

		private static SuppressionManager Instance { get; set; }

		private SuppressionManager(ISuppressionFileSystemService fileSystemService, IEnumerable<SuppressionManagerInitInfo> suppressionFiles)
		{
			fileSystemService.ThrowOnNull(nameof(fileSystemService));

			_fileSystemService = fileSystemService;

			foreach (var fileInfo in suppressionFiles)
			{
				if (!SuppressionFile.IsSuppressionFile(fileInfo.Path))
				{
					throw new ArgumentException($"File {fileInfo.Path} is not a suppression file");
				}

				var (file, assembly) = LoadFileAndTrackItsChanges(fileInfo.Path, fileInfo.GenerateSuppressionBase);

				if (!_fileByAssembly.TryAdd(assembly, file))
				{
					throw new InvalidOperationException($"Suppression information for assembly {assembly} has been already loaded");
				}
			}
		}

		public void ReloadFile(object sender, SuppressionFileEventArgs e)
		{	
			var (newFile, assembly) = LoadFileAndTrackItsChanges(suppressionFilePath: e.FullPath, generateSuppressionBase: false);
			var oldFile = _fileByAssembly.GetOrAdd(assembly, (SuppressionFile)null);

			// We need to unsubscribe from the old file's event because it can be fired until the link to the file will be collected by GC
			if (oldFile != null)
			{
				oldFile.Changed -= ReloadFile;
			}

			_fileByAssembly[assembly] = newFile;
		}

		private (SuppressionFile File, string Assembly) LoadFileAndTrackItsChanges(string suppressionFilePath, bool generateSuppressionBase)
		{
			lock (_fileSystemService)
			{
				SuppressionFile suppressionFile = SuppressionFile.Load(_fileSystemService, suppressionFilePath, generateSuppressionBase);
				var assemblyName = suppressionFile.AssemblyName;

				suppressionFile.Changed += ReloadFile;

				return (suppressionFile, assemblyName);
			}		
		}

		public static void Init(ISuppressionFileSystemService fileSystemService, IEnumerable<SuppressionManagerInitInfo> additionalFiles)
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
			CheckIfInstanceIsInitialized();
			
			lock (Instance._fileSystemService)
			{
				//Create local copy in order to avoid concurency problem when the collection is changed during the iteration
				var filesWithGeneratedSuppression = Instance._fileByAssembly.Values.Where(f => f.GenerateSuppressionBase).ToList();

				foreach (var file in filesWithGeneratedSuppression)
				{
					Instance._fileSystemService.Save(file.MessagesToDocument(), file.Path);
				}
			}
		}

		public static SuppressionFile CreateSuppressionFileForProject(Project project)
		{
			project.ThrowOnNull(nameof(project));
			CheckIfInstanceIsInitialized();

			//First check if file already exists to dismiss threads withou acquiring the lock
			SuppressionFile existingSuppressionFile = Instance._fileByAssembly.GetOrAdd(project.Name, (SuppressionFile)null);

			if (existingSuppressionFile != null)
				return existingSuppressionFile;

			lock (Instance._fileSystemService)
			{
				//Second check inside the lock if file already exists 
				existingSuppressionFile = Instance._fileByAssembly.GetOrAdd(project.Name, (SuppressionFile)null);
				return existingSuppressionFile ?? AddNewSuppressionFileImpl();
			}

			//---------------------------------------------Local Function--------------------------------------------------
			SuppressionFile AddNewSuppressionFileImpl()
			{
				string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
				string projectDir = Instance._fileSystemService.GetFileDirectory(project.FilePath);
				string suppressionFilePath = Path.Combine(projectDir, suppressionFileName);

				//Create new xml document and save it on disk
				System.Xml.Linq.XDocument newXDocument = SuppressionFile.NewDocumentFromMessages(Enumerable.Empty<SuppressMessage>());

				if (!Instance._fileSystemService.Save(newXDocument, suppressionFilePath))
					return null;

				var (suppressionFile, assembly) = Instance.LoadFileAndTrackItsChanges(suppressionFilePath, generateSuppressionBase: false);
				Instance._fileByAssembly.TryAdd(assembly, suppressionFile);
				return suppressionFile;
			}
		}

		public static bool SuppressDiagnostic(SemanticModel semanticModel, string diagnosticID, TextSpan diagnosticSpan,
											  DiagnosticSeverity defaultDiagnosticSeverity, CancellationToken cancellation = default)
		{
			CheckIfInstanceIsInitialized();

			if (!IsSuppressableSeverity(defaultDiagnosticSeverity))
				return false;

			var (fileAssemblyName, suppressMessage) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnosticID, 
																						 diagnosticSpan, cancellation);

			if (fileAssemblyName.IsNullOrWhiteSpace() || !suppressMessage.IsValid)
				return false;

			lock (Instance._fileSystemService)
			{
				SuppressionFile file = Instance._fileByAssembly.GetOrAdd(fileAssemblyName, (SuppressionFile)null);

				if (file == null)
					return false;

				file.AddMessage(suppressMessage);
				Instance._fileSystemService.Save(file.MessagesToDocument(), file.Path);
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
				foreach (var entry in Instance._fileByAssembly)
				{
					var currentFile = entry.Value;
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

			if (settings.SuppressionMechanismEnabled && Instance?.IsSuppressed(semanticModel, diagnostic, cancellation) == true)
			{
				return;
			}

			reportDiagnostic(diagnostic);
		}

		private bool IsSuppressed(SemanticModel semanticModel, Diagnostic diagnostic, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var (assembly, message) = SuppressMessage.GetSuppressionInfo(semanticModel, diagnostic, cancellation);

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
				if (IsSuppressableSeverity(diagnostic?.Descriptor.DefaultSeverity))
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

		private static bool IsSuppressableSeverity(DiagnosticSeverity? diagnosticSeverity) =>
			diagnosticSeverity == DiagnosticSeverity.Error || diagnosticSeverity == DiagnosticSeverity.Warning;

		private static void CheckIfInstanceIsInitialized()
		{
			if (Instance == null)
			{
				throw new InvalidOperationException($"{nameof(SuppressionManager)} instance was not initialized");
			}
		}
	}
}
