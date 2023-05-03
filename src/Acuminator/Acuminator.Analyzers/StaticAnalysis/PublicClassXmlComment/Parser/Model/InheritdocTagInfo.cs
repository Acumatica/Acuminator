#nullable enable

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
		public XmlElementSyntax? Tag { get; }

		public ImmutableArray<XmlCrefAttributeSyntax> CrefAttributes { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(Tag))]
		public bool HasInheritdocTag => Tag != null;

		public bool InheritdocTagHasCrefAttributes => !CrefAttributes.IsDefaultOrEmpty;

		public InheritdocTagInfo(XmlElementSyntax? inheritdocTag)
		{
			Tag = inheritdocTag;
			CrefAttributes = GetCrefAttribute(inheritdocTag);
		}

		private static ImmutableArray<XmlCrefAttributeSyntax> GetCrefAttribute(XmlElementSyntax? inheritdocTag)
		{
			if (inheritdocTag?.StartTag == null)
				return ImmutableArray<XmlCrefAttributeSyntax>.Empty;

			var attributes = inheritdocTag.StartTag.Attributes;
			return attributes.Count > 0
				? attributes.OfType<XmlCrefAttributeSyntax>().ToImmutableArray()
				: ImmutableArray<XmlCrefAttributeSyntax>.Empty;
		}

		public override string ToString() => Tag?.ToString() ?? string.Empty;
	}
}