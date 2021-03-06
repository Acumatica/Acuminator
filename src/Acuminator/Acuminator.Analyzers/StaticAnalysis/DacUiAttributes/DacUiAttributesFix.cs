﻿using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacUiAttributes
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class DacUiAttributesFix : CodeFixProvider
	{
		private enum FixOption
		{
			AddPXCacheNameAttribute,
			AddPXHiddenAttribute
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1094_DacShouldHaveUiAttribute.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var addPXHiddenTitle = nameof(Resources.PX1094FixPXHiddenAttribute).GetLocalized().ToString();
			var addPXHiddenAction = CodeAction.Create(
				addPXHiddenTitle,
				cancellation => AddAttributeToDac(context.Document, context.Span, FixOption.AddPXHiddenAttribute, cancellation),
				equivalenceKey: addPXHiddenTitle);

			context.RegisterCodeFix(addPXHiddenAction, context.Diagnostics);

			var addPXCacheNameTitle = nameof(Resources.PX1094FixPXCacheNameAttribute).GetLocalized().ToString();
			var addPXCacheNameAction = CodeAction.Create(
				addPXCacheNameTitle,
				cancellation => AddAttributeToDac(context.Document, context.Span, FixOption.AddPXCacheNameAttribute, cancellation),
				equivalenceKey: addPXCacheNameTitle);

			context.RegisterCodeFix(addPXCacheNameAction, context.Diagnostics);

			return Task.CompletedTask;
		}

		private async Task<Document> AddAttributeToDac(Document document, TextSpan span, FixOption option, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var root = await document
				.GetSyntaxRootAsync(cancellation)
				.ConfigureAwait(false);

			if (!(root?.FindNode(span) is ClassDeclarationSyntax node))
			{
				return document;
			}

			var semanticModel = await document
				.GetSemanticModelAsync(cancellation)
				.ConfigureAwait(false);

			if (semanticModel == null)
			{
				return document;
			}

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			var attributeList = option == FixOption.AddPXCacheNameAttribute ?
				pxContext.AttributeTypes.PXCacheNameAttribute.GetAttributeList(CreateDefaultArgumentList()) :
				pxContext.AttributeTypes.PXHiddenAttribute.GetAttributeList();
			var newNode = node.AddAttributeLists(attributeList);
			var newRoot = root.ReplaceNode(node, newNode);
			var newDocument = document.WithSyntaxRoot(newRoot);

			return newDocument;

			AttributeArgumentListSyntax CreateDefaultArgumentList()
			{
				var pxCacheNameDefaultArgumentValue = nameof(Resources.PX1094PXCacheNameDefaultArgumentValue).GetLocalized().ToString();

				return SyntaxFactory.AttributeArgumentList(
					SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.AttributeArgument(
							SyntaxFactory.LiteralExpression(
								SyntaxKind.StringLiteralExpression,
								SyntaxFactory.Literal(pxCacheNameDefaultArgumentValue)))));
			}
		}
	}
}
