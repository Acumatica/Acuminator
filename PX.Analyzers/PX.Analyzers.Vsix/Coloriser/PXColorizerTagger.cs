using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace PX.Analyzers.Coloriser
{
	internal class PXColorizerTagger : ITagger<IClassificationTag>
	{
		//static PXColorizerTagger()
		//{
		//	AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
		//}

		List<ITagSpan<IClassificationTag>> tags = new List<ITagSpan<IClassificationTag>>();
		private IClassificationType dacType;
		private IClassificationType fieldType;
		//private IClassificationType extensionMethodType;
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
			//extensionMethodType = registry.GetClassificationType(Constants.ExtensionMethodFormat);
		}

		public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (spans.Count == 0)
			{
				return Enumerable.Empty<ITagSpan<IClassificationTag>>();
			}

			//if (cache != null && cache == spans[0].Snapshot)
			//	return tags;

			tags.Clear();
			cache = spans[0].Snapshot;
			return GetTagsImpl(cache, spans);
		}

		private IEnumerable<ITagSpan<IClassificationTag>> GetTagsImpl(ITextSnapshot newSnapshot, NormalizedSnapshotSpanCollection spans)
		{
			
			const string pattern = @"<?([A-Z][a-z]*)+\.[\r|\t|\n]*([a-z]+[A-Z]*)+([>|,])?";

			string row = newSnapshot.GetText();
			var matches = Regex.Matches(row, pattern);

			foreach (Match match in matches)
			{
				string[] dacParts = match.Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

				if (dacParts.Length != 2)
					continue;

				ITagSpan<IClassificationTag> dacTag = GetDacTag(newSnapshot, match, dacParts);
				ITagSpan<IClassificationTag> fieldTag = GetFieldTag(newSnapshot, match, dacParts);
				
				tags.Add(dacTag);
				tags.Add(fieldTag);
			}

			return tags;
		}

		private TagSpan<IClassificationTag> GetDacTag(ITextSnapshot newSnapshot, Match match, string[] dacParts)
		{
			string dacPart = dacParts[0].TrimStart(',', '<');
			int startIndex = match.Index + match.Value.IndexOf(dacPart);
			Span dacSpan = new Span(startIndex, dacPart.Length);
			SnapshotSpan snapshotSpan = new SnapshotSpan(newSnapshot, dacSpan);
			return new TagSpan<IClassificationTag>(snapshotSpan, new ClassificationTag(dacType));
		}

		private TagSpan<IClassificationTag> GetFieldTag(ITextSnapshot newSnapshot, Match match, string[] dacParts)
		{
			string fieldPart = dacParts[1].TrimEnd(',', '>');
			int startIndex = match.Index + match.Value.IndexOf(fieldPart);
			Span fieldSpan = new Span(startIndex, fieldPart.Length);
			SnapshotSpan snapshotSpan = new SnapshotSpan(newSnapshot, fieldSpan);
			return new TagSpan<IClassificationTag>(snapshotSpan, new ClassificationTag(fieldType));
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

	}
}
