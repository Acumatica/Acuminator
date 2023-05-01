#nullable enable

using System;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal readonly struct XmlCommentTagsInfo
	{
		XmlElementSyntax? SummaryTag { get; }

		XmlElementSyntax? InheritdocTag { get; }

		XmlElementSyntax? ExcludeTag { get; }

		public bool HasSummaryTag => SummaryTag != null;

		public bool HasInheritdocTag => InheritdocTag != null;

		public bool HasExcludeTag => ExcludeTag != null;

		public XmlCommentTagsInfo(XmlElementSyntax? summaryTag, XmlElementSyntax? inheritdocTag, XmlElementSyntax? excludeTag)
		{
			SummaryTag = summaryTag;
			InheritdocTag = inheritdocTag;
			ExcludeTag = excludeTag;
		}
	}
}
