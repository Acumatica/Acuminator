using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class PublicClassXmlCommentFix : CodeFixProvider
	{
		internal enum FixOption
		{
			NoXmlComment,
			NoSummaryTag,
			EmptySummaryTag
		}

		private const string _xmlTextNewLine = "\n";
		private const char _descriptionWordsSeparator = ' ';

		public const string FixOptionKey = nameof(FixOption);
		public const string XmlCommentExcludeTag = "exclude";

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1007_PublicClassXmlComment.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics
				.Where(d => d.Id.Equals(Descriptors.PX1007_PublicClassXmlComment.Id))
				.FirstOrDefault();

			if (diagnostic?.Properties == null || !diagnostic.Properties.TryGetValue(FixOptionKey, out string value) ||
				!Enum.TryParse(value, out FixOption option))
			{
				return Task.CompletedTask;
			}

			var addDescriptionTitle = nameof(Resources.PX1007FixAddDescription).GetLocalized().ToString();
			var addDescriptionAction = CodeAction.Create(
				addDescriptionTitle,
				cancellation => AddDescriptionAsync(context.Document, context.Span, option, cancellation),
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

		private async Task<Document> AddDescriptionAsync(Document document, TextSpan span, FixOption option, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (!(rootNode?.FindNode(span) is MemberDeclarationSyntax memberDeclaration))
			{
				return document;
			}

			string memberName = memberDeclaration.GetIdentifiers().FirstOrDefault().ToString();
			if (memberName.IsNullOrWhiteSpace())
				return document;

			var newRootNode = GetRootNodeSyntaxWithDescription(rootNode, memberDeclaration, memberName, option, cancellation);
			var newDocument = document.WithSyntaxRoot(newRootNode);

			return newDocument;
		}

		private SyntaxNode GetRootNodeSyntaxWithDescription(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
			string className, FixOption option, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var description = GenerateDescriptionFromCamelCase(className);

			switch (option)
			{
				case FixOption.NoXmlComment:
				case FixOption.NoSummaryTag:
					return AddXmlCommentDescription(rootNode, memberDeclaration, description, cancellation);
				case FixOption.EmptySummaryTag:
					return AddDescription(rootNode, memberDeclaration, description, cancellation);
				default:
					return memberDeclaration;
			}
		}

		private SyntaxNode AddDescription(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
										  string description, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var summaryTag = memberDeclaration
				.GetLeadingTrivia()
				.Select(t => t.GetStructure())
				.OfType<DocumentationCommentTriviaSyntax>()
				.SelectMany(d => d.ChildNodes())
				.OfType<XmlElementSyntax>()
				.Where(n => PublicClassXmlCommentAnalyzer.XmlCommentSummaryTag.Equals(n?.StartTag?.Name?.ToString(), StringComparison.Ordinal))
				.First();

			var xmlDescription = new[]
			{
				XmlText(
					XmlTextNewLine(Environment.NewLine, true)
				),
				XmlText(
					XmlTextNewLine(description + Environment.NewLine, true)
				)
			};
			var newContent = new SyntaxList<XmlNodeSyntax>(xmlDescription);
			var newSummaryTag = summaryTag
				.WithContent(newContent)
				.WithAdditionalAnnotations(Formatter.Annotation);

			return rootNode.ReplaceNode(summaryTag, newSummaryTag);
		}

		private SyntaxNode AddXmlCommentDescription(SyntaxNode rootNode, MemberDeclarationSyntax memberDeclaration,
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

		private async Task<Document> ExcludeClassAsync(Document document, TextSpan span, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootNode = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			if (!(rootNode?.FindNode(span) is MemberDeclarationSyntax memberDeclaration))
			{
				return document;
			}

			var xmlExcludeTrivia = Trivia(
				DocumentationComment(
					XmlEmptyElement(XmlCommentExcludeTag),
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
