using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;
using PX.Data;

namespace Acuminator.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class StartRowResetForPagingFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1010_StartRowResetForPaging.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
			Diagnostic diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1010_StartRowResetForPaging.Id);

			if (diagnostic == null || diagnostic.C || context.CancellationToken.IsCancellationRequested)
				return;

			SyntaxNode root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
            var invocation = root?.FindNode(context.Span) as InvocationExpressionSyntax;

			if (invocation == null)
				return;

			string codeActionName = nameof(Resources.PX1010Fix).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName, cToken => InsertStartRowAssigmentAsync(context.Document, invocation, cToken),
													  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private async Task<Document> InsertStartRowAssigmentAsync(Document document, InvocationExpressionSyntax invocationDeclaration, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax methodDeclaration = invocationDeclaration.GetDeclaringMethodNode();

			if (methodDeclaration?.Body == null)
				return document;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var pxContext = new PXContext(semanticModel.Compilation);

			ReturnStatementSyntax lastReturnStatement = methodDeclaration.Body.DescendantNodes()
																			  .OfType<ReturnStatementSyntax>()
																			  .LastOrDefault();

			if (lastReturnStatement != null)
			{
				var newAssigment = SyntaxFactory.ExpressionStatement(
					SyntaxFactory.ParseExpression($"{nameof(PXView)}.{nameof(PXView.StartRow)} = 0").
					WithLeadingTrivia(lastReturnStatement.GetLeadingTrivia()));

				var root = await document.GetSyntaxRootAsync();
				return document.WithSyntaxRoot(
									root.InsertNodesBefore(lastReturnStatement, new SyntaxNode[] { newAssigment }));
			}

			
        }
    }
}