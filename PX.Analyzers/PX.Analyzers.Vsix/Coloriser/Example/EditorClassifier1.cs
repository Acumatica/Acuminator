//------------------------------------------------------------------------------
// <copyright file="EditorClassifier1.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace PX.Analyzers.Coloriser
{
	/// <summary>
	/// Classifier that classifies all text as an instance of the "EditorClassifier1" classification type.
	/// </summary>
	internal class EditorClassifier1 : IClassifier
	{
		private ITextSnapshot prevSpan;
		private const string pattern = @"<?([A-Z][a-z]*)+\.([a-z]+[A-Z]*)+([>|,])?";
		private readonly List<ClassificationSpan> emptyList = new List<ClassificationSpan>();
		private readonly List<ClassificationSpan> spansList = new List<ClassificationSpan>();
		private bool analyzed;

		/// <summary>
		/// Classification type.
		/// </summary>
		private readonly IClassificationType classificationType;

		/// <summary>
		/// Initializes a new instance of the <see cref="EditorClassifier1"/> class.
		/// </summary>
		/// <param name="registry">Classification registry.</param>
		internal EditorClassifier1(IClassificationTypeRegistryService registry)
		{
			this.classificationType = registry.GetClassificationType("EditorClassifier1");
		}

		#region IClassifier

#pragma warning disable 67

		/// <summary>
		/// An event that occurs when the classification of a span of text has changed.
		/// </summary>
		/// <remarks>
		/// This event gets raised if a non-text change would affect the classification in some way,
		/// for example typing /* would cause the classification to change in C# without directly
		/// affecting the span.
		/// </remarks>
		public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

		/// <summary>
		/// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
		/// </summary>
		/// <remarks>
		/// This method scans the given SnapshotSpan for potential matches for this classification.
		/// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
		/// </remarks>
		/// <param name="span">The span currently being classified.</param>
		/// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
		public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
		{
			prevSpan = span.Snapshot;

			if (span == null)
				return emptyList;

			return AnalyzeForDacFieldsInSpan(span);
		}

		private List<ClassificationSpan> AnalyzeForDacFieldsInSpan(SnapshotSpan span)
		{
			analyzed = true;
			string row = span.Snapshot.GetText();
			var matches = Regex.Matches(row, pattern);

			if (matches.Count == 0)
				return emptyList;

			List<ClassificationSpan> res = new List<ClassificationSpan>();

			foreach (Match match in matches)
			{
				string[] dacParts = match.Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

				if (dacParts.Length != 2)
					continue;

				string fieldPart = dacParts[1].TrimEnd(',', '>');
				int startIndex = match.Index + dacParts[0].Length + 1;
				Span fieldSpan = new Span(startIndex, fieldPart.Length);
				SnapshotSpan snapshotSpan = new SnapshotSpan(span.Snapshot, fieldSpan);
				res.Add(new ClassificationSpan(snapshotSpan, classificationType));
			}		

			return res;
		}

		private int GetStartIndexFromMatch(Match match)
		{
			for (int i = 0; i < match.Length; i++)
			{
				if (char.IsUpper(match.Value[i]))
					return match.Index + i;
			}

			return -1;
		}

		#endregion
	}
}
