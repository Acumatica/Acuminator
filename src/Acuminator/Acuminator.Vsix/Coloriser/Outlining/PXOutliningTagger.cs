#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Acuminator.Vsix.Coloriser
{
	/// <summary>
	/// An outlining tagger. Delegates the collection of outlining tags to the <see cref="PXRoslynColorizerTagger"/>.<br/>
	/// Subscribes to the <see cref="PXRoslynColorizerTagger.TagsChanged"/> event to raise its own <see cref="ITagger{T}.TagsChanged"/> event when the colorizing tagger's tags change.
	/// </summary>
	internal class PXOutliningTagger : PXTaggerBase, ITagger<IOutliningRegionTag>
	{
		private int _isSubscribed = NOT_SUBSCRIBED;
		private const int NOT_SUBSCRIBED = 0;
		private const int SUBSCRIBED = 1;

		protected PXRoslynColorizerTagger? ColorizerTagger { get; private set; }

		[MemberNotNullWhen(returnValue: true, nameof(ColorizerTagger))]
		public override bool HasReferenceToAcumaticaPlatform => ColorizerTagger?.HasReferenceToAcumaticaPlatform ?? false;

		public PXOutliningTagger(ITextBuffer buffer, ITextDocumentFactoryService textDocumentFactory, bool subscribeToSettingsChanges, 
								 bool useCacheChecking) :
							base(buffer, textDocumentFactory, subscribeToSettingsChanges, useCacheChecking)
		{
		}

		public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection requestedSpans)
		{
			if (requestedSpans?.Count is null or 0 || AcuminatorVSPackage.Instance?.UseBqlOutlining != true)
				return [];

			if (ColorizerTagger == null)
			{
				if (!TryGetColorizingTaggerFromBuffer(Buffer, out PXRoslynColorizerTagger colorizingTagger) || colorizingTagger == null)
					return [];

				SubscribeToColorizingTaggerEvents(colorizingTagger);
			}

			// Check reference to Acumatica platform only after initializing ColorizerTagger
			if (!HasReferenceToAcumaticaPlatform)
				return [];

			var processedTags = ColorizerTagger.OutliningsTagsCache.ProcessedTags;
			return GetIntersectionWithRequestedTags(processedTags, requestedSpans);
		}

		private static bool TryGetColorizingTaggerFromBuffer(ITextBuffer textBuffer, out PXRoslynColorizerTagger colorizingTagger)
		{
			return textBuffer.Properties.TryGetProperty(typeof(PXRoslynColorizerTagger), out colorizingTagger);
		}

		private void SubscribeToColorizingTaggerEvents(PXRoslynColorizerTagger colorizerTagger)
		{
			if (Interlocked.Exchange(ref _isSubscribed, SUBSCRIBED) == NOT_SUBSCRIBED)
			{
				ColorizerTagger = colorizerTagger;
				ColorizerTagger.TagsChanged += OnColorizingTaggerTagsChanged;
			}
		}

		private void OnColorizingTaggerTagsChanged(object sender, SnapshotSpanEventArgs e)
		{
			RaiseTagsChanged();
		}

		protected override void CleanupOnTextDocumentDisposed(object sender, EventArgs e)
		{
			base.CleanupOnTextDocumentDisposed(sender, e);

			if (Interlocked.Exchange(ref _isSubscribed, NOT_SUBSCRIBED) == SUBSCRIBED && ColorizerTagger != null)
			{
				ColorizerTagger.TagsChanged -= OnColorizingTaggerTagsChanged;
				ColorizerTagger = null;
			}
		}
	} 
}
