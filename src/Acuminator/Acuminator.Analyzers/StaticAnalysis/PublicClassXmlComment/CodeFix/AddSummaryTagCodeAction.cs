#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix
{
	internal class AddSummaryTagCodeAction : AddDocumentationTagCodeAction
	{
		private const char _descriptionWordsSeparator = ' ';

		private readonly XmlCommentParseResult _parseResult;

		public AddSummaryTagCodeAction(Document document, TextSpan span, XmlCommentParseResult parseResult) : 
								  base(nameof(Resources.PX1007FixAddSummaryTag).GetLocalized().ToString(), document, span)
		{
			_parseResult = parseResult;
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

			var newRootNode = GetRootNodeSyntaxWithDescription(rootNode, memberDeclaration, memberName, cancellation);
			if (newRootNode == null)
				return Document;

			var newDocument = Document.WithSyntaxRoot(newRootNode);
			return newDocument;
		}

		private SyntaxNode GetRootNodeSyntaxWithDescription(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
															string apiName, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var description = GenerateDescriptionFromCamelCase(apiName);
			return _parseResult switch
			{
				XmlCommentParseResult.NoXmlComment 			   => AddXmlCommentWithSummaryTag(rootNode, memberDeclaration, description, cancellation),
				XmlCommentParseResult.NoSummaryOrInheritdocTag => AddXmlCommentWithSummaryTag(rootNode, memberDeclaration, description, cancellation),
				XmlCommentParseResult.EmptySummaryTag 		   => AddDescriptionToSummaryTag(rootNode, memberDeclaration, description, cancellation),
				_ 											   => memberDeclaration
			};
		}

		private SyntaxNode AddDescriptionToSummaryTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
													  string description, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			XmlNodeSyntax? summaryTag = FindDocTagByName(memberDeclaration, XmlCommentsConstants.SummaryTag);
			if (summaryTag == null)
				return AddXmlCommentWithSummaryTag(rootNode, memberDeclaration, description, cancellation);

			switch (summaryTag)
			{
				case XmlElementSyntax summaryTagWithEmptyContent:
					{
						var newSummaryTag = AddDescriptionToEmptySummaryTag(summaryTagWithEmptyContent, description);
						return rootNode.ReplaceNode(summaryTag, newSummaryTag);
					}
				case XmlEmptyElementSyntax oneLinerSummaryTag:
					{
						var newSummaryTag = CreateNonEmptySummaryNode(description);
						return rootNode.ReplaceNode(oneLinerSummaryTag, newSummaryTag);
					}

				default:
					return rootNode;
			}

			//---------------------------------------------------------Local Function--------------------------------------------------
			static XmlElementSyntax AddDescriptionToEmptySummaryTag(XmlElementSyntax summaryTagWithEmptyContent, string description)
			{
				var xmlDescription = new[]
				{
					XmlText(
						XmlTextNewLine(Environment.NewLine, continueXmlDocumentationComment: true)
					),
					XmlText(
						XmlTextNewLine(description + Environment.NewLine, continueXmlDocumentationComment: true)
					)
				};

				var newContent = new SyntaxList<XmlNodeSyntax>(xmlDescription);
				return summaryTagWithEmptyContent.WithContent(newContent);
			}
		}

		/// <summary>
		/// Adds an XML comment with summary tag.
		/// </summary>
		private SyntaxNode AddXmlCommentWithSummaryTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
													   string description, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var summaryNode = CreateNonEmptySummaryNode(description);
			DocumentationCommentTriviaSyntax summaryTag = DocumentationComment(
															XmlText(string.Empty),
															summaryNode);	
			var xmlDescriptionTrivia = Trivia(summaryTag);			
			var newMemberDeclaration = AddDocumentationTrivia(memberDeclaration, index: 0, xmlDescriptionTrivia);
			return rootNode.ReplaceNode(memberDeclaration, newMemberDeclaration);
		}

		private static XmlElementSyntax CreateNonEmptySummaryNode(string description) =>
			XmlSummaryElement(
				XmlText(
					XmlTextNewLine(Environment.NewLine, continueXmlDocumentationComment: true)
				),
				XmlText(
					XmlTextNewLine(description + Environment.NewLine)
				)
			);

		private static XmlNodeSyntax? FindDocTagByName(MemberDeclarationSyntax memberDeclaration, string tagName)
		{
			var triviaList = memberDeclaration.GetLeadingTrivia();

			if (triviaList.Count == 0)
				return null;

			foreach (var trivia in triviaList)
			{
				if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax documentationComment || documentationComment.Content.Count == 0)
					continue;

				foreach (XmlNodeSyntax docTagNode in documentationComment.Content)
				{
					string? docTagName = docTagNode.GetDocTagName();

					if (tagName.Equals(docTagName, StringComparison.Ordinal))
						return docTagNode;
				}
			}

			return null;
		}

		private static string GenerateDescriptionFromCamelCase(string name)
		{
			var descriptionStringBuilder = new StringBuilder();

			for (int i = 0; i < name.Length; i++)
				if (char.IsUpper(name[i]) && i > 0)
				{
					descriptionStringBuilder.Append(_descriptionWordsSeparator);
					descriptionStringBuilder.Append(char.ToLower(name[i]));
				}
				else
					descriptionStringBuilder.Append(name[i]);

			return descriptionStringBuilder.ToString();
		}
	}
}