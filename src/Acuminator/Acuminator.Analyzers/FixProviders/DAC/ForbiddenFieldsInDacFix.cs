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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.CSharp.Formatting;

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
            ClassDeclarationSyntax classDeclaration = diagnosticNode.Parent<ClassDeclarationSyntax>();

            var rewriterWalker = new RegionClassRewriter((diagnosticNode is ClassDeclarationSyntax) ?
                                                                (diagnosticNode as ClassDeclarationSyntax)?.Identifier.Text :
                                                                (diagnosticNode as PropertyDeclarationSyntax)?.Identifier.Text,
                                                         cancellationToken);
            var classModified = rewriterWalker.Visit(classDeclaration);
            
            if (cancellationToken.IsCancellationRequested)
                return document;
            //Format tabulations
            AdhocWorkspace workspace = new AdhocWorkspace();
            OptionSet options = workspace.Options; 
            options = options.WithChangedOption(FormattingOptions.UseTabs,LanguageNames.CSharp,false)
                             .WithChangedOption(FormattingOptions.TabSize, LanguageNames.CSharp, 4)
                             .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, 4);
            workspace.Options = options;
            var modifiedRoot = Formatter.Format(root.ReplaceNode(classDeclaration, classModified),workspace,options);

            if (cancellationToken.IsCancellationRequested)
                return document;
            return document.WithSyntaxRoot(modifiedRoot);
        }
    }


    public class RegionClassRewriter : CSharpSyntaxRewriter
    {
        //private Stack<RegionDirectiveTriviaSyntax> regionsStack;
        private Stack<SyntaxTrivia> deletedTriviaStack;
        private string removableIdentifier;
        private CancellationToken cancellationToken;

        public RegionClassRewriter(string aRemovableIdentifier,
                                   CancellationToken cToken)
                                : base(visitIntoStructuredTrivia: true)
        {
            //regionsStack = new Stack<RegionDirectiveTriviaSyntax>();
            deletedTriviaStack = new Stack<SyntaxTrivia>();
            removableIdentifier = aRemovableIdentifier.ToUpperInvariant();
            cancellationToken = cToken;
        }

        public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
        {
            if (deletedTriviaStack == null || cancellationToken.IsCancellationRequested)
                return base.VisitTrivia(trivia);
            if (trivia.Kind() == SyntaxKind.RegionDirectiveTrivia)
            {
                deletedTriviaStack.Push(trivia);
                if (trivia.ToString().ToUpperInvariant().Contains(removableIdentifier))
                {
                    return SyntaxFactory.Tab;// SyntaxFactory.ElasticEndOfLine(" a") as SyntaxNode;
                }
            }
            if (trivia.Kind()  == SyntaxKind.EndRegionDirectiveTrivia)
            {
                if (deletedTriviaStack.Count == 0)
                    return base.VisitTrivia(trivia);
                SyntaxTrivia regionStart = deletedTriviaStack.Pop();
                if (regionStart.ToString().ToUpperInvariant().Contains(removableIdentifier))
                    return  SyntaxFactory.Tab;
            }
            return base.VisitTrivia(trivia);
        }
        

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;
            var properties = node.Members.OfType<PropertyDeclarationSyntax>();
            foreach (var property in properties)
            {
                if (string.Equals(property.Identifier.Text, removableIdentifier, StringComparison.OrdinalIgnoreCase))
                    node = node.RemoveNode(property,SyntaxRemoveOptions.KeepExteriorTrivia);
            }
            var classes = node.Members.OfType<ClassDeclarationSyntax>();
            foreach (var classElement in classes)
            {
                if (string.Equals(classElement.Identifier.Text, removableIdentifier, StringComparison.OrdinalIgnoreCase))
                    node = node.RemoveNode(classElement, SyntaxRemoveOptions.KeepExteriorTrivia);
            }
            
            var result2 = base.VisitClassDeclaration(node);
            return result2;
        }

    }
}
