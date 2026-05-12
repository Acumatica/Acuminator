#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Acuminator.Utilities.Roslyn.ProjectSystem;

namespace Acuminator.Vsix.Coloriser
{
	/// <content>
	/// A colorizer tagger base class.
	/// </content>
	public abstract class PXColorizerTaggerBase : PXTaggerBase, ITagger<IClassificationTag>, IDisposable
	{
		public BackgroundTagging? BackgroundTagging { get; protected set; }

		protected internal abstract ITagsCache<IClassificationTag> ClassificationTagsCache { get; }

		protected internal abstract ITagsCache<IOutliningRegionTag> OutliningsTagsCache { get; }

		protected internal abstract bool UseAsyncTagging { get; }

		protected PXColorizerTaggerProvider Provider => (ProviderBase as PXColorizerTaggerProvider)!;

		private bool _hasReferenceToAcumaticaPlatform;

		public sealed override bool HasReferenceToAcumaticaPlatform => _hasReferenceToAcumaticaPlatform;

		protected PXColorizerTaggerBase(ITextBuffer buffer, PXColorizerTaggerProvider aProvider, bool subscribeToSettingsChanges,
										bool useCacheChecking) :
								   base(buffer, aProvider, subscribeToSettingsChanges, useCacheChecking)
		{
			if (RoslynWorkspace != null)
			{
				_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(projectId: null);
				RoslynWorkspace.WorkspaceChanged += OnWorkspaceChanged;
			}
		}

		protected internal override void ResetCacheAndFlags(ITextSnapshot? newSnapshotToCache)
		{
			base.ResetCacheAndFlags(newSnapshotToCache);
			ClassificationTagsCache.Reset();
			OutliningsTagsCache.Reset();
		}

		public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if ((spans?.Count is null or 0) || AcuminatorVSPackage.Instance?.ColoringEnabled != true || !HasReferenceToAcumaticaPlatform)
				return [];

			ITextSnapshot newSnapshotToTag = spans[0].Snapshot;

			if (CheckIfRetaggingIsNotNecessary(newSnapshotToTag))
			{
				return ClassificationTagsCache.ProcessedTags;
			}

			if (UseAsyncTagging)
			{
				return GetTagsAsync(newSnapshotToTag);
			}
			else
			{
				ResetCacheAndFlags(newSnapshotToTag);
				return GetTagsSynchronousImplementation(newSnapshotToTag);
			}
		}

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods - this method is async by its nature.
		/// <summary>
		/// Gets the tags asynchronous in this collection.
		/// </summary>
		/// <param name="snapshot">The snapshot.</param>
		/// <returns>
		/// An enumerator that allows foreach to be used to process the tags asynchronous in this collection.
		/// </returns>
		protected virtual IEnumerable<ITagSpan<IClassificationTag>> GetTagsAsync(ITextSnapshot snapshot)

		{
			if (BackgroundTagging != null)
			{
				BackgroundTagging.CancelTagging();   //Cancel currently running task
				BackgroundTagging = null;
			}

			ResetCacheAndFlags(snapshot);
			BackgroundTagging = BackgroundTagging.StartBackgroundTagging(this);

			return ClassificationTagsCache.ProcessedTags;
		}
#pragma warning restore VSTHRD200

		protected internal abstract IEnumerable<ITagSpan<IClassificationTag>> GetTagsSynchronousImplementation(ITextSnapshot snapshot);

		protected internal abstract Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementationAsync(ITextSnapshot snapshot,
																													CancellationToken cancellationToken);

		public override void Dispose()
		{
			BackgroundTagging?.Dispose();
			ClassificationTagsCache?.Reset();
			OutliningsTagsCache?.Reset();

			if (RoslynWorkspace != null)
			{
				RoslynWorkspace.WorkspaceChanged -= OnWorkspaceChanged;
			}

			base.Dispose();
		}

		private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
		{
			bool oldValue = _hasReferenceToAcumaticaPlatform;

			switch (e.Kind)
			{
				case WorkspaceChangeKind.SolutionRemoved:
				case WorkspaceChangeKind.SolutionCleared:
					_hasReferenceToAcumaticaPlatform = false;
					break;

				case WorkspaceChangeKind.SolutionAdded:
				case WorkspaceChangeKind.ProjectAdded:
					_hasReferenceToAcumaticaPlatform |= CheckIfCurrentSolutionHasReferenceToAcumatica(e.ProjectId);
					break;

				case WorkspaceChangeKind.SolutionChanged:
				case WorkspaceChangeKind.SolutionReloaded:
				case WorkspaceChangeKind.ProjectRemoved:
					_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(e.ProjectId);
					break;

				case WorkspaceChangeKind.ProjectChanged:
				case WorkspaceChangeKind.ProjectReloaded:
					if (e.IsProjectMetadataChanged())
					{
						_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(e.ProjectId);
					}

					break;

				default:
					return;
			}

			if (Buffer.CurrentSnapshot != null && (oldValue != _hasReferenceToAcumaticaPlatform || !LastTaggingWasSuccessful))
			{
				ResetCacheAndFlags(newSnapshotToCache: null);
				RaiseTagsChanged();
			}
		}

		protected bool CheckIfCurrentSolutionHasReferenceToAcumatica(ProjectId? projectId)
		{
			var currentSolution = RoslynWorkspace?.CurrentSolution;

			if (currentSolution == null || currentSolution.ProjectIds.Count == 0)
				return false;

			var roslynDocument = Buffer.CurrentSnapshot?.GetOpenDocumentInCurrentContextWithChanges();
			var currentProject = roslynDocument?.Project;
			bool allProjectsChanged = projectId == null;

			if (allProjectsChanged)
			{
				bool hasAcumaticaProjectsInSolution =
					currentSolution.Projects.Any(project => IsAcumaticaAssemblyName(project.Name) || IsAcumaticaAssemblyName(project.AssemblyName));

				if (hasAcumaticaProjectsInSolution)
					return true;

				bool hasReferenceInMetadata = (from project in currentSolution.Projects
											   from reference in project.MetadataReferences
											   select Path.GetFileNameWithoutExtension(reference.Display))
											   .Any(reference => IsAcumaticaAssemblyName(reference));
				return hasReferenceInMetadata;
			}
			else if (currentProject?.Id == projectId)  // Check that the changed project is the same as the project of the current document. If not, then return the old value.
			{
				if (CanCreateGraphFastCheck(currentProject!) is bool canCreateGraph)
					return canCreateGraph;

				bool hasReferenceInMetadata = currentProject!.MetadataReferences.Count > 0
					? currentProject.MetadataReferences.Any(IsAcumaticaAssemblyName)
					: false;

				if (hasReferenceInMetadata)
					return true;

				bool isAcumaticaProject = IsAcumaticaAssemblyName(currentProject.Name) || IsAcumaticaAssemblyName(currentProject.AssemblyName);
				return isAcumaticaProject;
			}
			else
				return _hasReferenceToAcumaticaPlatform;
		}

		private static bool IsAcumaticaAssemblyName(MetadataReference reference)
		{
			string referenceName = Path.GetFileNameWithoutExtension(reference.Display);
			return IsAcumaticaAssemblyName(referenceName);
		}

		private static bool IsAcumaticaAssemblyName(string dllName) => ColoringConstants.PlatformDllName == dllName ||
																	   ColoringConstants.AppDllName == dllName;

		private static bool? CanCreateGraphFastCheck(Project project)
		{
			if (!project.TryGetCompilation(out var compilation) || compilation == null)
				return null;

			var graphType = compilation.GetTypeByMetadataName(TypeFullNames.PXGraph);
			return graphType != null;
		}
	}
}
