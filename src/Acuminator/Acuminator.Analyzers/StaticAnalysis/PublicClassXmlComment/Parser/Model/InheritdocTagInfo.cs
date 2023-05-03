#nullable enable

using System;
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

		public XmlAttributeSyntax? CrefAttribute { get; }

		[MemberNotNullWhen(returnValue: true, member: nameof(Tag))]
		public bool HasInheritdocTag => Tag != null;

		[MemberNotNullWhen(returnValue: true, member: nameof(CrefAttribute))]
		public bool InheritdocTagHasCrefAttribute => CrefAttribute != null;

		public InheritdocTagInfo(XmlElementSyntax? inheritdocTag)
		{
			Tag = inheritdocTag;
			CrefAttribute = GetCrefAttribute(inheritdocTag);
		}

		private static XmlAttributeSyntax? GetCrefAttribute(XmlElementSyntax? inheritdocTag)
		{
			if (inheritdocTag?.StartTag == null)
				return null;

			var attributes = inheritdocTag.StartTag.Attributes;
			return attributes.Count > 0
				? attributes.FirstOrDefault(xmlAttribute => xmlAttribute.IsKind(SyntaxKind.XmlCrefAttribute))
				: null;
		}

		public override string ToString() => Tag?.ToString() ?? string.Empty;
	}
}