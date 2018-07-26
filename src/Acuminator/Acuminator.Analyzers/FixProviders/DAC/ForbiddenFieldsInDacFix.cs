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
    public class ForbiddenFieldsInDacFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            return Task.Run(() =>
            {
                var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1027_ForbiddenFieldsInDacDeclaration.Id);

                if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
                    return;

                    
                string codeActionName = nameof(Resources.PX1027Fix).GetLocalized().ToString();
                CodeAction codeAction = CodeAction.Create(codeActionName,
                                                          cToken => DeleteForbiddenFieldsAsync(context.Document, context.Span, cToken),
                                                          equivalenceKey: codeActionName);

                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }, context.CancellationToken);
        }

        private async Task<Document> DeleteForbiddenFieldsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode diagnosticNode = root?.FindNode(span);
           

            if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
                return document;
            //var trackingRoot = root.TrackNodes(diagnosticNode);
            ClassDeclarationSyntax classDeclaration = diagnosticNode.Parent<ClassDeclarationSyntax>();

            var rewriterWalker = new RegionClassRewriter((diagnosticNode is ClassDeclarationSyntax) ?
                                                                (diagnosticNode as ClassDeclarationSyntax)?.Identifier.Text :
                                                                (diagnosticNode as PropertyDeclarationSyntax)?.Identifier.Text,
                                                         cancellationToken);
            var classModified = rewriterWalker.Visit(classDeclaration) as ClassDeclarationSyntax;

            //var modifiedNode = trackingRoot.GetCurrentNode(diagnosticNode);
            var modifiedRoot = root.ReplaceNode(classDeclaration, classModified);

            return document.WithSyntaxRoot(modifiedRoot);
        }
    }


    public class RegionClassRewriter : CSharpSyntaxRewriter
    {
        private Stack<RegionDirectiveTriviaSyntax> regionsStack;
        private string removableIdentifier;
        private CancellationToken cancellationToken;

        public RegionClassRewriter(string aRemovableIdentifier,
                                   CancellationToken cToken)
                                : base(visitIntoStructuredTrivia: true)
        {
            regionsStack = new Stack<RegionDirectiveTriviaSyntax>();
            removableIdentifier = aRemovableIdentifier.ToUpperInvariant();
            cancellationToken = cToken;
        }

        public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
        {
            if (regionsStack == null || cancellationToken.IsCancellationRequested)
                return null;
            regionsStack.Push(node);
            if (node.ToString().ToUpperInvariant().Contains(removableIdentifier))
                return null;
            return node;

        }
        public override SyntaxNode VisitEndRegionDirectiveTrivia(EndRegionDirectiveTriviaSyntax node)
        {
            if (regionsStack == null || regionsStack.Count == 0 || cancellationToken.IsCancellationRequested)
                return null;
            RegionDirectiveTriviaSyntax regionStart = regionsStack.Pop();
            if (regionStart.ToString().ToUpperInvariant().Contains(removableIdentifier))
                return null;
            return node;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;
            if (string.Equals(node.Identifier.Text, removableIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                var result = base.VisitClassDeclaration(node);
                
                return null;
            }
            var result2 = base.VisitClassDeclaration(node);
            return result2;
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;
            if (string.Equals(node.Identifier.Text, removableIdentifier, StringComparison.OrdinalIgnoreCase))
                return null;
            return base.VisitPropertyDeclaration(node);
        }
    }
}
