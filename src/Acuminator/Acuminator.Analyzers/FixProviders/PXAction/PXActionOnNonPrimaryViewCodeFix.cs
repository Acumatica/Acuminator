using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;


namespace Acuminator.Analyzers.FixProviders
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class PXActionOnNonPrimaryViewCodeFix : CodeFixProvider
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

			string format = nameof(Resources.PX1012Fix).GetLocalized().ToString();
			string codeActionName = string.Format(format, mainDacName);
			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
																.ConfigureAwait(false);

			INamedTypeSymbol mainDacType = semanticModel?.Compilation.GetTypeByMetadataName(mainDacMetadata);

			if (mainDacType == null || context.CancellationToken.IsCancellationRequested)
				return;

			CodeAction codeAction =
				CodeAction.Create(codeActionName,
								  cToken => ChangePXActionDeclarationAsync(context.Document, context.Span, cToken, mainDacType),
								  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> ChangePXActionDeclarationAsync(Document document, TextSpan span, CancellationToken cancellationToken,
																	INamedTypeSymbol mainDacType)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken)
											.ConfigureAwait(false);
			SyntaxNode dacFieldDeclaration = root?.FindNode(span);

			if (dacFieldDeclaration == null || cancellationToken.IsCancellationRequested)
				return document;

			return document;

			//SyntaxToken abstractToken = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);

			//if (dacFieldDeclaration.Modifiers.Contains(abstractToken))
			//	return document;

			//var modifiedRoot = root.ReplaceNode(dacFieldDeclaration, dacFieldDeclaration.AddModifiers(abstractToken));
			//return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}