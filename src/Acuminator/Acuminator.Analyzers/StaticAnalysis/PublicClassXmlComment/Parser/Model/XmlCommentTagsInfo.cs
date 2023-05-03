#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal readonly struct XmlCommentTagsInfo
	{
		public XmlElementSyntax? SummaryTag { get; }

		public InheritdocTagInfo InheritdocTagInfo { get; }

		public XmlElementSyntax? ExcludeTag { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(SummaryTag))]
		public bool HasSummaryTag => SummaryTag != null;

		public bool HasInheritdocTag => InheritdocTagInfo.HasInheritdocTag;

		[MemberNotNullWhen(returnValue: true, member: nameof(ExcludeTag))]
		public bool HasExcludeTag => ExcludeTag != null;

		public bool NoXmlComments => !HasExcludeTag && !HasSummaryTag && !HasInheritdocTag;

		public XmlCommentTagsInfo(XmlElementSyntax? summaryTag, XmlElementSyntax? inheritdocTag, XmlElementSyntax? excludeTag)
		{
			SummaryTag = summaryTag;
			InheritdocTagInfo = new InheritdocTagInfo(inheritdocTag);
			ExcludeTag = excludeTag;
		}

		public IEnumerable<XmlElementSyntax> GetAllTagNodes()
		{
			if (HasSummaryTag)
				yield return SummaryTag;

			if (InheritdocTagInfo.HasInheritdocTag)
				yield return InheritdocTagInfo.Tag;

			if (HasExcludeTag)
				yield return ExcludeTag;
		}
	}
}