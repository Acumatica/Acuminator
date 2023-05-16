#nullable enable

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class PublicClassXmlCommentFix : CodeFixProvider
	{
		private const string _xmlTextNewLine = "\n";
		private const char _descriptionWordsSeparator = ' ';

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1007_PublicClassXmlComment.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics
				.FirstOrDefault(d => d.Id.Equals(Descriptors.PX1007_PublicClassXmlComment.Id));

			if (diagnostic?.Properties == null || 
				!diagnostic.Properties.TryGetValue(XmlAnalyzerConstants.XmlCommentParseResultKey, out string value) ||
				!Enum.TryParse(value, out XmlCommentParseResult parseResult) ||
				parseResult == XmlCommentParseResult.HasExcludeTag || parseResult == XmlCommentParseResult.HasNonEmptySummaryTag)
			{
				return Task.CompletedTask;
			}

			var addDescriptionTitle = nameof(Resources.PX1007FixAddDescription).GetLocalized().ToString();
			var addDescriptionAction = CodeAction.Create(
				addDescriptionTitle,
				cancellation => AddDescriptionAsync(context.Document, context.Span, parseResult, cancellation),
				equivalenceKey: addDescriptionTitle);
			context.RegisterCodeFix(addDescriptionAction, context.Diagnostics);

			var excludeClassTitle = nameof(Resources.PX1007FixExcludeClass).GetLocalized().ToString();
			var excludeClassAction = CodeAction.Create(
				excludeClassTitle,
				cancellation => ExcludeClassAsync(context.Document, context.Span, cancellation),
				equivalenceKey: excludeClassTitle);
			context.RegisterCodeFix(excludeClassAction, context.Diagnostics);

			return Task.CompletedTask;
		}

		private async Task<Document> AddDescriptionAsync(Document document, TextSpan span, XmlCommentParseResult parseResult, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (rootNode?.FindNode(span) is not MemberDeclarationSyntax memberDeclaration)
				return document;

			string memberName = memberDeclaration.GetIdentifiers().FirstOrDefault().ToString();
			if (memberName.IsNullOrWhiteSpace())
				return document;

			var newRootNode = GetRootNodeSyntaxWithDescription(rootNode, memberDeclaration, memberName, parseResult, cancellation);
			if (newRootNode == null)
				return document;

			var newDocument = document.WithSyntaxRoot(newRootNode);
			return newDocument;
		}

		private SyntaxNode GetRootNodeSyntaxWithDescription(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
			string className, XmlCommentParseResult parseResult, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var description = GenerateDescriptionFromCamelCase(className);
			return parseResult switch
			{
				XmlCommentParseResult.NoXmlComment			   => AddXmlCommentWithSummaryTag(rootNode, memberDeclaration, description, cancellation),
				XmlCommentParseResult.NoSummaryOrInheritdocTag => AddXmlCommentWithSummaryTag(rootNode, memberDeclaration, description, cancellation),
				XmlCommentParseResult.EmptySummaryTag		   => AddDescriptionToSummaryTag(rootNode, memberDeclaration, description, cancellation),
				_											   => memberDeclaration
			};
		}

		private SyntaxNode AddDescriptionToSummaryTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
													  string description, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			XmlNodeSyntax? summaryTag = FindSummaryTag(memberDeclaration);
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
					var newSummaryTag = CreateNonEmptySummaryNode(description).WithAdditionalAnnotations(Formatter.Annotation);
					return rootNode.ReplaceNode(oneLinerSummaryTag, newSummaryTag);
				}

				default:
					return rootNode;
			}

			//---------------------------------------------------------Local Function--------------------------------------------------
			static XmlNodeSyntax? FindSummaryTag(MemberDeclarationSyntax memberDeclaration)
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

						if (XmlCommentsConstants.SummaryTag.Equals(docTagName, StringComparison.Ordinal))
							return docTagNode;
					}
				}

				return null;
			}

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
				return summaryTagWithEmptyContent.WithContent(newContent)
												 .WithAdditionalAnnotations(Formatter.Annotation);
			}
		}

		private SyntaxNode AddXmlCommentWithSummaryTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
													   string description, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var xmlDescriptionTrivia = Trivia(
				DocumentationComment(
					XmlSummaryElement(
						XmlText(
							XmlTextNewLine(Environment.NewLine, true)
						),
						XmlText(
							XmlTextNewLine(description + Environment.NewLine, true)
						)
					),
					XmlText(_xmlTextNewLine)
				)
				.WithAdditionalAnnotations(Formatter.Annotation)
			);

			return AddDocumentationTrivia(rootNode, memberDeclaration, xmlDescriptionTrivia, cancellation);
		}

		private static XmlElementSyntax CreateNonEmptySummaryNode(string description) =>
			XmlSummaryElement(
				XmlText(
					XmlTextNewLine(Environment.NewLine, continueXmlDocumentationComment: true)
				),
				XmlText(
					XmlTextNewLine(description + Environment.NewLine, continueXmlDocumentationComment: true)
				)
			);

		private async Task<Document> AddInheritdocTagAsync(Document document, TextSpan span, XmlCommentParseResult parseResult, 
														   INamedTypeSymbol projectionDacOriginalBqlFieldName, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (rootNode?.FindNode(span) is not MemberDeclarationSyntax memberDeclaration)
				return document;

			string memberName = memberDeclaration.GetIdentifiers().FirstOrDefault().ToString();
			if (memberName.IsNullOrWhiteSpace())
				return document;

			cancellation.ThrowIfCancellationRequested();

			var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
			bool removeOldTrivia = parseResult != XmlCommentParseResult.NoXmlComment;

			var newRootNode = AddXmlCommentWithInheritdocTag(rootNode, memberDeclaration, removeOldTrivia, projectionDacOriginalBqlFieldName, syntaxGenerator);
			if (newRootNode == null)
				return document;

			var newDocument = document.WithSyntaxRoot(newRootNode);
			return newDocument;
		}

		private SyntaxNode? AddXmlCommentWithInheritdocTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration, bool removeOldTrivia,
														   INamedTypeSymbol projectionDacOriginalBqlFieldName, SyntaxGenerator syntaxGenerator)
		{
			if (syntaxGenerator.TypeExpression(projectionDacOriginalBqlFieldName) is not TypeSyntax bqlDacFieldNode)
				return null;

			var crefAttributeList = SingletonList<XmlAttributeSyntax>(
											XmlCrefAttribute(
												TypeCref(bqlDacFieldNode)));
			XmlEmptyElementSyntax inheritdocTag = XmlEmptyElement(XmlName(XmlCommentsConstants.InheritdocTag), crefAttributeList);

			var xmlInheritdocTrivia = 
				Trivia(
					DocumentationCommentTrivia(
						SyntaxKind.SingleLineDocumentationCommentTrivia,
						SingletonList<XmlNodeSyntax>(inheritdocTag)
					)
					.WithAdditionalAnnotations(Formatter.Annotation));

			var newTrivia = memberDeclaration.GetLeadingTrivia().Add(xmlInheritdocTrivia);
			var newClassDeclarationSyntax = memberDeclaration.WithLeadingTrivia(newTrivia);

			return rootNode.ReplaceNode(memberDeclaration, newClassDeclarationSyntax);
		}

		private SyntaxNode RemoveWrongDocTagsFromDeclaration(MemberDeclarationSyntax memberDeclaration) 
		{
			var triviaList = memberDeclaration.GetLeadingTrivia();

			if (triviaList.Count == 0)
				return memberDeclaration;
			
			foreach (SyntaxTrivia trivia in triviaList)
			{
				if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax documentationNode || documentationNode.Content.Count == 0)
					continue;

				foreach (var docXmlNode in documentationNode.Content)
				{
					docXmlNode.Star
				}
			}

			XmlEmptyElement

			XmlElementSyntax

			var summaryTag =
											  .Select(t => t.GetStructure())
											  .OfType<DocumentationCommentTriviaSyntax>()
											  .SelectMany(d => d.ChildNodes())
											  .OfType<XmlElementSyntax>()
											  .FirstOrDefault(n => XmlCommentsConstants.SummaryTag.Equals(n.StartTag?.Name?.ToString(), StringComparison.Ordinal));
		}

		private async Task<Document> AddExcludeTagAsync(Document document, TextSpan span, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (!(rootNode?.FindNode(span) is MemberDeclarationSyntax memberDeclaration))
			{
				return document;
			}

			var xmlExcludeTrivia = Trivia(
				DocumentationComment(
					XmlEmptyElement(XmlAnalyzerConstants.XmlCommentExcludeTag),
					XmlText(_xmlTextNewLine)
				)
			);
			var newRootNode = AddDocumentationTrivia(rootNode, memberDeclaration, xmlExcludeTrivia, cancellation);
			var newDocument = document.WithSyntaxRoot(newRootNode);

			return newDocument;
		}

		private SyntaxNode AddDocumentationTrivia(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
												  SyntaxTrivia documentationTrivia, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var newTrivia = memberDeclaration.GetLeadingTrivia().Add(documentationTrivia);
			var newClassDeclarationSyntax = memberDeclaration.WithLeadingTrivia(newTrivia);

			return rootNode.ReplaceNode(memberDeclaration, newClassDeclarationSyntax);
		}

		private string GenerateDescriptionFromCamelCase(string name)
		{
			var descriptionStringBuilder = new StringBuilder();

			for (int i = 0; i < name.Length; i++)
			{
				if (char.IsUpper(name[i]) && i > 0)
				{
					descriptionStringBuilder.Append(_descriptionWordsSeparator);
					descriptionStringBuilder.Append(char.ToLower(name[i]));
				}
				else
				{
					descriptionStringBuilder.Append(name[i]);
				}
			}
			
			return descriptionStringBuilder.ToString();
		}
	}
}
