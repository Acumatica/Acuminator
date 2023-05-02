#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal readonly struct XmlCommentTagsInfo
	{
		public XmlElementSyntax? SummaryTag { get; }

		public XmlElementSyntax? InheritdocTag { get; }

		public XmlElementSyntax? ExcludeTag { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(SummaryTag))]
		public bool HasSummaryTag => SummaryTag != null;

		[MemberNotNullWhen(returnValue: true, member: nameof(InheritdocTag))]
		public bool HasInheritdocTag => InheritdocTag != null;

		[MemberNotNullWhen(returnValue: true, member: nameof(ExcludeTag))]
		public bool HasExcludeTag => ExcludeTag != null;

		public XmlCommentTagsInfo(XmlElementSyntax? summaryTag, XmlElementSyntax? inheritdocTag, XmlElementSyntax? excludeTag)
		{
			SummaryTag = summaryTag;
			InheritdocTag = inheritdocTag;
			ExcludeTag = excludeTag;
		}
	}
}