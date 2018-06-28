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
                    c => InsertStartRowAssigment(context.Document, node, c),
                    Resources.PX1010Fix),
                context.Diagnostics);
        }

        private async Task<Document> InsertStartRowAssigment(Document document, InvocationExpressionSyntax invocationDeclaration, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax mds = invocationDeclaration.SyntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().
                Where(m => m.DescendantNodes().Contains(invocationDeclaration)).Single();

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var pxContext = new PXContext(semanticModel.Compilation);

            var last = mds.Body.DescendantNodes().OfType<ReturnStatementSyntax>().Last();
            var newAssigment = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.ParseExpression($"{nameof(PXView)}.{nameof(PXView.StartRow)} = 0").
                WithLeadingTrivia(last.GetLeadingTrivia()));
            var root = await document.GetSyntaxRootAsync();
            return document.WithSyntaxRoot(
                root.InsertNodesBefore(last, new SyntaxNode[] { newAssigment }));
        }
    }
}