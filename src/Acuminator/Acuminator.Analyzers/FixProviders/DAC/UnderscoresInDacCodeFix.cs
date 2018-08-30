using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;


namespace Acuminator.Analyzers.FixProviders
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class UnderscoresInDacCodeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1026_UnderscoresInDacDeclaration.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return Task.Run(() =>
			{
				var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1026_UnderscoresInDacDeclaration.Id);

				if (diagnostic == null || !diagnostic.IsRegisteredForCodeFix())
					return;

				string codeActionName = nameof(Resources.PX1026Fix).GetLocalized().ToString();
				CodeAction codeAction = CodeAction.Create(codeActionName,
														  cToken => ChangeUnderscoredNamesAsync(context.Document, context.Span, cToken),
														  equivalenceKey: codeActionName);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			}, context.CancellationToken);
		}

		private async Task<Document> ChangeUnderscoredNamesAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode diagnosticNode = root?.FindNode(span);

			if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;

			SyntaxNode modifiedNode = GetNodeWithoutUnderscores(diagnosticNode);

			if (modifiedNode == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedRoot = root.ReplaceNode(diagnosticNode, modifiedNode);
			return document.WithSyntaxRoot(modifiedRoot);
		}

		private static SyntaxNode GetNodeWithoutUnderscores(SyntaxNode diagnosticNode)
		{
			switch (diagnosticNode)
			{
				case ClassDeclarationSyntax classDeclaration:
					return classDeclaration.WithIdentifier(
								GetIdentifierWithoutUnderscores(classDeclaration.Identifier));
				case IdentifierNameSyntax identifierName:
					return identifierName.WithIdentifier(
								GetIdentifierWithoutUnderscores(identifierName.Identifier));
				case VariableDeclaratorSyntax variableDeclarator:
					return variableDeclarator.WithIdentifier(
								GetIdentifierWithoutUnderscores(variableDeclarator.Identifier));
				case PropertyDeclarationSyntax propertyDeclaration:
					return propertyDeclaration.WithIdentifier(
													GetIdentifierWithoutUnderscores(propertyDeclaration.Identifier));
				case EventDeclarationSyntax eventDeclaration:
					return eventDeclaration.WithIdentifier(
								GetIdentifierWithoutUnderscores(eventDeclaration.Identifier));
				case DelegateDeclarationSyntax delegateDeclaration:
					return delegateDeclaration.WithIdentifier(
								GetIdentifierWithoutUnderscores(delegateDeclaration.Identifier));
				case MethodDeclarationSyntax methodDeclaration:
					return methodDeclaration.WithIdentifier(
								GetIdentifierWithoutUnderscores(methodDeclaration.Identifier));
				case EnumDeclarationSyntax enumDeclaration:
					return enumDeclaration.WithIdentifier(
								GetIdentifierWithoutUnderscores(enumDeclaration.Identifier));
				default:
					return null;
			}
		}

		private static SyntaxToken GetIdentifierWithoutUnderscores(SyntaxToken originalIdentifier)
		{
			string identifierWithoutUnderscores = originalIdentifier.ValueText.Replace("_", string.Empty);
			return SyntaxFactory.Identifier(identifierWithoutUnderscores)
								.WithTrailingTrivia(originalIdentifier.TrailingTrivia);
		}
	}
}