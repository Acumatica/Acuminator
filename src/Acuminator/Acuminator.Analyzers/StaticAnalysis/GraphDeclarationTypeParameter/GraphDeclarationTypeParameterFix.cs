using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.GraphDeclarationTypeParameter
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class GraphDeclarationTypeParameterFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1093_GraphDeclarationVialation.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			if (context.CancellationToken.IsCancellationRequested)
			{
				return;
			}

			var codeActionName = nameof(Resources.PX1093Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(
				codeActionName,
				cancellation => FixGraphDeclarationTypeParameter(context.Document, context.Span, cancellation));

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> FixGraphDeclarationTypeParameter(Document document, TextSpan span, CancellationToken cancellation)
		{
			if (cancellation.IsCancellationRequested)
			{
				return document;
			}

			var root = await document
				.GetSyntaxRootAsync(cancellation)
				.ConfigureAwait(false);

			if (!(root?.FindNode(span) is IdentifierNameSyntax invalidParameterTypeIdentifier))
			{
				return document;
			}

			var classDeclaration = invalidParameterTypeIdentifier
				.Ancestors()
				.OfType<ClassDeclarationSyntax>()
				.FirstOrDefault();

			if (classDeclaration == null)
			{
				return document;
			}

			var invalidTypeParameterToken = invalidParameterTypeIdentifier.Identifier;
			var validTypeParameterToken = classDeclaration.Identifier.WithTrailingTrivia();
			var newRoot = root.ReplaceToken(invalidTypeParameterToken, validTypeParameterToken);
			var newDocument = document.WithSyntaxRoot(newRoot);

			return newDocument;
		}
	}
}
