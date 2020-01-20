using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal enum XmlCommentParseResult
	{
		NoXmlComment,
		NoSummaryTag,
		EmptySummaryTag,
		HasExcludeTag,
		HasNonEmptySummaryTag
	}

	public static class XmlAnalyzerConstants
	{
		public const string XmlCommentExcludeTag = "exclude";
		public static readonly string XmlCommentSummaryTag = SyntaxFactory.XmlSummaryElement().StartTag.Name.ToFullString();
		internal const string XmlCommentParseResultKey = nameof(XmlCommentParseResult);

		internal static bool ShouldNotDisplayDiagnostic(this XmlCommentParseResult parseResult) =>
			parseResult == XmlCommentParseResult.HasExcludeTag ||
			parseResult == XmlCommentParseResult.HasNonEmptySummaryTag;
	}
}
