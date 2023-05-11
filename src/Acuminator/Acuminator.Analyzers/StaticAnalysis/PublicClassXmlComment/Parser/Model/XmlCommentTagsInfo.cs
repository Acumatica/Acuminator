#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

		public IEnumerable<XmlElementSyntax> GetTagNodes(bool includeSummaryTag, bool includeInheritdocTag, bool includeExcludeTag)
		{
			if (NoXmlComments || (!includeSummaryTag && !includeInheritdocTag && !includeExcludeTag))
				return Enumerable.Empty<XmlElementSyntax>();

			return GetTagNodesImplementation(includeSummaryTag, includeInheritdocTag, includeExcludeTag);
		}

		private IEnumerable<XmlElementSyntax> GetTagNodesImplementation(bool includeSummaryTag, bool includeInheritdocTag, bool includeExcludeTag)
		{
			if (HasSummaryTag && includeSummaryTag)
				yield return SummaryTag;

			if (InheritdocTagInfo.HasInheritdocTag && includeInheritdocTag)
				yield return InheritdocTagInfo.Tag;

			if (HasExcludeTag && includeExcludeTag)
				yield return ExcludeTag;
		}
	}
}