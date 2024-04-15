﻿using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.PXActionOnNonPrimaryView
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class PXActionOnNonPrimaryViewFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1012_PXActionOnNonPrimaryView.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1012_PXActionOnNonPrimaryView.Id);

			if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
				return;

			if (!diagnostic.Properties.TryGetValue(DiagnosticProperty.DacName, out string mainDacName) ||
				!diagnostic.Properties.TryGetValue(DiagnosticProperty.DacMetadataName, out string mainDacMetadata))
			{
				return;
			}

			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
																.ConfigureAwait(false);
			INamedTypeSymbol mainDacType = semanticModel?.Compilation.GetTypeByMetadataName(mainDacMetadata);

			if (mainDacType == null || context.CancellationToken.IsCancellationRequested)
				return;

			string codeActionName = nameof(Resources.PX1012Fix).GetLocalized(mainDacName).ToString();
			CodeAction codeAction =
				CodeAction.Create(codeActionName,
								  cToken => ChangePXActionDeclarationAsync(context.Document, context.Span, cToken, mainDacType),
								  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> ChangePXActionDeclarationAsync(Document document, TextSpan span, CancellationToken cancellationToken,
																	INamedTypeSymbol mainDacType)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			GenericNameSyntax pxActionTypeDeclaration = root?.FindNode(span) as GenericNameSyntax;

			if (pxActionTypeDeclaration == null || cancellationToken.IsCancellationRequested)
				return document;

			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
			TypeSyntax mainDacTypeNode = generator.TypeExpression(mainDacType) as TypeSyntax;

			if (mainDacType == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedTypeArgsSyntax = pxActionTypeDeclaration.TypeArgumentList
																.WithArguments(SyntaxFactory.SingletonSeparatedList(mainDacTypeNode));
			GenericNameSyntax modifiedDeclaration = pxActionTypeDeclaration.WithTypeArgumentList(modifiedTypeArgsSyntax);

			(SyntaxNode originalNode, SyntaxNode modifiedNode) = pxActionTypeDeclaration.Parent switch
			{
				VariableDeclarationSyntax variableDeclaration => (variableDeclaration, variableDeclaration.WithType(modifiedDeclaration)),
				PropertyDeclarationSyntax propertyDeclaration => (propertyDeclaration, propertyDeclaration.WithType(modifiedDeclaration)),
				_ => default((SyntaxNode, SyntaxNode))
			};
			
			if (originalNode == null || modifiedNode == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedRoot = root.ReplaceNode(originalNode, modifiedNode);
			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}