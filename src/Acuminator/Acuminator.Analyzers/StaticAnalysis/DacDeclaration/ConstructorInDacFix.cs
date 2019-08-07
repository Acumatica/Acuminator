using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.DacDeclaration
{
    [Shared]
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class ConstructorInDacFix : PXCodeFixProvider
	{
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1028_ConstructorInDacDeclaration.Id);
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            return Task.Run(() =>
            {
                var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1028_ConstructorInDacDeclaration.Id);

                if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
                    return;

                string codeActionName = nameof(Resources.PX1028Fix).GetLocalized().ToString();
                CodeAction codeAction = CodeAction.Create(codeActionName,
                                                          cToken => DeleteConstructorsAsync(context.Document, context.Span, cToken),
                                                          equivalenceKey: codeActionName);

                context.RegisterCodeFix(codeAction, context.Diagnostics);
				base.RegisterSuppressionComment(context);
            }, context.CancellationToken);
        }

        private async Task<Document> DeleteConstructorsAsync(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode diagnosticNode = root?.FindNode(span);

            if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
                return document;

            var modifiedRoot = root.RemoveNode(diagnosticNode, SyntaxRemoveOptions.KeepTrailingTrivia | SyntaxRemoveOptions.KeepLeadingTrivia);

            return document.WithSyntaxRoot(modifiedRoot);
        }

       
    }
}
