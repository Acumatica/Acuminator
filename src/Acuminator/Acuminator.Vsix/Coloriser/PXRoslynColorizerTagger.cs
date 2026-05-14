#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;

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

	internal override bool LastTaggingWasSuccessful { get; set; }

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
		var currentWorkspace = _roslynWorkspaceProvider.Workspace;

		if (currentWorkspace != null)
		{
			_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(currentWorkspace);
			currentWorkspace.WorkspaceChanged += OnWorkspaceChanged;
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

		Task<ParsedDocument?> getDocumentTask = ParsedDocument.ResolveAsync(snapshot, cToken);

		if (cToken.IsCancellationRequested)              // Razor cshtml returns a null document for some reason.
			return ClassificationTagsCache.ProcessedTags;

		var documentTaskResult = await getDocumentTask.TryAwait();

		if (!documentTaskResult.IsSuccess)
			return ClassificationTagsCache.ProcessedTags;

		ParsedDocument? document = documentTaskResult.Result;

		if (document == null || cToken.IsCancellationRequested)
			return ClassificationTagsCache.ProcessedTags;

		bool completedSuccessfully = await WalkDocumentSyntaxTreeForTagsOnThreadPoolAsync(document, cToken).TryAwait();
		LastTaggingWasSuccessful = completedSuccessfully && ClassificationTagsCache.IsCompleted;
		return ClassificationTagsCache.ProcessedTags;
	}

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
		BackgroundTagging?.Dispose();
		ClassificationTagsCache?.Reset();
		OutliningsTagsCache?.Reset();

		var workspace = _roslynWorkspaceProvider.Workspace;

		if (workspace != null)
			workspace.WorkspaceChanged -= OnWorkspaceChanged;

		_roslynWorkspaceProvider.WorkspaceChanged -= WorkspaceAttachedToDocumentChanged;
		_roslynWorkspaceProvider.Dispose();

		_hasReferenceToAcumaticaPlatform = false;
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
				_hasReferenceToAcumaticaPlatform |= CheckIfCurrentSolutionHasReferenceToAcumatica(_roslynWorkspaceProvider.Workspace);
				break;

			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionReloaded:
			case WorkspaceChangeKind.ProjectRemoved:
				_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(_roslynWorkspaceProvider.Workspace);
				break;

			case WorkspaceChangeKind.ProjectChanged:
			case WorkspaceChangeKind.ProjectReloaded:
				if (e.IsProjectMetadataChanged())
				{
					_hasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica(_roslynWorkspaceProvider.Workspace);
				}

				break;

			default:
				return;
		}

		if (oldValue != _hasReferenceToAcumaticaPlatform)
		{
			ResetCacheAndFlags(newSnapshotToCache: null);
			ThreadHelper.JoinableTaskFactory.Run(RaiseTagsChangedAsync);
		}	
	}

	private void WorkspaceAttachedToDocumentChanged(object sender, DocumentWorkspaceChangedEventArgs e)
	{
		if (e.OldWorkspace != null)
			e.OldWorkspace.WorkspaceChanged -= OnWorkspaceChanged;

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

		// We need to raise the tags changed event to trigger re-coloring on workspace change
		ResetCacheAndFlags(newSnapshotToCache: null);
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

	private static bool IsAcumaticaAssemblyName(MetadataReference reference)
	{
		string referenceName = Path.GetFileNameWithoutExtension(reference.Display);
		return IsAcumaticaAssemblyName(referenceName);
	}

	private static bool IsAcumaticaAssemblyName(string dllName) => ColoringConstants.PlatformDllName == dllName ||
																   ColoringConstants.AppDllName == dllName;
}
