#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	internal partial class XmlCommentsParser
	{
		private static readonly string[] _xmlCommentSummarySeparators = { SyntaxFactory.DocumentationComment().ToFullString() };

		private readonly SemanticModel _semanticModel;
		private readonly CancellationToken _cancellation;

		public XmlCommentsParser(SemanticModel semanticModel, CancellationToken cancellation)
		{
			_semanticModel = semanticModel;
			_cancellation  = cancellation;
		}

		public XmlCommentsParseInfo AnalyzeXmlComments(MemberDeclarationSyntax memberDeclaration, IPropertySymbol? mappedDacProperty)
		{
			var (parseResult, tagsInfos) = ParseDeclarationXmlComments(memberDeclaration, mappedDacProperty);

			_cancellation.ThrowIfCancellationRequested();

			DiagnosticDescriptor? diagnosticToReport = GetDiagnosticFromParseResult(parseResult);
			bool stepIntoChildren = memberDeclaration is TypeDeclarationSyntax && parseResult != XmlCommentParseResult.HasExcludeTag;

			return CreateParseInfo(parseResult, tagsInfos, diagnosticToReport, stepIntoChildren);
		}

		private (XmlCommentParseResult ParseResult, List<XmlCommentTagsInfo>? TagsInfos) ParseDeclarationXmlComments(MemberDeclarationSyntax memberDeclaration,
																													 IPropertySymbol? mappedDacProperty)
		{
			_cancellation.ThrowIfCancellationRequested();

			if (!memberDeclaration.HasStructuredTrivia)
				return (XmlCommentParseResult.NoXmlComment, TagsInfos: null);

			bool isProjectionDacProperty = mappedDacProperty != null;			
			bool hasXmlComment = false, hasSummaryTag = false, hasInheritdocTag = false, 
				 nonEmptySummaryTag = false, correctInheritdocTagOfProjectionProperty = false;

			IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(memberDeclaration);
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
					hasSummaryTag = true;
					nonEmptySummaryTag = nonEmptySummaryTag || IsNonEmptySummaryTag(commentTagsInfo.SummaryTag);
				}

				if (commentTagsInfo.HasInheritdocTag)
				{
					hasInheritdocTag = true;

					// To determine if the inheritdoc tag is correct we check these things:
					// 1. We only check inheritdoc for the projection DAC properties, for all other APIs inheritdoc tag is considered to be correct by default
					// 2. If there already was a correct inheritdoc tag (there could be multiple inheritdoc tags), then no need to check this tag
					// 3. If none of above is true check that inherit doc is referencing the DAC field property to which the projection property is mapped to.
					correctInheritdocTagOfProjectionProperty = 
						!isProjectionDacProperty || correctInheritdocTagOfProjectionProperty ||
						IsCorrectInheritdocTagOfProjectionDacProperty(commentTagsInfo.InheritdocTagInfo, mappedDacProperty);
				}
			}

			if (!hasXmlComment)
				return (XmlCommentParseResult.NoXmlComment, TagsInfos: null);

			var parseResult = (hasSummaryTag, hasInheritdocTag, isProjectionDacProperty) switch
			{
				(true, true, true)	 => correctInheritdocTagOfProjectionProperty
											? XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty
											: XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty,

				(true, true, false)  => nonEmptySummaryTag
											? XmlCommentParseResult.HasNonEmptySummaryAndCorrectInheritdocTags
											: XmlCommentParseResult.EmptySummaryTag,

				(false, false, _) 	 => XmlCommentParseResult.NoSummaryOrInheritdocTag,
				(true, false, true)  => XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty,

				(true, false, false) => nonEmptySummaryTag
											? XmlCommentParseResult.HasNonEmptySummaryTag
											: XmlCommentParseResult.EmptySummaryTag,

				(false, true, _)  	 => correctInheritdocTagOfProjectionProperty
											? XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty
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
			if (xmlComment.Content.Count == 0)
				return new XmlCommentTagsInfo(summaryTag: null, inheritdocTag: null, excludeTag: null);

			XmlNodeSyntax? summaryTag = null, inheritDocTag = null, excludeTag = null;
			
			foreach (XmlNodeSyntax xmlNode in xmlComment.Content)
			{
				string? tagName = xmlNode switch
				{
					XmlElementSyntax docTagWithContent   => docTagWithContent.StartTag?.Name?.ToString(),
					XmlEmptyElementSyntax oneLinerDocTag => oneLinerDocTag.Name?.ToString(),
					_									 => null
				};
				
				switch (tagName)
				{
					case XmlCommentsConstants.SummaryTag:
						summaryTag = xmlNode;
						break;

					case XmlCommentsConstants.InheritdocTag:
						inheritDocTag = xmlNode;
						break;

					case XmlCommentsConstants.ExcludeTag:
						excludeTag = xmlNode;
						break;
				}
			}

			return new XmlCommentTagsInfo(summaryTag, inheritDocTag, excludeTag);
		}

		private bool IsNonEmptySummaryTag(XmlNodeSyntax summaryTag)
		{
			if (summaryTag is not XmlElementSyntax summaryTagWithContent)
				return false;

			var summaryContent = summaryTagWithContent.Content;

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

		private bool IsCorrectInheritdocTagOfProjectionDacProperty(InheritdocTagInfo inheritdocTagInfo, IPropertySymbol? mappedDacProperty)
		{
			bool isProjectionDacProperty = mappedDacProperty != null;

			if (!isProjectionDacProperty)
				return true;
			else if (!inheritdocTagInfo.InheritdocTagHasCrefAttributes || inheritdocTagInfo.CrefAttributes.Length > 1)
				return false;
			
			XmlCrefAttributeSyntax crefAttribute = inheritdocTagInfo.CrefAttributes[0];

			if (crefAttribute?.Cref == null) 
				return false;

			SymbolInfo crefSymbolInfo = _semanticModel.GetSymbolInfo(crefAttribute.Cref, _cancellation);
			ISymbol? crefSymbol = crefSymbolInfo.Symbol ?? crefSymbolInfo.CandidateSymbols.FirstOrDefault();

			return crefSymbol is IPropertySymbol referencedProperty && mappedDacProperty!.Equals(referencedProperty);
		}

		private DiagnosticDescriptor? GetDiagnosticFromParseResult(XmlCommentParseResult parseResult) =>
			parseResult switch
			{
				XmlCommentParseResult.HasExcludeTag 								=> null,
				XmlCommentParseResult.HasNonEmptySummaryTag 						=> null,
				XmlCommentParseResult.CorrectInheritdocTag 							=> null,
				XmlCommentParseResult.HasNonEmptySummaryAndCorrectInheritdocTags 	=> null,
				XmlCommentParseResult.NoXmlComment 									=> Descriptors.PX1007_PublicClassNoXmlComment,
				XmlCommentParseResult.EmptySummaryTag 								=> Descriptors.PX1007_PublicClassNoXmlComment,
				XmlCommentParseResult.NoSummaryOrInheritdocTag 						=> Descriptors.PX1007_PublicClassNoXmlComment,
				XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty => Descriptors.PX1007_InvalidProjectionDacFieldDescription,
				XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty 		=> Descriptors.PX1007_InvalidProjectionDacFieldDescription,
				_ 																	=> null
			};

		private XmlCommentsParseInfo CreateParseInfo(XmlCommentParseResult parseResult, List<XmlCommentTagsInfo>? tagsInfos, 
													 DiagnosticDescriptor? diagnosticToReport, bool stepIntoChildren)
		{
			if (diagnosticToReport == null)
				return new XmlCommentsParseInfo(parseResult, stepIntoChildren);
			else if (tagsInfos?.Count is null or 0)
				return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, locationsWithErrors: null);

			switch (parseResult)
			{
				case XmlCommentParseResult.NoXmlComment:
				case XmlCommentParseResult.NoSummaryOrInheritdocTag:
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, locationsWithErrors: null);

				case XmlCommentParseResult.EmptySummaryTag:
					XmlCommentTagsInfo summaryTagInfo = tagsInfos.FirstOrDefault(tagInfo => tagInfo.HasSummaryTag);
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, summaryTagInfo.SummaryTag);

				case XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty:
					XmlCommentTagsInfo inheritdocTagInfo = tagsInfos.FirstOrDefault(tagInfo => tagInfo.HasInheritdocTag);
					var crefAttributes = inheritdocTagInfo.InheritdocTagInfo.CrefAttributes;

					return crefAttributes.Length switch
					{
						0 => new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, inheritdocTagInfo.InheritdocTagInfo.Tag),
						1 => new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, crefAttributes[0]),
						_ => new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, crefAttributes)
					}; ;

				case XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty:
					var nodesWithErrors = tagsInfos.Where(tagInfo => tagInfo.HasSummaryTag)
												   .Select(tagInfo => tagInfo.SummaryTag!);			//Only summary tag can be among error nodes at this step
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, nodesWithErrors);

				default:
					return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren, locationsWithErrors: null);
			}
		}
	}
}