using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var diagnostic = context.Diagnostics
                .FirstOrDefault(d => d.Id.Equals(Descriptors.PX1092_MissingAttributesOnActionHandler.Id));

            if (diagnostic?.Properties == null || !diagnostic.Properties.TryGetValue(FixOptionKey, out string value) ||
				!Enum.TryParse(value, out FixOption option))
            {
                return Task.CompletedTask;
            }

            var codeActionName = nameof(Resources.PX1092Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(
				codeActionName,
				cancellation => FixActionHandlerAttributes(context.Document, context.Span, option, cancellation),
				equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
			return Task.CompletedTask;
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

            var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			var pxButtonAttributeList = pxContext.AttributeTypes.PXButtonAttribute.GetAttributeList();
			var pxUIFieldAttributeList = pxContext.AttributeTypes.PXUIFieldAttribute.Type.GetAttributeList();
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
    }
}
