using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Analyzers.Coloriser
{
	public static class RoslynVSEditorExtensions
	{
		public static ITagSpan<IClassificationTag> ToClassificationTagSpan(this TextSpan span, ITextSnapshot snapshot, IClassificationType classificationType)
		{
			return new TagSpan<IClassificationTag>(
			  new SnapshotSpan(snapshot, span.Start, span.Length),
			  new ClassificationTag(classificationType));
		}

        public static ITagSpan<IOutliningRegionTag> ToOutliningTagSpan(this TextSpan span, ITextSnapshot snapshot, 
                                                                       string collapsedText = null, 
                                                                       string toolTipText = null)
        {
            if (collapsedText.IsNullOrWhiteSpace())
                collapsedText = OutliningConstants.DefaultCollapsedBQLRegionText;

            if (toolTipText.IsNullOrWhiteSpace())
                toolTipText = snapshot.GetText(span);

            if (toolTipText.Length > OutliningConstants.HintTooltipMaxLength)
                toolTipText = toolTipText.Substring(OutliningConstants.HintTooltipMaxLength) + OutliningConstants.SuffixForTooLongTooltips;

            return new TagSpan<IOutliningRegionTag>(
              new SnapshotSpan(snapshot, span.Start, span.Length),
              new OutliningRegionTag(isDefaultCollapsed: false, isImplementation: false,
                                     collapsedForm: collapsedText, collapsedHintForm: toolTipText));
        }


        public static string GetText(this ITextSnapshot snapshot, TextSpan span) => snapshot.GetText(span.Start, span.Length);
	}
}
