#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
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

			var (semanticModel, rootNode) = await Document.GetSemanticModelAndRootAsync(cancellation).ConfigureAwait(false);

			if (semanticModel == null || rootNode?.FindNode(Span) is not MemberDeclarationSyntax memberDeclaration)
				return Document;

			string memberName = memberDeclaration.GetIdentifiers().FirstOrDefault().ToString();
			if (memberName.IsNullOrWhiteSpace())
				return Document;

			if (semanticModel.Compilation.GetTypeByMetadataName(_mappedOriginalDacName) is not INamedTypeSymbol mappedOriginalDac)
				return Document;

			cancellation.ThrowIfCancellationRequested();

			var newRootNode = AddXmlCommentWithInheritdocTag(rootNode, memberDeclaration, mappedOriginalDac, cancellation);
			if (newRootNode == null)
				return Document;

			var newDocument = Document.WithSyntaxRoot(newRootNode);
			return newDocument;
		}

		private SyntaxNode? AddXmlCommentWithInheritdocTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration, INamedTypeSymbol mappedOriginalDac, 
														   CancellationToken cancellation)
		{
			SyntaxGenerator syntaxGenerator = SyntaxGenerator.GetGenerator(Document);

			if (syntaxGenerator.TypeExpression(mappedOriginalDac) is not TypeSyntax typeSyntax)
				return null;

			var mapppedPropertyCref = QualifiedCref(
										typeSyntax,
										NameMemberCref(
											IdentifierName(_mappedPropertyName)));

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
			var allNodesToDelete = GetAllNodesToDeleteFromMemberDeclaration(memberDeclaration, cancellation);

			if (allNodesToDelete?.Count is null or 0)
				return AddInheritdocTagToDeclaration(memberDeclaration, inheritdocTag);

			var removeOptions = SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives;
			var memberDeclarationWithRemovedTags = memberDeclaration.RemoveNodes(allNodesToDelete, removeOptions);
			var newMemberDeclaration = AddInheritdocTagToDeclaration(memberDeclarationWithRemovedTags, inheritdocTag);

			return newMemberDeclaration;
		}

		private List<SyntaxNode>? GetAllNodesToDeleteFromMemberDeclaration(MemberDeclarationSyntax memberDeclaration, CancellationToken cancellation)
		{
			var triviaList = memberDeclaration.GetLeadingTrivia();

			if (triviaList.Count == 0)
				return null;

			List<SyntaxNode>? allDocTagsToDelete = null;

			foreach (SyntaxTrivia trivia in triviaList)
			{
				cancellation.ThrowIfCancellationRequested();

				if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax docCommentParentNode)
					continue;

				var docCommentNodesToDelete = GetNodesToDeleteFromDocComment(docCommentParentNode);

				if (docCommentNodesToDelete?.Count is null or 0)
					continue;

				if (allDocTagsToDelete == null)
					allDocTagsToDelete = new List<SyntaxNode>(docCommentNodesToDelete);
				else
					allDocTagsToDelete.AddRange(docCommentNodesToDelete);
			}

			return allDocTagsToDelete;
		}

		/// <summary>
		/// Gets tags to delete from document comment.
		/// </summary>
		private IReadOnlyCollection<SyntaxNode>? GetNodesToDeleteFromDocComment(DocumentationCommentTriviaSyntax docCommentParentNode)
		{
			if (docCommentParentNode.Content.Count == 0)
				return null;

			List<XmlNodeSyntax>? tagsToDeleteFromDocComment = null;
			bool collectEmptyTags = false;
			bool allTagsAreDeleted = true;

			foreach (XmlNodeSyntax docXmlNode in docCommentParentNode.Content)
			{
				if (docXmlNode is XmlTextSyntax textNode)
				{
					if (collectEmptyTags)
					{
						tagsToDeleteFromDocComment ??= new List<XmlNodeSyntax>(capacity: 4);
						tagsToDeleteFromDocComment.Add(textNode);
					}

					continue;
				}

				string? tagName = docXmlNode.GetDocTagName();

				if (tagName == XmlCommentsConstants.SummaryTag || tagName == XmlCommentsConstants.InheritdocTag)
				{
					tagsToDeleteFromDocComment ??= new List<XmlNodeSyntax>(capacity: 4);
					tagsToDeleteFromDocComment.Add(docXmlNode);
					collectEmptyTags = true;
				}
				else
				{
					collectEmptyTags = false;
					allTagsAreDeleted = false;
				}
			}

			return allTagsAreDeleted 
				? new[] { docCommentParentNode }
				: tagsToDeleteFromDocComment;
		}
	}
}