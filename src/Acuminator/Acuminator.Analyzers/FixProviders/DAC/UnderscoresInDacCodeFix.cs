using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1026_UnderscoresInDacDeclaration.Id);

			if (diagnostic == null || !diagnostic.IsRegisteredForCodeFix())
				return Task.FromResult(false);

			return Task.Run(() =>
			{
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
			string identifierWithoutUnderscores = null;

			switch (diagnosticNode)
			{
				case ClassDeclarationSyntax classDeclaration:
					identifierWithoutUnderscores = classDeclaration.Identifier.ValueText.Replace("_", string.Empty);
					return classDeclaration.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case IdentifierNameSyntax identifierName:
					identifierWithoutUnderscores = identifierName.Identifier.ValueText.Replace("_", string.Empty);
					return identifierName.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case VariableDeclaratorSyntax variableDeclarator:
					identifierWithoutUnderscores = variableDeclarator.Identifier.ValueText.Replace("_", string.Empty);
					return variableDeclarator.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case PropertyDeclarationSyntax propertyDeclaration:
					identifierWithoutUnderscores = propertyDeclaration.Identifier.ValueText.Replace("_", string.Empty);
					return propertyDeclaration.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case EventDeclarationSyntax eventDeclaration:
					identifierWithoutUnderscores = eventDeclaration.Identifier.ValueText.Replace("_", string.Empty);
					return eventDeclaration.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case DelegateDeclarationSyntax delegateDeclaration:
					identifierWithoutUnderscores = delegateDeclaration.Identifier.ValueText.Replace("_", string.Empty);
					return delegateDeclaration.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case MethodDeclarationSyntax methodDeclaration:
					identifierWithoutUnderscores = methodDeclaration.Identifier.ValueText.Replace("_", string.Empty);
					return methodDeclaration.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				case EnumDeclarationSyntax enumDeclaration:
					identifierWithoutUnderscores = enumDeclaration.Identifier.ValueText.Replace("_", string.Empty);
					return enumDeclaration.WithIdentifier(
						SyntaxFactory.Identifier(identifierWithoutUnderscores));

				default:
					return null;
			}
		}
	}
}