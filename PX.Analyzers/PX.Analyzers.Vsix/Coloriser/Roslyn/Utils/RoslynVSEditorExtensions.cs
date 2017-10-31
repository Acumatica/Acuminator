using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;

namespace PX.Analyzers.Coloriser
{
	public static class RoslynVSEditorExtensions
	{
		public static ITagSpan<IClassificationTag> ToTagSpan(this TextSpan span, ITextSnapshot snapshot, IClassificationType classificationType)
		{
			return new TagSpan<IClassificationTag>(
			  new SnapshotSpan(snapshot, span.Start, span.Length),
			  new ClassificationTag(classificationType));
		}


		public static String GetText(this ITextSnapshot snapshot, TextSpan span) => snapshot.GetText(span.Start, span.Length);
	}
}
