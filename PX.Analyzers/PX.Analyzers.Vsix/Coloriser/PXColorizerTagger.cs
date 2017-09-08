using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace PX.Analyzers.Coloriser
{
	internal class PXColorizerTagger : ITagger<IClassificationTag>
	{	
		//private readonly ConcurrentBag<ITagSpan<IClassificationTag>> tags = new ConcurrentBag<ITagSpan<IClassificationTag>>();
		private readonly List<ITagSpan<IClassificationTag>> tags = new List<ITagSpan<IClassificationTag>>();

		private IClassificationType dacType;
		private IClassificationType fieldType;
		private IClassificationType bqlParameterType;
		private IClassificationType bqlOperatorType;

		private ITextBuffer theBuffer;
		private ITextSnapshot cache;

#pragma warning disable CS0067
		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

		internal PXColorizerTagger(ITextBuffer buffer, IClassificationTypeRegistryService registry)
		{
			theBuffer = buffer;
			dacType = registry.GetClassificationType(Constants.DacFormat);
			fieldType = registry.GetClassificationType(Constants.DacFieldFormat);
			bqlParameterType = registry.GetClassificationType(Constants.BQLParameterFormat);
			bqlOperatorType = registry.GetClassificationType(Constants.BQLOperatorFormat);
		}

		public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (spans.Count == 0)
			{
				return Enumerable.Empty<ITagSpan<IClassificationTag>>();
			}

			if (cache != null && cache == spans[0].Snapshot)
				return tags;

			tags.Clear();
			//tagsList.Clear();
			cache = spans[0].Snapshot;
			GetTagsFromSnapShot(cache, spans);
			//tagsList.AddRange(tags);
			return tags;
		}

		private void GetTagsFromSnapShot(ITextSnapshot newSnapshot, NormalizedSnapshotSpanCollection spans)
		{
			string row = newSnapshot.GetText();
			var matches = Regex.Matches(row, RegExpressions.BQLSelectCommandPattern, RegexOptions.Singleline)
							   .OfType<Match>()
							   .Where(bqlCommandMatch => !string.IsNullOrWhiteSpace(bqlCommandMatch.Value));

			foreach (Match bqlCommandMatch in matches)
			{
				GetTagsFromBQLCommand(newSnapshot, bqlCommandMatch.Value, bqlCommandMatch.Index);
			}


			//Parallel.ForEach(matches,
			//				 bqlCommandMatch => GetTagsFromBQLCommand(newSnapshot, bqlCommandMatch.Value, bqlCommandMatch.Index));			
		}

		private void GetTagsFromBQLCommand(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			
			bqlCommand = bqlCommand.TrimStart('{');
			int firstCurlyBraces = bqlCommand.IndexOf('{');

			if (firstCurlyBraces >= 0)
			{
				bqlCommand = bqlCommand.Substring(0, firstCurlyBraces);
			}

			int firstSquareBrace = bqlCommand.IndexOf('[');

			if (firstSquareBrace >= 0)
			{
				bqlCommand = bqlCommand.Substring(0, firstSquareBrace);
			}

			int lastAngleBraces = bqlCommand.LastIndexOf('>');

			bqlCommand = bqlCommand.Substring(0, lastAngleBraces + 1);

			int firstAngleBraceIndex = bqlCommand.IndexOf('<');
			string selectOp = bqlCommand.Substring(0, firstAngleBraceIndex);

			if (!selectOp.Contains("Select") && !selectOp.Contains("Search"))
				return;

			int lastAngleBraceIndex = bqlCommand.LastIndexOf('>');
			bqlCommand = bqlCommand.Substring(0, lastAngleBraceIndex + 1);
			
			GetSelectCommandTag(newSnapshot, bqlCommand, offset);
			GetBQLOperandTags(newSnapshot, bqlCommand, offset);
			GetDacWithFieldTags(newSnapshot, bqlCommand, offset);
			GetSingleDacAndConstantTags(newSnapshot, bqlCommand, offset);
			GetBqlParameterTags(newSnapshot, bqlCommand, offset);
		}

		private void GetBQLOperandTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = Regex.Matches(bqlCommand, RegExpressions.DacOperandPattern);

			foreach (Match bqlOperandMatch in matches.OfType<Match>().Skip(1))
			{
				string bqlOperand = bqlOperandMatch.Value.TrimEnd('<');
				CreateTag(newSnapshot, bqlOperandMatch, offset, bqlOperand, bqlOperatorType);
			}
		}

		private void GetSelectCommandTag(ITextSnapshot newSnapshot, string selectOp, int offset)
		{
			Span span = new Span(offset, selectOp.Length);
			SnapshotSpan snapshotSpan = new SnapshotSpan(newSnapshot, span);
			var tag = new TagSpan<IClassificationTag>(snapshotSpan, new ClassificationTag(bqlOperatorType));
			tags.Add(tag);
		}

		private void GetBqlParameterTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = Regex.Matches(bqlCommand, RegExpressions.BQLParametersPattern);

			foreach (Match bqlParamMatch in matches)
			{
				string bqlParam = bqlParamMatch.Value;
				CreateTag(newSnapshot, bqlParamMatch, offset, bqlParam, bqlParameterType);
			}
		}

		private void GetSingleDacAndConstantTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = Regex.Matches(bqlCommand, RegExpressions.DacOrConstantPattern);

			foreach (Match dacOrConstMatch in matches)
			{
				string dacOrConst = dacOrConstMatch.Value.Trim(',', '<', '>');
				CreateTag(newSnapshot, dacOrConstMatch, offset, dacOrConst, dacType);
			}
		}

		private void GetDacWithFieldTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = Regex.Matches(bqlCommand, RegExpressions.DacWithFieldPattern);

			foreach (Match dacWithFieldMatch in matches)
			{
				string[] dacParts = dacWithFieldMatch.Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

				if (dacParts.Length != 2)
					continue;

				GetDacTag(newSnapshot, dacWithFieldMatch, offset, dacParts);
				GetFieldTag(newSnapshot, dacWithFieldMatch, offset, dacParts);
			}	
		}

		private void GetDacTag(ITextSnapshot newSnapshot, Match match, int offset, string[] dacParts)
		{
			string dacPart = dacParts[0].TrimStart(',', '<');
			CreateTag(newSnapshot, match, offset, dacPart, dacType);
		}

		private void GetFieldTag(ITextSnapshot newSnapshot, Match match, int offset, string[] dacParts)
		{
			string fieldPart = dacParts[1].TrimEnd(',', '>');
			CreateTag(newSnapshot, match, offset, fieldPart, fieldType);
		}

		private void CreateTag(ITextSnapshot newSnapshot, Match match, int offset, string tagContent, IClassificationType classType)
		{
			int startIndex = offset + match.Index + match.Value.IndexOf(tagContent);
			Span span = new Span(startIndex, tagContent.Length);
			SnapshotSpan snapshotSpan = new SnapshotSpan(newSnapshot, span);
			var tag = new TagSpan<IClassificationTag>(snapshotSpan, new ClassificationTag(classType));
			tags.Add(tag);
		}
	}
}
