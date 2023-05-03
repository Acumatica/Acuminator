#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal partial class XmlCommentsParser
	{
		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };
		private readonly CancellationToken _cancellation;

		public XmlCommentsParser(CancellationToken cancellation)
		{
			_cancellation = cancellation;
		}

		public (DiagnosticDescriptor? DiagnosticToReport, bool CheckChildNodes) AnalyzeDeclarationXmlComments(MemberDeclarationSyntax memberDeclaration)
		{
			XmlCommentParseResult parseResult = ParseDeclarationXmlComments(memberDeclaration);

			_cancellation.ThrowIfCancellationRequested();
			return AnalyzeCommentParseResult(parseResult);
		}

		private XmlCommentParseResult ParseDeclarationXmlComments(MemberDeclarationSyntax memberDeclaration)
		{
			_cancellation.ThrowIfCancellationRequested();

			if (!memberDeclaration.HasStructuredTrivia)
				return XmlCommentParseResult.NoXmlComment;

			IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(memberDeclaration);
			bool hasXmlComment = false, hasSummaryTag = false, nonEmptySummaryTag = false,
				 hasInheritdocTag = false, correctInheritdocTag = false;

			foreach (DocumentationCommentTriviaSyntax xmlComment in xmlComments)
			{
				_cancellation.ThrowIfCancellationRequested();

				hasXmlComment = true;
				XmlCommentTagsInfo tagsInfo = GetDocumentationTags(xmlComment);

				if (tagsInfo.HasExcludeTag)
					return XmlCommentParseResult.HasExcludeTag;
				else if (!tagsInfo.HasSummaryTag && !tagsInfo.HasInheritdocTag)
					continue;

				if (tagsInfo.HasSummaryTag)
				{
					hasSummaryTag = true;
					nonEmptySummaryTag = IsNonEmptySummaryTag(tagsInfo.SummaryTag);
				}

				if (tagsInfo.HasInheritdocTag)
				{
					hasInheritdocTag = true;
					correctInheritdocTag = IsCorrectInheritDocTag(tagsInfo.InheritdocTag);
				}
			}

			if (!hasXmlComment)
				return XmlCommentParseResult.NoXmlComment;

			switch ((hasSummaryTag, hasInheritdocTag))
			{
				case (true, true):
					return XmlCommentParseResult.SummaryAndInheritdocTags;
				case (false, false):
					return XmlCommentParseResult.NoSummaryOrInheritdocTag;
				case (true, false):
					return nonEmptySummaryTag
						? XmlCommentParseResult.HasNonEmptySummaryTag
						: XmlCommentParseResult.EmptySummaryTag;
				case (false, true):
					return correctInheritdocTag
						? XmlCommentParseResult.IncorrectInheritdocTag
						: XmlCommentParseResult.CorrectInheritdocTag;
			}
		}

		private IEnumerable<DocumentationCommentTriviaSyntax> GetXmlComments(MemberDeclarationSyntax member) =>
			member.GetLeadingTrivia()
				  .Select(t => t.GetStructure())
				  .OfType<DocumentationCommentTriviaSyntax>();

		private XmlCommentTagsInfo GetDocumentationTags(DocumentationCommentTriviaSyntax xmlComment)
		{
			var xmlNodes = xmlComment.ChildNodes().OfType<XmlElementSyntax>();
			XmlElementSyntax? summaryTag = null, inheritDocTag = null, excludeTag = null;

			foreach (XmlElementSyntax xmlNode in xmlNodes)
			{
				string? tagName = xmlNode.StartTag?.Name?.ToString();

				if (XmlCommentsConstants.SummaryTag.Equals(tagName, StringComparison.Ordinal))
					summaryTag = xmlNode;
				else if (XmlCommentsConstants.InheritdocTag.Equals(tagName, StringComparison.Ordinal))
					inheritDocTag = xmlNode;
				else if (XmlCommentsConstants.ExcludeTag.Equals(tagName, StringComparison.Ordinal))
					excludeTag = xmlNode;
			}

			return new XmlCommentTagsInfo(summaryTag, inheritDocTag, excludeTag);
		}

		private bool IsNonEmptySummaryTag(XmlElementSyntax summaryTag)
		{
			var summaryContent = summaryTag.Content;

			if (summaryContent.Count == 0)
				return false;

			foreach (XmlNodeSyntax contentNode in summaryContent)
			{
				var contentString = contentNode.ToFullString();
				if (contentString.IsNullOrWhiteSpace())
					continue;

				var contentHasText = contentString.Split(_xmlCommentSummarySeparators, StringSplitOptions.RemoveEmptyEntries)
												  .Any(CommentContentIsNotEmpty);
				if (contentHasText)
					return true;
			}

			return false;
		}

		private static bool CommentContentIsNotEmpty(string content) =>
			!content.IsNullOrEmpty() && content.Any(char.IsLetterOrDigit);

		private bool IsCorrectInheritdocTag(InheritdocTagInfo inheritdocTagInfo, ITypeSymbol? mappedBqlField)
		{
			bool isProjectionDacProperty = mappedBqlField != null;

			if (!inheritdocTagInfo.InheritdocTagHasCrefAttributes)
				return !isProjectionDacProperty;
			else if (inheritdocTagInfo.CrefAttributes.Length > 1)
				return false;

			XmlCrefAttributeSyntax crefAttribute = inheritdocTagInfo.CrefAttributes[0];

			if (crefAttribute?.Cref == null) 
				return false;
			else if (!isProjectionDacProperty)
				return true;

			var mappedPropertySymbol = GetDacPropertyCorrespondingToTheMappedBqlField(mappedBqlField!);

			if (mappedPropertySymbol == null) 
				return false;

			SymbolInfo crefSymbolInfo = _semanticModel.GetSymbolInfo(crefAttribute.Cref, _cancellation);
			ISymbol? crefSymbol = crefSymbolInfo.Symbol ?? crefSymbolInfo.CandidateSymbols.FirstOrDefault();

			return crefSymbol is IPropertySymbol referencedProperty && 
				   mappedPropertySymbol.Equals(referencedProperty);
		}

		private IPropertySymbol? GetDacPropertyCorrespondingToTheMappedBqlField(ITypeSymbol mappedBqlField)
		{
			if (mappedBqlField.ContainingType == null)
				return null;

			DacType? dacType = mappedBqlField.ContainingType.GetDacType(_pxContext);

			if (dacType == null)
				return null;
			else if (dacType == DacType.DacExtension)
				return GetDacPropertySymbol(mappedBqlField.ContainingType, mappedBqlField.Name);

			INamedTypeSymbol? currentDac = mappedBqlField.ContainingType;

			while (currentDac != null)
			{
				var property = GetDacPropertySymbol(currentDac, mappedBqlField.Name);

				if (property != null)
					return property;

				currentDac = currentDac.BaseType;
			}

			return null;
		}

		private IPropertySymbol? GetDacPropertySymbol(INamedTypeSymbol type, string caseInsensitivePropertyName)
		{
			var members = type.GetMembers();

			if (members.IsDefaultOrEmpty)
				return null;

			return members.OfType<IPropertySymbol>()
						  .FirstOrDefault(property => caseInsensitivePropertyName.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
		}

		private (DiagnosticDescriptor? DiagnosticToReport, bool CheckChildNodes) AnalyzeCommentParseResult(XmlCommentParseResult parseResult) =>
			parseResult switch
			{
				XmlCommentParseResult.HasExcludeTag => (null, CheckChildNodes: false),
				XmlCommentParseResult.HasNonEmptySummaryTag => (null, CheckChildNodes: true),
				XmlCommentParseResult.CorrectInheritdocTag => (null, CheckChildNodes: true),
				XmlCommentParseResult.NoXmlComment => (Descriptors.PX1007_PublicClassNoXmlComment, CheckChildNodes: true),
				XmlCommentParseResult.EmptySummaryTag => (Descriptors.PX1007_PublicClassNoXmlComment, CheckChildNodes: true),
				XmlCommentParseResult.NoSummaryOrInheritdocTag => (Descriptors.PX1007_PublicClassNoXmlComment, CheckChildNodes: true),
				XmlCommentParseResult.SummaryAndInheritdocTags => (Descriptors.PX1007_MultipleDocumentationTags, CheckChildNodes: true),
				XmlCommentParseResult.IncorrectInheritdocTag => (Descriptors.PX1007_InvalidProjectionDacFieldDescription, CheckChildNodes: true),
				_ => (null, CheckChildNodes: true)
			};
	}
}