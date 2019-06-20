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

				var (file, assembly) = CreateFileTrackChanges(fileInfo.Path, fileInfo.GenerateSuppressionBase);

				if (!_fileByAssembly.TryAdd(assembly, file))
				{
					throw new InvalidOperationException($"Suppression information for assembly {assembly} has been already loaded");
				}
			}
		}

		private (SuppressionFile File, string Assembly) CreateFileTrackChanges(string suppressionFilePath, bool generateSuppressionBase)
		{
			var suppressionFile = SuppressionFile.Load(_fileSystemService, suppressionFilePath, generateSuppressionBase);
			var assemblyName = suppressionFile.AssemblyName;

			suppressionFile.Changed += ReloadFile;

			return (suppressionFile, assemblyName);
		}

		public void ReloadFile(object sender, SuppressionFileEventArgs e)
		{	
			var (newFile, assembly) = CreateFileTrackChanges(suppressionFilePath: e.FullPath, generateSuppressionBase: false);
			var oldFile = _fileByAssembly.GetOrAdd(assembly, (SuppressionFile)null);

			// We need to unsubscribe from the old file's event because it can be fired until the link to the file will be collected by GC
			if (oldFile != null)
			{
				oldFile.Changed -= ReloadFile;
			}

			_fileByAssembly[assembly] = newFile;
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
				var oldFile = SuppressionFile.Load(Instance._fileSystemService, suppressionFilePath: currentFile.Path,
												   generateSuppressionBase: false);

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
	}
}
