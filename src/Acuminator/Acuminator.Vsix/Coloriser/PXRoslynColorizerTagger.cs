#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Settings;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

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

	private volatile bool _hasReferenceToAcumaticaPlatform;

	public sealed override bool HasReferenceToAcumaticaPlatform => _hasReferenceToAcumaticaPlatform;

	private volatile bool _lastTaggingWasSuccessful;

	internal bool LastTaggingWasSuccessful
	{
		get => _lastTaggingWasSuccessful;
		set => _lastTaggingWasSuccessful = value;
	}

	private readonly RoslynWorkspaceProvider _roslynWorkspaceProvider;

	public PXRoslynColorizerTagger(ITextBuffer buffer, PXColorizerTaggerProvider provider, bool subscribeToSettingsChanges,
									bool useCacheChecking) :
							  base(buffer, subscribeToSettingsChanges, useCacheChecking)
	{
		Provider = provider.CheckIfNull();
		ClassificationTagsCache = new TagsCacheAsync<IClassificationTag>();
		OutliningsTagsCache = new TagsCacheAsync<IOutliningRegionTag>();

		_roslynWorkspaceProvider = new RoslynWorkspaceProvider(buffer);
		_roslynWorkspaceProvider.WorkspaceChanged += WorkspaceAttachedToDocumentChanged;

		// Drive initial setup through the same code path as change events.
		// If a real change fires between the subscribe above and this call, the handler is idempotent
		// (guarded by _subscribedWorkspace) — whichever invocation runs second is a no-op.
		WorkspaceAttachedToDocumentChanged(this,
			new DocumentWorkspaceChangedEventArgs(oldWorkspace: null, newWorkspace: _roslynWorkspaceProvider.Workspace));

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

	protected override void ColoringSettingChangedHandler(object sender, SettingChangedEventArgs e)
	{
		LastTaggingWasSuccessful = false;

		base.ColoringSettingChangedHandler(sender, e);
	}

	protected internal override void ResetCacheAndFlags(ITextSnapshot? newSnapshotToCache)
	{
		base.ResetCacheAndFlags(newSnapshotToCache);

		LastTaggingWasSuccessful = false;
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
		if (spans?.Count is null or 0 || AcuminatorVSPackage.Instance?.ColoringEnabled != true || !HasReferenceToAcumaticaPlatform)
			return [];

		var workspace = _roslynWorkspaceProvider.Workspace; 
		
		if (workspace == null)
			return [];

		ITextSnapshot newSnapshotToTag = spans[0].Snapshot;

		if (CheckIfParsingAndRetaggingIsNotNecessary(newSnapshotToTag))
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

	protected virtual bool CheckIfParsingAndRetaggingIsNotNecessary(ITextSnapshot newSnapshotToTag) =>
		CacheCheckingEnabled && Snapshot != null && Snapshot == newSnapshotToTag && !ColoringSettingsChanged &&
		(LastTaggingWasSuccessful || BackgroundTagging?.IsTaskRunning() == true);

	protected internal async Task<IEnumerable<ITagSpan<IClassificationTag>>> GetTagsAsyncImplementationAsync(ITextSnapshot snapshot,
																											 CancellationToken cToken)
	{
		ClassificationTagsCache.SetCancellation(cToken);
		OutliningsTagsCache.SetCancellation(cToken);

		Workspace? workspace = _roslynWorkspaceProvider.Workspace;

		if (workspace == null || cToken.IsCancellationRequested)
		{
			LastTaggingWasSuccessful = false;
			return [];
		}

		Task<(ParsedDocument? Parsed, bool TaggingSupported)> getDocumentTask = 
			ParsedDocument.ResolveAsync(snapshot, workspace, cToken);  // Razor cshtml returns a null document for some reason.

		var documentTaskResult = await getDocumentTask.TryAwait(LogError);

		if (!documentTaskResult.IsSuccess)
		{
			LastTaggingWasSuccessful = false;
			return [];
		}

		var (document, taggingSupported) = documentTaskResult.Result;

		if (document == null || cToken.IsCancellationRequested)
		{
			// If the text buffer doesn't support tagging, we can mark tagging as successful to avoid repeated attempts
			LastTaggingWasSuccessful = !taggingSupported;
			return [];
		}

		bool completedSuccessfully = await WalkDocumentSyntaxTreeForTagsOnThreadPoolAsync(document, cToken).TryAwait(LogError);
		LastTaggingWasSuccessful = completedSuccessfully && ClassificationTagsCache.IsCompleted;
		return ClassificationTagsCache.ProcessedTags;
	}

	private static void LogError(Exception exception) =>
		AcuminatorVSPackage.Instance?.AcuminatorLogger?.LogException(exception, logOnlyFromAcuminatorAssemblies: true,
																	 Logger.LogMode.Warning);

	private Task WalkDocumentSyntaxTreeForTagsOnThreadPoolAsync(ParsedDocument document, CancellationToken cancellationToken)
	{
		return Task.Run(() => WalkDocumentSyntaxTreeForTags(document, cancellationToken));
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
		lock (_roslynWorkspaceProvider.WorkspaceSubscriptionLocker)
		{
			_roslynWorkspaceProvider.WorkspaceChanged -= WorkspaceAttachedToDocumentChanged;
			var workspaceToUnsubscribe = _roslynWorkspaceProvider.Workspace;

			if (workspaceToUnsubscribe != null)
				workspaceToUnsubscribe.WorkspaceChanged -= OnWorkspaceChanged;
		}
		

		_roslynWorkspaceProvider.Dispose();
		BackgroundTagging?.Dispose();
		ClassificationTagsCache.Reset();
		OutliningsTagsCache.Reset();

		_hasReferenceToAcumaticaPlatform = false;

		base.Dispose();
	}

	private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
	{
		bool oldHasReferenceToAcumaticaPlatform = _hasReferenceToAcumaticaPlatform;

		switch (e.Kind)
		{
			case WorkspaceChangeKind.SolutionRemoved:
			case WorkspaceChangeKind.SolutionCleared:
				_hasReferenceToAcumaticaPlatform = false;
				break;

			// Solution replaced entirely — always re-check.
			case WorkspaceChangeKind.SolutionAdded:
			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionReloaded:
				_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(e.NewSolution.Workspace);
				break;

			case WorkspaceChangeKind.ProjectChanged:
			case WorkspaceChangeKind.ProjectReloaded:
				_hasReferenceToAcumaticaPlatform = GetAcumaticaReferenceOnProjectChange(e, oldHasReferenceToAcumaticaPlatform);
				break;

			case WorkspaceChangeKind.ProjectAdded:
				// If we already have a reference to Acumatica platform, adding another project can't change that fact, so we can skip the check
				if (!oldHasReferenceToAcumaticaPlatform)   
					_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(e.NewSolution.Workspace);

				break;

			case WorkspaceChangeKind.ProjectRemoved:
				// Recalculate only if we had a reference to Acumatica platform before, otherwise it can't be a change that would add the reference back
				if (oldHasReferenceToAcumaticaPlatform)
					_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(e.NewSolution.Workspace);

				break;

			default:
				return;
		}

		if (oldHasReferenceToAcumaticaPlatform != _hasReferenceToAcumaticaPlatform)
		{
			ResetCacheAndFlags(newSnapshotToCache: null);

			if (ThreadHelper.CheckAccess())
				RaiseTagsChanged();
			else
				ThreadHelper.JoinableTaskFactory.Run(RaiseTagsChangedAsync);
		}	
	}

	private bool GetAcumaticaReferenceOnProjectChange(WorkspaceChangeEventArgs e, bool oldHasReferenceToAcumaticaPlatform)
	{
		if (e.ProjectId == null)
			return CheckIfCurrentSolutionHasReferenceToAcumatica(e.NewSolution.Workspace);

		var changedProject = e.NewSolution.GetProject(e.ProjectId);

		if (changedProject == null)
			return false;

		var textContainer = Buffer.AsTextContainer();
		var documentID = e.NewSolution.Workspace.GetDocumentIdInCurrentContext(textContainer);

		if (!e.ProjectId.Equals(documentID?.ProjectId))
			return oldHasReferenceToAcumaticaPlatform;

		// Check for the project if the changed project has reference to Acumatica platform in its metadata or name.
		if (CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata(changedProject))
			return true;

		// Do a BFS among the referenced projects. In practice, there should not be many projects referenced by the changed project.
		// It's better that doing a full solution scan.
		var visitedProjects = new HashSet<ProjectId>();
		var referencedProjects = GetReferencedProjects(changedProject);
		var projectsToVisit = new Queue<Project>(referencedProjects!);

		while (projectsToVisit.Count > 0)
		{
			var currentProject = projectsToVisit.Dequeue();

			if (!visitedProjects.Add(currentProject.Id))
				continue;

			if (CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata(currentProject))
				return true;

			var currentReferencedProjects = GetReferencedProjects(currentProject);

			foreach (var refProject in currentReferencedProjects)
			{
				if (!visitedProjects.Contains(refProject.Id))
					projectsToVisit.Enqueue(refProject);
			}
		}

		return false;
	}

	private bool CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata(Project project) =>
		project.MetadataReferences.Any(IsAcumaticaAssemblyName) ||
		IsAcumaticaAssemblyName(project);

	private void WorkspaceAttachedToDocumentChanged(object sender, DocumentWorkspaceChangedEventArgs e)
	{
		lock (_roslynWorkspaceProvider.WorkspaceSubscriptionLocker)
		{
			if (e.OldWorkspace != null)
				e.OldWorkspace.WorkspaceChanged -= OnWorkspaceChanged;

			// Defensive check in case workspace changed event fired by different threads in a quick succession
			// We check under lock that we subscribe to the latest namespace
			if (!ReferenceEquals(_roslynWorkspaceProvider.Workspace, e.NewWorkspace))
				return;

			// if new Workspace supports coloring, we need to subscribe to workspace events and calculate the hasReferenceToAcumaticaPlatform flag.
			if (e.NewWorkspace != null)
			{
				e.NewWorkspace.WorkspaceChanged += OnWorkspaceChanged;
				_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(e.NewWorkspace);
			}
			else
			{
				_hasReferenceToAcumaticaPlatform = false;
			}
		}

		// We need to raise the tags changed event to trigger re-coloring on workspace change
		ResetCacheAndFlags(newSnapshotToCache: null);

		if (ThreadHelper.CheckAccess())
			RaiseTagsChanged();
		else
			ThreadHelper.JoinableTaskFactory.Run(RaiseTagsChangedAsync);
	}

	protected bool CheckIfCurrentSolutionHasReferenceToAcumatica(Workspace? workspace)
	{
		var currentSolution = workspace?.CurrentSolution;

		if (currentSolution == null || currentSolution.ProjectIds.Count == 0)
			return false;

		bool hasReferenceInMetadata = currentSolution.Projects.SelectMany(project => project.MetadataReferences)
															  .Any(IsAcumaticaAssemblyName);
		if (hasReferenceInMetadata)
			return true;

		bool hasAcumaticaProjectsInSolution =
			currentSolution.Projects.Any(project => IsAcumaticaAssemblyName(project.Name) || IsAcumaticaAssemblyName(project.AssemblyName));

		return hasAcumaticaProjectsInSolution;
	}

	private static bool IsAcumaticaAssemblyName(Project project) =>
		IsAcumaticaAssemblyName(project.Name) || IsAcumaticaAssemblyName(project.AssemblyName);

	private static bool IsAcumaticaAssemblyName(MetadataReference reference)
	{
		string referenceName = Path.GetFileNameWithoutExtension(reference.Display);
		return IsAcumaticaAssemblyName(referenceName);
	}

	private static bool IsAcumaticaAssemblyName(string dllName) => ColoringConstants.PlatformDllName == dllName ||
																   ColoringConstants.AppDllName == dllName;

	private static IEnumerable<Project> GetReferencedProjects(Project project)
	{
		if (project.AllProjectReferences.Count == 0)
			return [];

		return project.AllProjectReferences
					  .Select(projectReference => project.Solution.GetProject(projectReference.ProjectId))
					  .Where(project => project != null)!;
	}
}
