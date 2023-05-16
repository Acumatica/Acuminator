#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
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
			new HashSet<string>
			{
				Descriptors.PX1007_PublicClassNoXmlComment.Id,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id
			}
			.ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var diagnostics = context.Diagnostics;

			if (diagnostics.IsDefaultOrEmpty)
				return;

			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
																.ConfigureAwait(false);
			if (semanticModel == null)
				return;

			List<Task> allTasks = new(capacity: diagnostics.Length);

			foreach (Diagnostic diagnostic in context.Diagnostics)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (diagnostic.Id != Descriptors.PX1007_PublicClassNoXmlComment.Id &&
					diagnostic.Id != Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id)
				{
					continue;
				}

				var task = RegisterCodeFixesForDiagnosticAsync(context, diagnostic, semanticModel);
				allTasks.Add(task);
			}

			await Task.WhenAll(allTasks).ConfigureAwait(false);
		}

		private Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic, SemanticModel semanticModel)
		{
			if (diagnostic?.Properties == null || !diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.ParseResult, out string value) ||
				!Enum.TryParse(value, out XmlCommentParseResult parseResult) || IsCorrectParseResult(parseResult))
			{
				return Task.CompletedTask;
			}

			bool isProjectionDacProperty = diagnostic.IsFlagSet(DocumentationDiagnosticProperties.IsProjectionDacProperty);

			if (isProjectionDacProperty)
				RegisterCodeFixForProjectionDacProperty(context, diagnostic, semanticModel, parseResult);
			else if (parseResult != XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty &&
					 parseResult != XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty)
			{
				RegisterAddSummaryTagCodeFix(context, parseResult);
			}

			var excludeClassTitle = nameof(Resources.PX1007FixExcludeClass).GetLocalized().ToString();
			var excludeClassAction = CodeAction.Create(
				excludeClassTitle,
				cancellation => AddExcludeTagAsync(context.Document, context.Span, cancellation),
				equivalenceKey: excludeClassTitle);
			context.RegisterCodeFix(excludeClassAction, context.Diagnostics);

			return Task.CompletedTask;
		}

		private bool IsCorrectParseResult(XmlCommentParseResult parseResult) =>
			parseResult is XmlCommentParseResult.HasExcludeTag or XmlCommentParseResult.HasNonEmptySummaryTag or 
						   XmlCommentParseResult.CorrectInheritdocTag or XmlCommentParseResult.HasNonEmptySummaryAndCorrectInheritdocTags;

		private void RegisterAddSummaryTagCodeFix(CodeFixContext context, XmlCommentParseResult parseResult)
		{
			var addSummaryTitle = nameof(Resources.PX1007FixAddSummaryTag).GetLocalized().ToString();
			var addSummaryAction = CodeAction.Create(addSummaryTitle,
													 cancellation => AddSummaryTagAsync(context.Document, context.Span, parseResult, cancellation),
													 equivalenceKey: addSummaryTitle);

			context.RegisterCodeFix(addSummaryAction, context.Diagnostics);
		}

		private void RegisterCodeFixForProjectionDacProperty(CodeFixContext context, Diagnostic diagnostic, SemanticModel semanticModel,
															 XmlCommentParseResult parseResult)
		{
			if (GetProjectionDacOriginalBqlField(diagnostic, semanticModel) is not INamedTypeSymbol projectionDacOriginalBqlField)
			{
				return;
			}
			
			var addInheritdocTitle = nameof(Resources.PX1007FixAddInheritdocTag).GetLocalized().ToString();
			var addInheritdocAction = CodeAction.Create(addInheritdocTitle,
														cancellation => AddInheritdocTagAsync(context.Document, context.Span, parseResult, 
																							  projectionDacOriginalBqlField, cancellation),
														equivalenceKey: addInheritdocTitle);

			context.RegisterCodeFix(addInheritdocAction, context.Diagnostics);
		}
	
		private INamedTypeSymbol? GetProjectionDacOriginalBqlField(Diagnostic diagnostic, SemanticModel semanticModel)
		{
			if (!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.MappedDacBqlFieldMetadataName, out string projectionDacOriginalBqlFieldName) ||
				projectionDacOriginalBqlFieldName.IsNullOrWhiteSpace())
			{
				return null;
			}

			return semanticModel.Compilation.GetTypeByMetadataName(projectionDacOriginalBqlFieldName);
		}

		private async Task<Document> AddSummaryTagAsync(Document document, TextSpan span, XmlCommentParseResult parseResult, CancellationToken cancellation)
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
					var newSummaryTag = CreateNonEmptySummaryNode(description).WithAdditionalAnnotations(Formatter.Annotation);
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
					CreateNonEmptySummaryNode(description),
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

		private SyntaxNode? AddXmlCommentWithInheritdocTag(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration, bool removeOldDocTags,
														   INamedTypeSymbol projectionDacOriginalBqlFieldName, SyntaxGenerator syntaxGenerator)
		{
			if (syntaxGenerator.TypeExpression(projectionDacOriginalBqlFieldName) is not TypeSyntax bqlDacFieldNode)
				return null;

			var crefAttributeList = SingletonList<XmlAttributeSyntax>(
											XmlCrefAttribute(
												TypeCref(bqlDacFieldNode)));
			XmlEmptyElementSyntax inheritdocTag = XmlEmptyElement(XmlName(XmlCommentsConstants.InheritdocTag), crefAttributeList);

			MemberDeclarationSyntax newMemberDeclaration = removeOldDocTags
				? ReplaceWrongDocTagsFromDeclaration(memberDeclaration, inheritdocTag)
				: AddInheritdocTagToDeclaration(memberDeclaration, inheritdocTag);

			return rootNode.ReplaceNode(memberDeclaration, newMemberDeclaration);
		}

		private MemberDeclarationSyntax AddInheritdocTagToDeclaration(MemberDeclarationSyntax memberDeclaration, XmlEmptyElementSyntax inheritdocTag) 
		{
			var xmlInheritdocTrivia =
				Trivia(
					DocumentationComment(
						inheritdocTag,
						XmlText(_xmlTextNewLine)
					)
					.WithAdditionalAnnotations(Formatter.Annotation));

			var newTrivia = memberDeclaration.GetLeadingTrivia().Add(xmlInheritdocTrivia);
			var newMemberDeclaration = memberDeclaration.WithLeadingTrivia(newTrivia);

			return newMemberDeclaration;
		}

		private MemberDeclarationSyntax ReplaceWrongDocTagsFromDeclaration(MemberDeclarationSyntax memberDeclaration, XmlEmptyElementSyntax inheritdocTag) 
		{
			var triviaList = memberDeclaration.GetLeadingTrivia();

			if (triviaList.Count == 0)
				return memberDeclaration;

			var newLeadingTrivias = UpdateTriviaDocCommentNodesWithNewContent(triviaList, inheritdocTag);
			var newMemberDeclaration = memberDeclaration.WithLeadingTrivia(newLeadingTrivias);

			return newMemberDeclaration;
		}

		private List<SyntaxTrivia> UpdateTriviaDocCommentNodesWithNewContent(in SyntaxTriviaList triviaList, XmlEmptyElementSyntax inheritdocTag)
		{
			List<SyntaxTrivia> newTriviaList = new(capacity: triviaList.Count);
			bool addInheritdocTag = true;

			foreach (SyntaxTrivia trivia in triviaList)
			{
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

		private async Task<Document> AddExcludeTagAsync(Document document, TextSpan span, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (rootNode?.FindNode(span) is not MemberDeclarationSyntax memberDeclaration)
			{
				return document;
			}

			var xmlExcludeTrivia = Trivia(
				DocumentationComment(
					XmlEmptyElement(XmlCommentsConstants.ExcludeTag),
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
