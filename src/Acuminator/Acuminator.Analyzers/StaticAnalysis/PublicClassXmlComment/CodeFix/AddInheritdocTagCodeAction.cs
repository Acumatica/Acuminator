#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix
{
	internal class AddInheritdocTagCodeAction : AddDocumentationTagCodeAction
	{
		private readonly XmlCommentParseResult _parseResult;
		private readonly string _mappedOriginalDacName;
		private readonly string _mappedPropertyName;

		public AddInheritdocTagCodeAction(Document document, TextSpan span, XmlCommentParseResult parseResult, 
										  string mappedOriginalDacName, string mappedPropertyName) : 
									 base(nameof(Resources.PX1007FixAddInheritdocTag).GetLocalized().ToString(), document, span)
		{
			_parseResult 		   = parseResult;
			_mappedOriginalDacName = mappedOriginalDacName;
			_mappedPropertyName    = mappedPropertyName;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await Document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (rootNode?.FindNode(Span) is not MemberDeclarationSyntax memberDeclaration)
				return Document;

			string memberName = memberDeclaration.GetIdentifiers().FirstOrDefault().ToString();
			if (memberName.IsNullOrWhiteSpace())
				return Document;

			cancellation.ThrowIfCancellationRequested();

			var newRootNode = AddXmlCommentWithInheritdocTag(rootNode, memberDeclaration, cancellation);
			if (newRootNode == null)
				return Document;

			var newDocument = Document.WithSyntaxRoot(newRootNode);
			return newDocument;
		}

		private SyntaxNode? AddXmlCommentWithInheritdocTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration, CancellationToken cancellation)
		{
			var mapppedPropertyCref = QualifiedCref(
										IdentifierName(_mappedOriginalDacName),
										NameMemberCref(
											IdentifierName(_mappedPropertyName)))
									  .WithAdditionalAnnotations(Simplifier.Annotation);

			var crefAttributeList = SingletonList<XmlAttributeSyntax>(
										XmlCrefAttribute(
											mapppedPropertyCref));

			XmlEmptyElementSyntax inheritdocTag = XmlEmptyElement(XmlName(XmlCommentsConstants.InheritdocTag), crefAttributeList);
			bool removeOldDocTags = _parseResult != XmlCommentParseResult.NoXmlComment;

			MemberDeclarationSyntax newMemberDeclaration = removeOldDocTags
				? ReplaceWrongDocTagsFromDeclaration(memberDeclaration, inheritdocTag, cancellation)
				: AddInheritdocTagToDeclaration(memberDeclaration, inheritdocTag);

			cancellation.ThrowIfCancellationRequested();

			return rootNode.ReplaceNode(memberDeclaration, newMemberDeclaration);
		}

		private MemberDeclarationSyntax AddInheritdocTagToDeclaration(MemberDeclarationSyntax memberDeclaration, XmlEmptyElementSyntax inheritdocTag)
		{
			var xmlInheritdocTrivia =
				Trivia(
					DocumentationComment(
						XmlText(string.Empty),
						inheritdocTag
					)
				);

			var newMemberDeclaration = AddDocumentationTrivia(memberDeclaration, index: 0, xmlInheritdocTrivia);
			return newMemberDeclaration;
		}

		private MemberDeclarationSyntax ReplaceWrongDocTagsFromDeclaration(MemberDeclarationSyntax memberDeclaration, XmlEmptyElementSyntax inheritdocTag,
																		   CancellationToken cancellation)
		{
			var triviaList = memberDeclaration.GetLeadingTrivia();

			if (triviaList.Count == 0)
				return memberDeclaration;

			var newLeadingTrivias = UpdateTriviaDocCommentNodesWithNewContent(triviaList, inheritdocTag, cancellation);
			var newMemberDeclaration = memberDeclaration.WithLeadingTrivia(newLeadingTrivias);

			return newMemberDeclaration;
		}

		private List<SyntaxTrivia> UpdateTriviaDocCommentNodesWithNewContent(in SyntaxTriviaList triviaList, XmlEmptyElementSyntax inheritdocTag,
																			 CancellationToken cancellation)
		{
			List<SyntaxTrivia> newTriviaList = new(capacity: triviaList.Count);
			bool addInheritdocTag = true;

			foreach (SyntaxTrivia trivia in triviaList)
			{
				cancellation.ThrowIfCancellationRequested();

				if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax docCommentParentNode)
				{
					newTriviaList.Add(trivia);
					continue;
				}

				var newContent = GetNewContentForDocComment(docCommentParentNode, addInheritdocTag, inheritdocTag);
				addInheritdocTag = false;

				if (newContent == null)
				{
					newTriviaList.Add(trivia);
					continue;
				}

				var newContentSyntaxList = newContent?.Count switch
				{
					0 => List<XmlNodeSyntax>(),
					1 => SingletonList(newContent[0]),
					_ => List(newContent)
				};

				var docCommentWithNewContent = docCommentParentNode.WithContent(newContentSyntaxList);
				var newTrivia = Trivia(docCommentWithNewContent).WithAdditionalAnnotations(Formatter.Annotation);

				newTriviaList.Add(newTrivia);
			}

			return newTriviaList;
		}

		private List<XmlNodeSyntax>? GetNewContentForDocComment(DocumentationCommentTriviaSyntax docCommentParentNode, bool addInheritdocTag,
																XmlEmptyElementSyntax inheritdocTag)
		{
			List<XmlNodeSyntax>? newDocCommentContent = addInheritdocTag
				? new List<XmlNodeSyntax>(capacity: 4) { inheritdocTag }
				: null;

			if (docCommentParentNode.Content.Count == 0)
				return newDocCommentContent;

			bool hasTagsToDelete = false;

			foreach (XmlNodeSyntax docXmlNode in docCommentParentNode.Content)
			{
				string? tagName = docXmlNode.GetDocTagName();

				// Add all doc tags except summary and inheritdoc tags to the new content
				if (tagName != XmlCommentsConstants.SummaryTag && tagName != XmlCommentsConstants.InheritdocTag)
				{
					newDocCommentContent ??= new List<XmlNodeSyntax>(capacity: 2);
					newDocCommentContent.Add(docXmlNode);
				}
				else
					hasTagsToDelete = true;
			}

			bool shouldReplaceDocCommentContent = addInheritdocTag || hasTagsToDelete;
			return shouldReplaceDocCommentContent
				? newDocCommentContent
				: null;
		}
	}
}