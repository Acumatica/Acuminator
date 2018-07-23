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
    public class ConstructorInDacCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1028_ConstructorInDacDeclaration.Id);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            return Task.Run(() =>
            {
                var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1028_ConstructorInDacDeclaration.Id);

                if (diagnostic == null || !diagnostic.IsRegisteredForCodeFix())
                    return;

                string codeActionName = nameof(Resources.PX1028Fix).GetLocalized().ToString();
                CodeAction codeAction = CodeAction.Create(codeActionName,
                                                          cToken => DeleteConstructorsAsync(context.Document, context.Span, cToken),
                                                          equivalenceKey: codeActionName);

                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }, context.CancellationToken);
        }

        private async Task<Document> DeleteConstructorsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode diagnosticNode = root?.FindNode(span);

            if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
                return document;

            var modifiedRoot = root.RemoveNode(diagnosticNode, SyntaxRemoveOptions.KeepNoTrivia);

            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            ClassDeclarationSyntax classDeclaration = diagnosticNode.Parent<ClassDeclarationSyntax>();

            var rewriterWalker = new RegionTriviaRewriter(document, semanticModel, diagnosticNode, cancellationToken);
            var classModified = rewriterWalker.Visit(classDeclaration) as ClassDeclarationSyntax;

            if (classModified == null || cancellationToken.IsCancellationRequested)
                return document;

            modifiedRoot = modifiedRoot.ReplaceNode(classDeclaration, classModified);
            return document.WithSyntaxRoot(modifiedRoot);
        }

        
        private class RegionTriviaRewriter : CSharpSyntaxRewriter
        {
            private int visitedRegionTriviaCounter;
            private int triviaListsRemovedCounter;
            private bool alreadyVisited;

            private Stack<RegionDirectiveTriviaSyntax> regionDirectives;

            private readonly Document document;
            private readonly SemanticModel semanticModel;
            private readonly SyntaxNode deletedNode; 
            private readonly RegionDirectiveTriviaSyntax regionDirectiveTrivia;
            private readonly CancellationToken cancellationToken;



            public RegionTriviaRewriter(Document aDocument, SemanticModel aSemanticModel,
                                        SyntaxNode aDeletedNode, CancellationToken cToken)
            {
                document = aDocument;
                semanticModel = aSemanticModel;
                cancellationToken = cToken;
                /*endRegionDirectiveTrivia = aEndRegionDirectiveTrivia;
                regionDirectiveTrivia = aRegionDirectiveTrivia;
                */
                PXContext pxContext = new PXContext(semanticModel.Compilation);
                // = new FieldAttributesRegister(pxContext);
            }

            public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
            {
                return base.VisitEndRegionDirectiveTrivia(node);
            }
            public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
            {
                regionDirectives.Push(node);
                if (cancellationToken.IsCancellationRequested)
                    return null;

                if (visitedRegionTriviaCounter < Int32.MaxValue)
                    visitedRegionTriviaCounter++;

                

                return null;
            }


        }
    }
}
