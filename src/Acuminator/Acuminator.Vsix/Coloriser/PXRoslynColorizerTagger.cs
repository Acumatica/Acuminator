#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
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
internal partial class PXRoslynColorizerTagger : PXTaggerBase, ITagger<IClassificationTag>
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
	private readonly SourceTextContainer _cachedTextContainer;

	public PXRoslynColorizerTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, PXColorizerTaggerProvider provider,
								   bool subscribeToSettingsChanges, bool useCacheChecking) :
							  base(buffer, textDocumentFactory, subscribeToSettingsChanges, useCacheChecking)
	{
		Provider = provider.CheckIfNull();
		_cachedTextContainer = Buffer.AsTextContainer();
		ClassificationTagsCache = new TagsCacheAsync<IClassificationTag>();
		OutliningsTagsCache = new TagsCacheAsync<IOutliningRegionTag>();

		// Roslyn workspace provider creation and subscription to workspace events should be done under sync lock to avoid
		// unlikely race condition in the constructor.
		// The lock is expected to be re-entrant, the Monitor synchronization should not be changed to another synchronization mechanism without a rework.
		object workspaceLock = new object();

		lock (workspaceLock)
		{
			_roslynWorkspaceProvider = RoslynWorkspaceProvider.Create(_cachedTextContainer, workspaceLock);
			_roslynWorkspaceProvider.WorkspaceChanged += WorkspaceAttachedToDocumentChanged;

			// Drive initial setup through the same code path as change events.
			WorkspaceAttachedToDocumentChanged(this,
				new DocumentWorkspaceChangedEventArgs(oldWorkspace: null, newWorkspace: _roslynWorkspaceProvider.Workspace));
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

	protected override void CleanupOnTextDocumentDisposed(object sender, EventArgs e)
	{
		base.CleanupOnTextDocumentDisposed(sender, e);

		lock (_roslynWorkspaceProvider.WorkspaceSubscriptionLocker)
		{
			_roslynWorkspaceProvider.WorkspaceChanged -= WorkspaceAttachedToDocumentChanged;
			var workspaceToUnsubscribe = _roslynWorkspaceProvider.Workspace;

			if (workspaceToUnsubscribe != null)
				workspaceToUnsubscribe.WorkspaceChanged -= OnWorkspaceChanged;

			_roslynWorkspaceProvider.Dispose();
		}
		
		BackgroundTagging?.Dispose();
		ClassificationTagsCache.Reset();
		OutliningsTagsCache.Reset();

		_hasReferenceToAcumaticaPlatform = false;
	}

	private void WorkspaceAttachedToDocumentChanged(object sender, DocumentWorkspaceChangedEventArgs e)
	{
		lock (_roslynWorkspaceProvider.WorkspaceSubscriptionLocker)
		{
			if (e.OldWorkspace != null)
				e.OldWorkspace.WorkspaceChanged -= OnWorkspaceChanged;

			// Defensive check in case workspace changed event fired by different threads in a quick succession
			// We check under lock that we subscribe to the latest workspace
			if (!ReferenceEquals(_roslynWorkspaceProvider.Workspace, e.NewWorkspace))
				return;

			// if new Workspace supports coloring, we need to subscribe to workspace events and calculate the hasReferenceToAcumaticaPlatform flag.
			if (e.NewWorkspace != null)
			{
				e.NewWorkspace.WorkspaceChanged += OnWorkspaceChanged;
				_hasReferenceToAcumaticaPlatform = CalculateAcumaticaReferenceForDocumentInSolution(e.NewWorkspace.CurrentSolution);
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
				_hasReferenceToAcumaticaPlatform = CalculateAcumaticaReferenceForDocumentInSolution(e.NewSolution);
				break;

			case WorkspaceChangeKind.ProjectChanged:
			case WorkspaceChangeKind.ProjectReloaded:
				_hasReferenceToAcumaticaPlatform = GetAcumaticaReferenceOnProjectChange(e, oldHasReferenceToAcumaticaPlatform);
				break;

			case WorkspaceChangeKind.ProjectAdded:
				// If we already have a reference to Acumatica platform, adding another project can't change that fact, so we can skip the check
				if (!oldHasReferenceToAcumaticaPlatform)   
					_hasReferenceToAcumaticaPlatform = GetAcumaticaReferenceOnProjectChange(e, oldHasReferenceToAcumaticaPlatform);

				break;

			case WorkspaceChangeKind.ProjectRemoved:
				// Recalculate only if we had a reference to Acumatica platform before, otherwise it can't be a change that would add the reference back
				if (oldHasReferenceToAcumaticaPlatform)
					_hasReferenceToAcumaticaPlatform = GetAcumaticaReferenceOnProjectDelete(e, oldHasReferenceToAcumaticaPlatform);

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
		var documentProjectNew = GetCurrentDocumentProject(e.NewSolution);

		if (documentProjectNew == null)
			return false;

		if (e.ProjectId == null)
		{
			// In case the changed project is unknown, we need to recalculate the reference to Acumatica platform for the document project
			return CalculateAcumaticaReferenceForProject(documentProjectNew);
		}

		var changedProjectNew = e.NewSolution.GetProject(e.ProjectId);

		if (changedProjectNew == null || changedProjectNew.Id.Equals(documentProjectNew.Id))
			return CalculateAcumaticaReferenceForProject(documentProjectNew);
		else if (documentProjectNew.AllProjectReferences.Count == 0)
			return oldHasReferenceToAcumaticaPlatform;

		// Do a BFS collection of the referenced projects. In practice, there should not be many projects referenced by the document project.
		var docProjectWithAllReferencedProjects = documentProjectNew.GetAllReferencedProjectsAndThis();
		bool changedProjectAffectsColoring = docProjectWithAllReferencedProjects.Any(project => project.Id.Equals(changedProjectNew.Id));

		return changedProjectAffectsColoring
			? docProjectWithAllReferencedProjects.Any(CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata)
			: oldHasReferenceToAcumaticaPlatform;
	}

	private bool GetAcumaticaReferenceOnProjectDelete(WorkspaceChangeEventArgs e, bool oldHasReferenceToAcumaticaPlatform)
	{
		var documentProjectNew = GetCurrentDocumentProject(e.NewSolution);

		if (documentProjectNew == null)
			return false;

		if (e.ProjectId == null)
		{
			// In case the changed project is unknown, we need to recalculate the reference to Acumatica platform for the document project
			return CalculateAcumaticaReferenceForProject(documentProjectNew);
		}

		var deletedProjectOld = e.OldSolution.GetProject(e.ProjectId);
		var documentProjectOld = GetCurrentDocumentProject(e.OldSolution);

		if (deletedProjectOld == null || documentProjectOld == null)
			return CalculateAcumaticaReferenceForProject(documentProjectNew);
		else if (documentProjectOld.Id.Equals(deletedProjectOld.Id))
			return false;
		else if (documentProjectOld.AllProjectReferences.Count == 0)
			return oldHasReferenceToAcumaticaPlatform;

		// Do a BFS collection of the referenced projects. In practice, there should not be many projects referenced by the document project.
		var docProjectWithAllReferencedProjectsOld = documentProjectOld.GetAllReferencedProjectsAndThis();
		bool deletedProjectAffectsColoring = docProjectWithAllReferencedProjectsOld.Any(project => project.Id.Equals(deletedProjectOld.Id));

		if (!deletedProjectAffectsColoring)
			return oldHasReferenceToAcumaticaPlatform;

		// Due to a possibility of complex scenarios that may appear on project deletion (for example, other projects are referenced by deleted project and became unavailable)
		// we need to re-collect the referenced projects for the document project in new solution.
		var docProjectWithAllReferencedProjectsNew = documentProjectNew.GetAllReferencedProjectsAndThis();
		return docProjectWithAllReferencedProjectsNew.Any(CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata);
	}

	private bool CalculateAcumaticaReferenceForDocumentInSolution(Solution? solution)
	{
		if (solution == null || solution.ProjectIds.Count == 0)
			return false;

		var documentProjectNew = GetCurrentDocumentProject(solution);
		return documentProjectNew != null && CalculateAcumaticaReferenceForProject(documentProjectNew);
	}

	private bool CalculateAcumaticaReferenceForProject(Project project)
	{
		if (CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata(project))
			return true;
		else if (project.AllProjectReferences.Count == 0)
			return false;
		else
			return project.GetAllReferencedProjects().Any(CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata);
	}

	private Project? GetCurrentDocumentProject(Solution solution)
	{
		var documentID = solution.Workspace.GetDocumentIdInCurrentContext(_cachedTextContainer);

		if (documentID?.ProjectId == null)
			return null;

		return solution.GetProject(documentID.ProjectId);
	}

	private bool CheckIfProjectHasReferenceToAcumaticaInNameOrMetadata(Project project) =>
		project.MetadataReferences.Any(IsAcumaticaAssemblyName) ||
		IsAcumaticaAssemblyName(project);

	private static bool IsAcumaticaAssemblyName(Project project) =>
		IsAcumaticaAssemblyName(project.Name) || IsAcumaticaAssemblyName(project.AssemblyName);

	private static bool IsAcumaticaAssemblyName(MetadataReference reference)
	{
		string referenceName = Path.GetFileNameWithoutExtension(reference.Display);
		return IsAcumaticaAssemblyName(referenceName);
	}

	private static bool IsAcumaticaAssemblyName(string dllName) => ColoringConstants.PlatformDllName == dllName ||
																   ColoringConstants.AppDllName == dllName;
}
