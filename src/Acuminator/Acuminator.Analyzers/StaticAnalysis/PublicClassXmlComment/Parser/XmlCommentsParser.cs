#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

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
			_cancellation = cancellation;
		}

		public XmlCommentsParseInfo AnalyzeXmlComments(MemberDeclarationSyntax memberDeclaration, IPropertySymbol? mappedDacProperty)
		{
			var parseResult = ParseDeclarationXmlComments(memberDeclaration, mappedDacProperty);

			_cancellation.ThrowIfCancellationRequested();

			DiagnosticDescriptor? diagnosticToReport = GetDiagnosticFromParseResult(parseResult);
			bool stepIntoChildren = memberDeclaration is TypeDeclarationSyntax && parseResult != XmlCommentParseResult.HasExcludeTag;
			return new XmlCommentsParseInfo(parseResult, diagnosticToReport, stepIntoChildren);
		}

		private XmlCommentParseResult ParseDeclarationXmlComments(MemberDeclarationSyntax memberDeclaration, IPropertySymbol? mappedDacProperty)
		{
			_cancellation.ThrowIfCancellationRequested();

			if (!memberDeclaration.HasStructuredTrivia)
				return XmlCommentParseResult.NoXmlComment;

			bool isProjectionDacProperty = mappedDacProperty != null;
			bool hasXmlComment = false, hasSummaryTag = false, hasInheritdocTag = false,
				 nonEmptySummaryTag = false, correctInheritdocTagOfProjectionProperty = false;

			IEnumerable<DocumentationCommentTriviaSyntax> xmlComments = GetXmlComments(memberDeclaration);

			foreach (DocumentationCommentTriviaSyntax xmlComment in xmlComments)
			{
				_cancellation.ThrowIfCancellationRequested();
				XmlCommentTagsInfo commentTagsInfo = GetDocumentationTags(xmlComment);

				if (commentTagsInfo.NoXmlComments)
					continue;
				else if (commentTagsInfo.HasExcludeTag)
					return XmlCommentParseResult.HasExcludeTag;

				hasXmlComment = true;
				
				if (!commentTagsInfo.HasSummaryTag && !commentTagsInfo.HasInheritdocTag)
					continue;

				if (commentTagsInfo.HasSummaryTag)
				{
					hasSummaryTag = true;
					nonEmptySummaryTag = nonEmptySummaryTag || commentTagsInfo.SummaryTags.Any(IsNonEmptySummaryTag);
				}

				if (commentTagsInfo.HasInheritdocTag)
				{
					hasInheritdocTag = true;

					// To determine if the inheritdoc tag is correct we check these things:
					// 1. We only check inheritdoc for the projection DAC properties, for all other APIs inheritdoc tag is considered to be correct by default
					// 2. If there already was a correct inheritdoc tag (there could be multiple inheritdoc tags), then no need to check this tag
					// 3. If none of above is true check that inherit doc is referencing the DAC field property to which the projection property is mapped to.
					correctInheritdocTagOfProjectionProperty =
						correctInheritdocTagOfProjectionProperty ||
						commentTagsInfo.InheritdocTagInfos.Any(info => IsCorrectInheritdocTag(info, mappedDacProperty));
				}
			}

			if (!hasXmlComment)
				return XmlCommentParseResult.NoXmlComment;

			var parseResult = (hasSummaryTag, hasInheritdocTag, isProjectionDacProperty) switch
			{
				(true, true, true) => correctInheritdocTagOfProjectionProperty
											? XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty
											: XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty,

				(true, true, false) => nonEmptySummaryTag
											? XmlCommentParseResult.HasNonEmptySummaryAndCorrectInheritdocTags
											: XmlCommentParseResult.EmptySummaryTag,

				(false, false, _) => XmlCommentParseResult.NoSummaryOrInheritdocTag,
				(true, false, true) => XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty,

				(true, false, false) => nonEmptySummaryTag
											? XmlCommentParseResult.HasNonEmptySummaryTag
											: XmlCommentParseResult.EmptySummaryTag,

				(false, true, _) => correctInheritdocTagOfProjectionProperty
											? XmlCommentParseResult.CorrectInheritdocTag
											: XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty
			};

			return parseResult;
		}

		private IEnumerable<DocumentationCommentTriviaSyntax> GetXmlComments(MemberDeclarationSyntax member) =>
			member.GetLeadingTrivia()
				  .Select(t => t.GetStructure())
				  .OfType<DocumentationCommentTriviaSyntax>();

		private XmlCommentTagsInfo GetDocumentationTags(DocumentationCommentTriviaSyntax xmlComment)
		{
			if (xmlComment.Content.Count == 0)
				return new XmlCommentTagsInfo(summaryTags: null, inheritdocTags: null, excludeTags: null);

			List<XmlNodeSyntax>? summaryTags = null, inheritDocTags = null, excludeTags = null;

			foreach (XmlNodeSyntax xmlNode in xmlComment.Content)
			{
				string? tagName = xmlNode.GetDocTagName();

				switch (tagName)
				{
					case XmlCommentsConstants.SummaryTag:
						summaryTags = AddTagToList(summaryTags, xmlNode);
						break;

					case XmlCommentsConstants.InheritdocTag:
						inheritDocTags = AddTagToList(inheritDocTags, xmlNode);
						break;

					case XmlCommentsConstants.ExcludeTag:
						excludeTags = AddTagToList(excludeTags, xmlNode);
						break;
				}
			}

			return new XmlCommentTagsInfo(summaryTags, inheritDocTags, excludeTags);

			//-------------------------------------------Local Function-----------------------------------------------------------
			static List<XmlNodeSyntax> AddTagToList(List<XmlNodeSyntax>? list, XmlNodeSyntax tagNode)
			{
				list ??= new List<XmlNodeSyntax>(capacity: 1);
				list.Add(tagNode);
				return list;
			}
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

		/// <summary>
		/// Query if <paramref name="inheritdocTagInfo"/> is correct inheritdoc tag.
		/// </summary>
		/// <remarks>
		///  To determine if the inheritdoc tag is correct we check these things:
		/// 1. We only check inheritdoc for the projection DAC properties, for all other APIs inheritdoc tag is considered to be correct by default
		/// 2. If there already was a correct inheritdoc tag (there could be multiple inheritdoc tags), then no need to check this tag
		/// 3. If none of above is true check that inherit doc is referencing the DAC field property to which the projection property is mapped to.  
		/// 4. </remarks>
		/// <param name="inheritdocTagInfo">Information describing the inheritdoc tag.</param>
		/// <param name="mappedDacProperty">The mapped DAC property.</param>
		/// <returns>
		/// True if correct inheritdoc tag, false if not.
		/// </returns>
		private bool IsCorrectInheritdocTag(in InheritdocTagInfo inheritdocTagInfo, IPropertySymbol? mappedDacProperty)
		{
			bool isProjectionDacProperty = mappedDacProperty != null;

			if (!isProjectionDacProperty)
				return true;

			bool allowEmptyInheritdocTag = !isProjectionDacProperty || mappedDacProperty!.IsOverride;

			if ((!inheritdocTagInfo.InheritdocTagHasCrefAttributes && !allowEmptyInheritdocTag) || inheritdocTagInfo.CrefAttributes.Length > 1)
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
	}
}