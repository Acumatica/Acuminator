using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Interlocked = System.Threading.Interlocked;

namespace PX.Analyzers.Coloriser
{
	internal class PXColorizerTagger : ITagger<IClassificationTag>
	{
        private const string PreprocessorText = "preprocessor text";
		private readonly ConcurrentBag<ITagSpan<IClassificationTag>> tags = new ConcurrentBag<ITagSpan<IClassificationTag>>();
		private readonly List<ITagSpan<IClassificationTag>> tagsList = new List<ITagSpan<IClassificationTag>>();

		private IClassificationType dacType;
		private IClassificationType fieldType;
		private IClassificationType bqlParameterType;
		private IClassificationType bqlOperatorType;

        private ITextBuffer theBuffer;
		private ITextSnapshot cache;

#pragma warning disable CS0067
		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

		
		private static int prioiryIncreased;

		internal PXColorizerTagger(ITextBuffer buffer, IClassificationTypeRegistryService registry, IClassificationFormatMap formatMap)
		{            
            theBuffer = buffer;
			dacType = registry.GetClassificationType(Constants.DacFormat);
			fieldType = registry.GetClassificationType(Constants.DacFieldFormat);
			bqlParameterType = registry.GetClassificationType(Constants.BQLParameterFormat);
			bqlOperatorType = registry.GetClassificationType(Constants.BQLOperatorFormat);

			if (Interlocked.CompareExchange(ref prioiryIncreased, 1, 0) == 0)
			{
				IncreaseServiceFormatPriority(formatMap, registry, PredefinedClassificationTypeNames.Comment);
			}
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
			tagsList.Clear();
			cache = spans[0].Snapshot;
			GetTagsFromSnapShot(cache, spans);
			tagsList.AddRange(tags);
			return tags;
		}

        private void IncreaseServiceFormatPriority(IClassificationFormatMap formatMap, IClassificationTypeRegistryService registry, string formatName)
        {
            IClassificationType predefinedClassificationType = registry.GetClassificationType(formatName);
            IClassificationType artificialClassType = registry.CreateTransientClassificationType(predefinedClassificationType);
            TextFormattingRunProperties properties = formatMap.GetExplicitTextProperties(predefinedClassificationType);
           
            formatMap.AddExplicitTextProperties(artificialClassType, properties, bqlParameterType);
            formatMap.SwapPriorities(artificialClassType, predefinedClassificationType);
            formatMap.SwapPriorities(bqlParameterType, predefinedClassificationType);
        }

		private void GetTagsFromSnapShot(ITextSnapshot newSnapshot, NormalizedSnapshotSpanCollection spans)
		{
			string wholeText = newSnapshot.GetText();
            var matches = RegExpressions.BQLSelectCommandRegex
                                        .Matches(wholeText)
							            .OfType<Match>()
							            .Where(bqlCommandMatch => !string.IsNullOrWhiteSpace(bqlCommandMatch.Value));

            Parallel.ForEach(matches, 
                             bqlCommandMatch => GetTagsFromBQLCommand(newSnapshot, bqlCommandMatch.Value, bqlCommandMatch.Index));
        }

		private void GetTagsFromBQLCommand(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{			
			int lastAngleBraces = bqlCommand.LastIndexOf('>');
			bqlCommand = bqlCommand.Substring(0, lastAngleBraces + 1);			
			int lastAngleBraceIndex = bqlCommand.LastIndexOf('>');

            if (lastAngleBraceIndex >= 0)
                bqlCommand = bqlCommand.Substring(0, lastAngleBraceIndex + 1);

            int firstAngleBraceIndex = bqlCommand.IndexOf('<');

            if (firstAngleBraceIndex < 0)
                return;

            string selectOp = bqlCommand.Substring(0, firstAngleBraceIndex);
            GetSelectCommandTag(newSnapshot, bqlCommand, offset);

			GetBQLOperandTags(newSnapshot, bqlCommand, offset);
			GetDacWithFieldTags(newSnapshot, bqlCommand, offset);
			GetSingleDacAndConstantTags(newSnapshot, bqlCommand, offset);
			GetBqlParameterTags(newSnapshot, bqlCommand, offset);
		}

		private void GetBQLOperandTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = RegExpressions.DacOperandRegex.Matches(bqlCommand);

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
			var matches = RegExpressions.BQLParametersRegex.Matches(bqlCommand);

			foreach (Match bqlParamMatch in matches)
			{
				string bqlParam = bqlParamMatch.Value;
				CreateTag(newSnapshot, bqlParamMatch, offset, bqlParam, bqlParameterType);
			}
		}

		private void GetSingleDacAndConstantTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = RegExpressions.DacOrConstantRegex.Matches(bqlCommand);

			foreach (Match dacOrConstMatch in matches)
			{
				string dacOrConst = dacOrConstMatch.Value.Trim(',', '<', '>');
				CreateTag(newSnapshot, dacOrConstMatch, offset, dacOrConst, dacType);
			}
		}

		private void GetDacWithFieldTags(ITextSnapshot newSnapshot, string bqlCommand, int offset)
		{
			var matches = RegExpressions.DacWithFieldRegex.Matches(bqlCommand);

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
