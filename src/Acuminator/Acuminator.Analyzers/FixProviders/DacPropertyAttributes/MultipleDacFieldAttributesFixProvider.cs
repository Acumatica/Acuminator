using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities;


namespace Acuminator.Analyzers.FixProviders
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class MultipleDacFieldAttributesFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1023_DacPropertyMultipleFieldAttributes.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
													.ConfigureAwait(false);

			if (!(root?.FindNode(context.Span) is AttributeSyntax attributeNode))
				return;

			string codeActionName = nameof(Resources.PX1023Fix).GetLocalized().ToString();
			CodeAction codeAction =
				CodeAction.Create(codeActionName,
								  cToken => RemoveAllOtherAttributesFromPropertyAsync(context.Document, root, attributeNode, cToken),
								  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> RemoveAllOtherAttributesFromPropertyAsync(Document document, SyntaxNode root, AttributeSyntax attributeNode,
																			   CancellationToken cancellationToken)
		{
			var propertyDeclaration = attributeNode.Parent<PropertyDeclarationSyntax>();

			if (propertyDeclaration == null || cancellationToken.IsCancellationRequested)
				return document;

			SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null)
				return document;

			var rewriterWalker = new MultipleFieldAttributesRewriter(document, semanticModel, attributeNode, cancellationToken);
			var propertyModified = rewriterWalker.Visit(propertyDeclaration) as PropertyDeclarationSyntax;

			if (propertyModified == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedRoot = root.ReplaceNode(propertyDeclaration, propertyModified);
			return document.WithSyntaxRoot(modifiedRoot);
		}



		private class MultipleFieldAttributesRewriter : CSharpSyntaxRewriter
		{
			private int visitedAttributeListCounter;
			private bool someAttributeListWasRemoved;
			private bool alreadyMetRemainingAttribute;
			
			private readonly Document document;
			private readonly SemanticModel semanticModel;
			private readonly FieldAttributesRegister attributesRegister;
			private readonly AttributeSyntax remainingAttribute;
			private readonly CancellationToken cancellationToken;

			public MultipleFieldAttributesRewriter(Document aDocument, SemanticModel aSemanticModel, AttributeSyntax aRemainingAttribute,
												   CancellationToken cToken)
			{
				document = aDocument;
				semanticModel = aSemanticModel;
				cancellationToken = cToken;
				remainingAttribute = aRemainingAttribute;

				PXContext pxContext = new PXContext(semanticModel.Compilation);
				attributesRegister = new FieldAttributesRegister(pxContext);
			}

			public override SyntaxNode VisitAttributeList(AttributeListSyntax attributeListNode)
			{
				if (visitedAttributeListCounter < Int32.MaxValue)
				{
					visitedAttributeListCounter++;
				}

				if (cancellationToken.IsCancellationRequested)
					return null;

				bool oldAlreadyMetRemainingAttribute = alreadyMetRemainingAttribute;
				AttributeListSyntax modifiedAttributeListNode = base.VisitAttributeList(attributeListNode) as AttributeListSyntax;

				if (modifiedAttributeListNode == null || modifiedAttributeListNode.Attributes.Count == 0)
				{
					someAttributeListWasRemoved = true;
					return null;
				}

				bool containsRemainingAttribute = !oldAlreadyMetRemainingAttribute && alreadyMetRemainingAttribute;
				bool isSecondAttributeList = visitedAttributeListCounter == 2;
				bool firstAttributeListWasRemoved = someAttributeListWasRemoved;

				if (containsRemainingAttribute && isSecondAttributeList && firstAttributeListWasRemoved)
				{
					var trivia = modifiedAttributeListNode.GetLeadingTrivia()
														  .Prepend(SyntaxFactory.CarriageReturnLineFeed);
					modifiedAttributeListNode = modifiedAttributeListNode.WithLeadingTrivia(trivia);
				}

				return modifiedAttributeListNode;
			}

			public override SyntaxNode VisitAttribute(AttributeSyntax attributeNode)
			{
				if (!alreadyMetRemainingAttribute && attributeNode.Equals(remainingAttribute))
				{
					alreadyMetRemainingAttribute = true;
					return base.VisitAttribute(attributeNode);
				}

				ITypeSymbol attributeType = semanticModel.GetTypeInfo(attributeNode, cancellationToken).Type;

				if (attributeType == null || cancellationToken.IsCancellationRequested)
				{
					return !cancellationToken.IsCancellationRequested 
						? base.VisitAttribute(attributeNode)
						: null;
				}

				FieldAttributeInfo info = attributesRegister.GetFieldAttributeInfo(attributeType);
				return info.IsFieldAttribute ? null : base.VisitAttribute(attributeNode);
			}
		}
	}
}