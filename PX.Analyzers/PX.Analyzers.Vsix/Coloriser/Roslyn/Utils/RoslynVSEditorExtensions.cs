using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using PX.Analyzers.Vsix.Utilities;


namespace PX.Analyzers.Coloriser
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
                collapsedText = Constants.DefaultCollapsedRegionText;

            if (toolTipText.IsNullOrWhiteSpace())
                toolTipText = snapshot.GetText(span);

            return new TagSpan<IOutliningRegionTag>(
              new SnapshotSpan(snapshot, span.Start, span.Length),
              new OutliningRegionTag(isDefaultCollapsed: false, isImplementation: false,
                                     collapsedForm: collapsedText, collapsedHintForm: toolTipText));
        }


        public static string GetText(this ITextSnapshot snapshot, TextSpan span) => snapshot.GetText(span.Start, span.Length);
	}
}
