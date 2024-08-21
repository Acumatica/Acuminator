
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class DatabaseQueriesInRowSelectingFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1042_DatabaseQueriesInRowSelecting.Id);

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

			if (semanticModel == null || newRoot == null)
				return document;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);

			if (!ShouldGenerateConnectionScope(pxContext))
				return document;

			var usingNode = CreateUsingStatementNode(document, pxContext, methodNode);
			var newMethodNode = methodNode.Body.WithStatements(new SyntaxList<StatementSyntax>().Add(usingNode));

			newRoot = newRoot.ReplaceNode(
				methodNode.Body,
				newMethodNode);

			return document.WithSyntaxRoot(newRoot);
		}

		protected virtual bool ShouldGenerateConnectionScope(PXContext pxContext) =>
			!pxContext.IsAcumatica2023R1_OrGreater;

		private UsingStatementSyntax CreateUsingStatementNode(Document document, PXContext pxContext, MethodDeclarationSyntax methodNode)
		{
			var generator = SyntaxGenerator.GetGenerator(document);
			var usingNode = 
				generator.UsingStatement(
					SyntaxFactory.ObjectCreationExpression(
										(TypeSyntax)generator.TypeExpression(pxContext.PXConnectionScope),
										SyntaxFactory.ArgumentList(),
										default(InitializerExpressionSyntax)),
					methodNode.Body.Statements)
				.WithAdditionalAnnotations(Formatter.Annotation);

			return (UsingStatementSyntax)usingNode;
		}
	}
}
