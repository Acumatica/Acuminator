#nullable enable


using Acuminator;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal enum XmlCommentParseResult : byte
	{
		NoXmlComment,
		NoSummaryOrInheritdocTag,
		EmptySummaryTag,
		HasExcludeTag,
		HasNonEmptySummaryTag,
		CorrectInheritdocTag,
		IncorrectInheritdocTag,
		MultipleDocTags
	}
}
