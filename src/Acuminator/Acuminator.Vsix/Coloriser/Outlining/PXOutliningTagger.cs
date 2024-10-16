﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

using Shell = Microsoft.VisualStudio.Shell;

namespace Acuminator.Vsix.Coloriser
{
	public class PXOutliningTagger : PXTaggerBase, ITagger<IOutliningRegionTag>
	{
		private int _isSubscribed = NOT_SUBSCRIBED;
		private const int NOT_SUBSCRIBED = 0;
		private const int SUBSCRIBED = 1;

		public override TaggerType TaggerType => TaggerType.Outlining;

		protected PXOutliningTaggerProvider Provider => (ProviderBase as PXOutliningTaggerProvider)!;

		protected PXColorizerTaggerBase? ColorizerTagger { get; private set; }

		public PXOutliningTagger(ITextBuffer buffer, PXOutliningTaggerProvider aProvider,
								 bool subscribeToSettingsChanges, bool useCacheChecking) :
							base(buffer, aProvider, subscribeToSettingsChanges, useCacheChecking)
		{
		}

		public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (spans == null || spans.Count == 0 || AcuminatorVSPackage.Instance?.UseBqlOutlining != true)
				return [];

			if (ColorizerTagger == null)
			{
				if (!TryGetColorizingTaggerFromBuffer(Buffer, out PXColorizerTaggerBase colorizingTagger) || colorizingTagger == null)
					return [];

				SubscribeToColorizingTaggerEvents(colorizingTagger);
			}

			switch (ColorizerTagger?.TaggerType)
			{
				case TaggerType.General when AcuminatorVSPackage.Instance?.UseRegexColoring == true:
				case TaggerType.RegEx:
				case null:
					return [];
			}

			return ColorizerTagger.OutliningsTagsCache.ProcessedTags;
		}

		private static bool TryGetColorizingTaggerFromBuffer(ITextBuffer textBuffer, out PXColorizerTaggerBase colorizingTagger)
		{
			return textBuffer.Properties.TryGetProperty(typeof(PXColorizerTaggerBase), out colorizingTagger);
		}

		private void SubscribeToColorizingTaggerEvents(PXColorizerTaggerBase colorizerTagger)
		{
			if (colorizerTagger.TaggerType == TaggerType.RegEx)
				return;

			if (Interlocked.Exchange(ref _isSubscribed, SUBSCRIBED) == NOT_SUBSCRIBED)
			{
				ColorizerTagger = colorizerTagger;
				ColorizerTagger.TagsChanged += OnColorizingTaggerTagsChanged;
			}
		}

		private void OnColorizingTaggerTagsChanged(object sender, SnapshotSpanEventArgs e)
		{
			Shell.ThreadHelper.JoinableTaskFactory.Run(RaiseTagsChangedAsync);
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
