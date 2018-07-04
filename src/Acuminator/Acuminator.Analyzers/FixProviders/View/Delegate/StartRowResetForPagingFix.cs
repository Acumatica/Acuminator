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
            var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
            var node = (InvocationExpressionSyntax)root.FindNode(context.Span);

            context.RegisterCodeFix(
                CodeAction.Create(
                    Resources.PX1010Fix,
                    c => InsertStartRowAssigmentAsync(context.Document, node, c),
                    Resources.PX1010Fix),
                context.Diagnostics);
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