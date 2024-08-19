
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal readonly struct InheritdocTagInfo
	{
		public XmlNodeSyntax? Tag { get; }

		public ImmutableArray<XmlCrefAttributeSyntax> CrefAttributes { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(Tag))]
		public bool HasInheritdocTag => Tag != null;

		public bool InheritdocTagHasCrefAttributes => !CrefAttributes.IsDefaultOrEmpty;

		public InheritdocTagInfo(XmlNodeSyntax? inheritdocTag)
		{
			Tag = inheritdocTag;
			CrefAttributes = GetCrefAttribute(inheritdocTag);
		}

		private static ImmutableArray<XmlCrefAttributeSyntax> GetCrefAttribute(XmlNodeSyntax? inheritdocTag)
		{
			var attributes = GetInheritdocTagAttributes(inheritdocTag);

			if (attributes == null || attributes.Value.Count == 0)
				return ImmutableArray<XmlCrefAttributeSyntax>.Empty;

			return attributes.Value.OfType<XmlCrefAttributeSyntax>().ToImmutableArray();
		}

		private static SyntaxList<XmlAttributeSyntax>? GetInheritdocTagAttributes(XmlNodeSyntax? inheritdocTag)
		{
			switch (inheritdocTag)
			{
				case XmlEmptyElementSyntax oneLineTag:
					return oneLineTag.Attributes;
				case XmlElementSyntax tagWithContent when tagWithContent.StartTag != null:
					return tagWithContent.StartTag.Attributes;
				default:
					return null;
			}
		}

		public override string ToString() => Tag?.ToString() ?? string.Empty;
	}
}