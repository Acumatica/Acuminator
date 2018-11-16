using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.ActionHandlerAttributes
{
    internal enum FixOption
    {
        AddPXButtonAttribute,
        AddPXUIFieldAttribute,
        AddBothAttributes
    }

    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ActionHandlerAttributesFix : CodeFixProvider
    {
        internal const string FixOptionKey = nameof(FixOptionKey);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1092_MissingAttributesOnActionHandler.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics
                .FirstOrDefault(d => d.Id.Equals(Descriptors.PX1092_MissingAttributesOnActionHandler.Id));

            if (diagnostic == null || diagnostic.Properties == null)
            {
                return;
            }

            if (!diagnostic.Properties.TryGetValue(FixOptionKey, out string value))
            {
                return;
            }

            if (!Enum.TryParse(value, out FixOption option))
            {
                return;
            }

            var codeActionName = nameof(Resources.PX1092Fix).GetLocalized().ToString();
            var codeAction = CodeAction.Create(
                codeActionName,
                cancellation => FixActionHandlerAttributes(context.Document, context.Span, option, cancellation));

            context.RegisterCodeFix(codeAction, context.Diagnostics);
        }

        private async Task<Document> FixActionHandlerAttributes(Document document, TextSpan span, FixOption option, CancellationToken cancellation)
        {
			cancellation.ThrowIfCancellationRequested();

            var root = await document
                .GetSyntaxRootAsync(cancellation)
                .ConfigureAwait(false);

            if (!(root?.FindNode(span) is MethodDeclarationSyntax node))
            {
                return document;
            }

            var semanticModel = await document
                .GetSemanticModelAsync(cancellation)
                .ConfigureAwait(false);

            if (semanticModel == null)
            {
                return document;
            }

            var pxContext = new PXContext(semanticModel.Compilation);
			var pxButtonAttributeList = GetAttributeList(pxContext.AttributeTypes.PXButtonAttribute);
			var pxUIFieldAttributeList = GetAttributeList(pxContext.AttributeTypes.PXUIFieldAttribute.Type);
            var attributeListCollection = new List<AttributeListSyntax>();

            switch (option)
            {
                case FixOption.AddPXButtonAttribute:
					attributeListCollection.Add(pxButtonAttributeList);
                    break;
                case FixOption.AddPXUIFieldAttribute:
					attributeListCollection.Add(pxUIFieldAttributeList);
                    break;
                case FixOption.AddBothAttributes:
					attributeListCollection.Add(pxButtonAttributeList);
					attributeListCollection.Add(pxUIFieldAttributeList);
                    break;
            }

            var newNode = node.AddAttributeLists(attributeListCollection.ToArray());
            var newRoot = root.ReplaceNode(node, newNode);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }

		private AttributeListSyntax GetAttributeList(INamedTypeSymbol type)
		{
			var node = SyntaxFactory.Attribute(
				SyntaxFactory.IdentifierName(
					type.Name))
				.WithAdditionalAnnotations(Simplifier.Annotation);

			var list = SyntaxFactory.AttributeList(
				SyntaxFactory.SingletonSeparatedList(
					node));

			return list;
		}
    }
}
