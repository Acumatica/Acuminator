using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Acuminator.Analyzers.StaticAnalysis.ConnectionScopeInRowSelecting
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class ConnectionScopeInRowSelectingFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1042_ConnectionScopeInRowSelecting.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			if (context.CancellationToken.IsCancellationRequested) return;

			var root = await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);
			var node = root?.FindNode(context.Span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (node?.Body != null) // do not suggest the code fix for expression-bodied methods
			{
				string title = nameof(Resources.PX1042Fix).GetLocalized().ToString();
				context.RegisterCodeFix(CodeAction.Create(title, 
					c => AddConnectionScopeAsync(context.Document, node, c), title),
					context.Diagnostics);
			}
		}

		private async Task<Document> AddConnectionScopeAsync(
			Document document,
			MethodDeclarationSyntax methodNode,
			CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var generator = SyntaxGenerator.GetGenerator(document);
			var pxContext = new PXContext(semanticModel.Compilation);
			var usingNode = (UsingStatementSyntax) generator.UsingStatement(
				SyntaxFactory.ObjectCreationExpression((TypeSyntax) generator.TypeExpression(pxContext.PXConnectionScope),
					SyntaxFactory.ArgumentList(), default(InitializerExpressionSyntax)),
				methodNode.Body.Statements)
				.WithAdditionalAnnotations(Formatter.Annotation);
			var newMethodNode = methodNode.Body.WithStatements(new SyntaxList<StatementSyntax>().Add(usingNode));

			newRoot = newRoot.ReplaceNode(
				methodNode.Body,
				newMethodNode);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
