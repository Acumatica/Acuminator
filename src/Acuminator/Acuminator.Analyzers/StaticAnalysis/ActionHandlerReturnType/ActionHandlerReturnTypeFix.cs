using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.ActionHandlerReturnType
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ActionHandlerReturnTypeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1013_PXActionHandlerInvalidReturnType.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var root = await context.Document
                .GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            if (!(root?.FindNode(context.Span) is MethodDeclarationSyntax node))
            {
                return;
            }

            var changeReturnTypeTitle = nameof(Resources.PX1013Fix).GetLocalized().ToString();
            var codeFixAction = new InvalidPXActionSignatureFix.ChangeSignatureAction(changeReturnTypeTitle, context.Document, node);

            context.RegisterCodeFix(codeFixAction, context.Diagnostics);
        }
    }
}
