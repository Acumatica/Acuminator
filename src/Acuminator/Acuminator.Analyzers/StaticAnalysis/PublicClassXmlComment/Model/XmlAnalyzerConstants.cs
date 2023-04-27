#nullable enable

using System;

using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	public static class XmlAnalyzerConstants
	{
		public const string XmlCommentExcludeTag = "exclude";
		public static readonly string XmlCommentSummaryTag = SyntaxFactory.XmlSummaryElement().StartTag.Name.ToFullString();
		public static readonly string XmlInheritDocTag = "inheritdoc";

		internal const string XmlCommentParseResultKey = nameof(XmlCommentParseResult);
	}
}
