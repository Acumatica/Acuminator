#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.ProjectSystem;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Acuminator.Vsix.Coloriser;

/// <summary>
/// A Roslyn-based colorizer tagger.
/// </summary>
internal partial class PXRoslynColorizerTagger : PXTaggerBase, ITagger<IClassificationTag>, IDisposable
{
	protected internal TagsCacheAsync<IClassificationTag> ClassificationTagsCache { get; }

	protected internal TagsCacheAsync<IOutliningRegionTag> OutliningsTagsCache { get; }

	public BackgroundTagging? BackgroundTagging { get; private set; }

	protected PXColorizerTaggerProvider Provider { get; }

	private bool _hasReferenceToAcumaticaPlatform;

	public sealed override bool HasReferenceToAcumaticaPlatform => _hasReferenceToAcumaticaPlatform;

	internal override bool LastTaggingWasSuccessful { get; set; }

	public Workspace? RoslynWorkspace { get; private set; }

	public PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider provider, bool subscribeToSettingsChanges,
									bool useCacheChecking) :
							  base(buffer, subscribeToSettingsChanges, useCacheChecking)
	{
		Provider = provider.CheckIfNull();

		ClassificationTagsCache = new TagsCacheAsync<IClassificationTag>();
		OutliningsTagsCache = new TagsCacheAsync<IOutliningRegionTag>();
		RoslynWorkspace = Buffer.GetWorkspaceThatSupportsColoring();

		if (RoslynWorkspace != null)
		{
			_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(projectId: null);
			RoslynWorkspace.WorkspaceChanged += OnWorkspaceChanged;
		}

		//Buffer.Changed += Buffer_Changed;
	}

	#region Commented Parsing optimizations
	//private bool isParsed;
	//private ParsedDocument documentCache;
	//private volatile static int walking;

	//private async void Buffer_Changed(object sender, TextContentChangedEventArgs e)
	//{
	//    if (TagsChangedIsNull() || Buffer.CurrentSnapshot == null || e.Changes.IsNullOrEmpty())
	//        return;

	//    if (e.After != Buffer.CurrentSnapshot)
	//        return;

	//    try
	//    {
	//        // If this isn't the most up-to-date version of the buffer, then ignore it for now (we'll eventually get another change event).               
	//        int min = Int32.MaxValue, max = Int32.MinValue;

	//        foreach (var change in e.Changes)
	//        {
	//            min = Math.Min(min, change.NewPosition);
	//            max = Math.Max(max, change.NewPosition + change.NewLength);
	//        }

	//        TextSpan span = new TextSpan(min, max); 
	//        var parsedDoc = await ParsedDocument.Resolve(Buffer, Buffer.CurrentSnapshot).ConfigureAwait(false);

	//        documentCache = parsedDoc;

	//        if (System.Threading.Interlocked.CompareExchange(ref walking, 1, comparand: 0) == 0)
	//        {
	//            WalkDocumentSyntaxTreeForTags(parsedDoc);
	//            RaiseTagsChanged();
	//            walking = 0;
	//        }
	//    }
	//    catch
	//    {

	//    }
	//}
	#endregion

	protected internal override void ResetCacheAndFlags(ITextSnapshot? newSnapshotToCache)
	{
		base.ResetCacheAndFlags(newSnapshotToCache);
		ClassificationTagsCache.Reset();
		OutliningsTagsCache.Reset();
	}

	/// <summary>
	/// Gets the tags asynchronously from the specified snapshot with Roslyn.
	/// </summary>
	/// <param name="spans">The spans for tagging. The current implementation doesn't take them into account and re-tags the entire document.</param>
	/// <returns>
	/// The current snapshot of the collected tags.
	/// </returns>
	public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
	{
		if (spans?.Count is null or 0 || AcuminatorVSPackage.Instance?.ColoringEnabled != true)
			return [];

		// If Roslyn workspace wasn't initialized yet, we need to try to initialize it.
		UpdateWorkspaceAndStateIfNeeded();

		if (RoslynWorkspace == null || !HasReferenceToAcumaticaPlatform)
			return [];

		ITextSnapshot newSnapshotToTag = spans[0].Snapshot;

		if (CheckIfRetaggingIsNotNecessary(newSnapshotToTag))
		{
			return ClassificationTagsCache.ProcessedTags;
		}

		if (BackgroundTagging != null)
		{
			BackgroundTagging.CancelTagging();   //Cancel currently running task
			BackgroundTagging = null;
		}

		ResetCacheAndFlags(newSnapshotToTag);
		BackgroundTagging = BackgroundTagging.StartBackgroundTagging(this);

		return ClassificationTagsCache.ProcessedTags;
	}

	private void UpdateWorkspaceAndStateIfNeeded()
	{
		Workspace? currentBufferWorkspace = Buffer.GetWorkspaceThatSupportsColoring();

		if (ReferenceEquals(RoslynWorkspace, currentBufferWorkspace))
			return;

		if (RoslynWorkspace != null)
			RoslynWorkspace.WorkspaceChanged -= OnWorkspaceChanged;

		RoslynWorkspace = currentBufferWorkspace;

		// If initialization was successful, we need to subscribe to workspace events and calculate the hasReferenceToAcumaticaPlatform flag.
		if (RoslynWorkspace != null)
		{
			RoslynWorkspace.WorkspaceChanged += OnWorkspaceChanged;
			_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(projectId: null);
		}
		else
		{
			_hasReferenceToAcumaticaPlatform = false;
		}
	}

	protected internal async Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementationAsync(ITextSnapshot snapshot,
																											 CancellationToken cToken)
	{
		ClassificationTagsCache.SetCancellation(cToken);
		OutliningsTagsCache.SetCancellation(cToken);

		Task<ParsedDocument?> getDocumentTask = ParsedDocument.ResolveAsync(snapshot, cToken);

		if (cToken.IsCancellationRequested)              // Razor cshtml returns a null document for some reason.
			return ClassificationTagsCache.ProcessedTags;

		var documentTaskResult = await getDocumentTask.TryAwait();

		if (!documentTaskResult.IsSuccess)
			return ClassificationTagsCache.ProcessedTags;

		ParsedDocument? document = documentTaskResult.Result;

		if (document == null || cToken.IsCancellationRequested)
			return ClassificationTagsCache.ProcessedTags;

		bool completedSuccessfully = await WalkDocumentSyntaxTreeForTagsOnThreadpoolAsync(document, cToken).TryAwait();
		LastTaggingWasSuccessful = completedSuccessfully && ClassificationTagsCache.IsCompleted;
		return ClassificationTagsCache.ProcessedTags;
	}

	private Task WalkDocumentSyntaxTreeForTagsOnThreadPoolAsync(ParsedDocument document, CancellationToken cancellationToken)
	{
		return Task.Factory.StartNew(() => WalkDocumentSyntaxTreeForTags(document, cancellationToken), 
									 cancellationToken, 
									 TaskCreationOptions.LongRunning, 
									 TaskScheduler.Default);
	}

	private void WalkDocumentSyntaxTreeForTags(ParsedDocument document, CancellationToken cancellationToken)
	{
		var syntaxWalker = new PXColorizerSyntaxWalker(this, document, cancellationToken);

		syntaxWalker.Visit(document.SyntaxRoot);
		ClassificationTagsCache.CompleteProcessing();
		OutliningsTagsCache.CompleteProcessing();
	}

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
