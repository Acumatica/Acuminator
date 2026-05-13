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
	internal class PXOutliningTagger : PXTaggerBase, ITagger<IOutliningRegionTag>
	{
		private int _isSubscribed = NOT_SUBSCRIBED;
		private const int NOT_SUBSCRIBED = 0;
		private const int SUBSCRIBED = 1;

		protected PXRoslynColorizerTagger? ColorizerTagger { get; private set; }

		internal override bool LastTaggingWasSuccessful 
		{
			get => ColorizerTagger?.LastTaggingWasSuccessful ?? false;
			set { }
		}

		[MemberNotNullWhen(returnValue: true, nameof(ColorizerTagger))]
		public override bool HasReferenceToAcumaticaPlatform => ColorizerTagger?.HasReferenceToAcumaticaPlatform ?? false;

		public PXOutliningTagger(ITextBuffer buffer, bool subscribeToSettingsChanges, bool useCacheChecking) :
							base(buffer, subscribeToSettingsChanges, useCacheChecking)
		{
		}

		public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (spans == null || spans.Count == 0 || AcuminatorVSPackage.Instance?.UseBqlOutlining != true)
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

			return ColorizerTagger.OutliningsTagsCache.ProcessedTags;
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

		public override void Dispose()
		{
			if (Interlocked.Exchange(ref _isSubscribed, NOT_SUBSCRIBED) == SUBSCRIBED && ColorizerTagger != null)
			{
				ColorizerTagger.TagsChanged -= OnColorizingTaggerTagsChanged;
				ColorizerTagger = null;
			}

			base.Dispose();
		}
	} 
}
