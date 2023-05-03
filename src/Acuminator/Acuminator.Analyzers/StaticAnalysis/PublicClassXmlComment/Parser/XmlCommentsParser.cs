﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal partial class XmlCommentsParser
	{
		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };

		private readonly SemanticModel _semanticModel;
		private readonly PXContext _pxContext;
		
		private readonly CancellationToken _cancellation;

		public XmlCommentsParser(SemanticModel semanticModel, PXContext pxContext, CancellationToken cancellation)
		{
			_semanticModel = semanticModel;
			_pxContext = pxContext;
			_cancellation = cancellation;
		}

		public XmlCommentsParseInfo AnalyzeXmlComments(MemberDeclarationSyntax memberDeclaration, ITypeSymbol? mappedBqlField)
		{
			var (parseResult, tagsInfos) = ParseDeclarationXmlComments(memberDeclaration, mappedBqlField);

			_cancellation.ThrowIfCancellationRequested();

			var (diagnosticToReport, stepIntoChildren) = AnalyzeCommentParseResult(parseResult);
			return CreateParseInfo(parseResult, tagsInfos, diagnosticToReport, stepIntoChildren);
		}

		private (XmlCommentParseResult ParseResult, List<XmlCommentTagsInfo>? TagsInfos) ParseDeclarationXmlComments(MemberDeclarationSyntax memberDeclaration,
																													 ITypeSymbol? mappedBqlField)
		{
			_cancellation.ThrowIfCancellationRequested();

			if (!memberDeclaration.HasStructuredTrivia)
				return (XmlCommentParseResult.NoXmlComment, TagsInfos: null);

			IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(memberDeclaration);
			bool hasXmlComment = false;
			int summaryTagCount = 0, inheritdocCount = 0;
			bool nonEmptySummaryTag = false, correctInheritdocTag = false;

			List<XmlCommentTagsInfo>? tagsInfos = null;

			foreach (DocumentationCommentTriviaSyntax xmlComment in xmlComments)
			{
				_cancellation.ThrowIfCancellationRequested();
				XmlCommentTagsInfo commentTagsInfo = GetDocumentationTags(xmlComment);

				if (commentTagsInfo.NoXmlComments)
					continue;
				else if (commentTagsInfo.HasExcludeTag)
					return (XmlCommentParseResult.HasExcludeTag, TagsInfos: null);

				hasXmlComment = true;
				tagsInfos ??= new List<XmlCommentTagsInfo>(capacity: 1);
				tagsInfos.Add(commentTagsInfo);

				if (!commentTagsInfo.HasSummaryTag && !commentTagsInfo.HasInheritdocTag)
					continue;

				if (commentTagsInfo.HasSummaryTag)
				{
					summaryTagCount++;
					nonEmptySummaryTag = nonEmptySummaryTag || IsNonEmptySummaryTag(commentTagsInfo.SummaryTag);
				}

				if (commentTagsInfo.HasInheritdocTag)
				{
					inheritdocCount++;
					correctInheritdocTag = inheritdocCount == 1 && 
										   IsCorrectInheritdocTag(commentTagsInfo.InheritdocTagInfo, mappedBqlField);
				}
			}

			if (!hasXmlComment)
				return (XmlCommentParseResult.NoXmlComment, TagsInfos: null);
			else if (summaryTagCount > 1 || inheritdocCount > 1)
				return (XmlCommentParseResult.MultipleDocTags, tagsInfos);

			bool hasSummaryTag    = summaryTagCount == 0;
			bool hasInheritdocTag = inheritdocCount == 0;
			var parseResult = (hasSummaryTag, hasInheritdocTag) switch
			{
				(true, true)   => XmlCommentParseResult.MultipleDocTags,
				(false, false) => XmlCommentParseResult.NoSummaryOrInheritdocTag,
				(true, false)  => nonEmptySummaryTag
									? XmlCommentParseResult.HasNonEmptySummaryTag
									: XmlCommentParseResult.EmptySummaryTag,
				(false, true)  => correctInheritdocTag
									? XmlCommentParseResult.IncorrectInheritdocTag
									: XmlCommentParseResult.CorrectInheritdocTag
			};

			return (parseResult, tagsInfos);
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

		private (DiagnosticDescriptor? DiagnosticToReport, bool StepIntoChildNodes) AnalyzeCommentParseResult(XmlCommentParseResult parseResult) =>
			parseResult switch
			{
				XmlCommentParseResult.HasExcludeTag            => (null, StepIntoChildNodes: false),
				XmlCommentParseResult.HasNonEmptySummaryTag    => (null, StepIntoChildNodes: true),
				XmlCommentParseResult.CorrectInheritdocTag     => (null, StepIntoChildNodes: true),
				XmlCommentParseResult.NoXmlComment             => (Descriptors.PX1007_PublicClassNoXmlComment, StepIntoChildNodes: true),
				XmlCommentParseResult.EmptySummaryTag          => (Descriptors.PX1007_PublicClassNoXmlComment, StepIntoChildNodes: true),
				XmlCommentParseResult.NoSummaryOrInheritdocTag => (Descriptors.PX1007_PublicClassNoXmlComment, StepIntoChildNodes: true),
				XmlCommentParseResult.MultipleDocTags          => (Descriptors.PX1007_MultipleDocumentationTags, StepIntoChildNodes: true),
				XmlCommentParseResult.IncorrectInheritdocTag   => (Descriptors.PX1007_InvalidProjectionDacFieldDescription, StepIntoChildNodes: true),
				_                                              => (null, StepIntoChildNodes: true)
			};

		private XmlCommentsParseInfo CreateParseInfo(XmlCommentParseResult parseResult, List<XmlCommentTagsInfo>? tagsInfos, DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren)
		{
			if (diagnosticToReport == null)
				return new XmlCommentsParseInfo(parseResult, stepIntoChildren);
			else if (tagsInfos?.Count is null or 0)
				return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, nodesWithErrors: null);

			switch (parseResult)
			{
				case XmlCommentParseResult.NoXmlComment:
				case XmlCommentParseResult.NoSummaryOrInheritdocTag:
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, nodesWithErrors: null);

				case XmlCommentParseResult.EmptySummaryTag:
					XmlCommentTagsInfo summaryTagInfo = tagsInfos.FirstOrDefault(tagInfo => tagInfo.HasSummaryTag);
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, summaryTagInfo.SummaryTag);

				case XmlCommentParseResult.IncorrectInheritdocTag:
					XmlCommentTagsInfo inheritdocTagInfo = tagsInfos.FirstOrDefault(tagInfo => tagInfo.HasInheritdocTag);
					var crefAttributes = inheritdocTagInfo.InheritdocTagInfo.CrefAttributes;

					return crefAttributes.Length switch
					{
						0 => new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, inheritdocTagInfo.InheritdocTagInfo.Tag),
						1 => new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, crefAttributes[0]),
						_ => new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, crefAttributes)
					}; ;

				case XmlCommentParseResult.MultipleDocTags:
					var nodesWithErrors = tagsInfos.SelectMany(tagInfo => tagInfo.GetAllTagNodes());
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, nodesWithErrors);

				default:
					return new XmlCommentsParseInfo(parseResult, stepIntoChildren);
			}
		}
	}
}