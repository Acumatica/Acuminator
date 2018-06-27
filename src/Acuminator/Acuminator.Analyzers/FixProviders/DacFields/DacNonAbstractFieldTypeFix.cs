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
	public class DacNonAbstractFieldTypeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1024_DacNonAbstractFieldType.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return Task.Run(() =>
			{
				string codeActionName = nameof(Resources.PX1024Fix).GetLocalized().ToString();
				CodeAction codeAction =
					CodeAction.Create(codeActionName,
									  cToken => MarkDacFieldAsAbstractAsync(context.Document, context.Span, cToken),
									  equivalenceKey: codeActionName);

				context.RegisterCodeFix(codeAction, context.Diagnostics);
			});
		}

		private async Task<Document> MarkDacFieldAsAbstractAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken)
											.ConfigureAwait(false);
			ClassDeclarationSyntax dacFieldDeclaration = root?.FindNode(span) as ClassDeclarationSyntax;

			if (dacFieldDeclaration == null || cancellationToken.IsCancellationRequested)
				return document;

			SyntaxToken abstractToken = SyntaxFactory.Token(SyntaxKind.AbstractKeyword);

			if (dacFieldDeclaration.Modifiers.Contains(abstractToken))
				return document;

			var modifiedRoot = root.ReplaceNode(dacFieldDeclaration, dacFieldDeclaration.AddModifiers(abstractToken));
			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}