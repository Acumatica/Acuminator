using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	/// <summary>
	/// A multiple attributes on DAC property fix base class.
	/// </summary>
	public abstract class MultipleAttributesOnDacPropertyFixBase : CodeFixProvider
	{
		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			string codeActionName = GetCodeActionName();
			Func<FieldTypeAttributeInfo, bool> removePredicate = GetRemoveAttributeByAttributeInfoPredicate();

			if (codeActionName.IsNullOrWhiteSpace() || removePredicate == null)
				return;

			SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(root?.FindNode(context.Span) is AttributeSyntax attributeNode))
				return;

			var propertyDeclaration = attributeNode.Parent<PropertyDeclarationSyntax>();

			if (propertyDeclaration == null)
				return;

			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
																.ConfigureAwait(false);
			if (semanticModel == null)
				return;

			CodeAction codeAction =
				CodeAction.Create(codeActionName,
								  cToken => RemoveAllOtherAttributesFromPropertyAsync(context.Document, root, attributeNode,
																					  propertyDeclaration, semanticModel, 
																					  removePredicate, cToken),
								  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		protected abstract string GetCodeActionName();

		protected abstract Func<FieldTypeAttributeInfo, bool> GetRemoveAttributeByAttributeInfoPredicate();

		private Task<Document> RemoveAllOtherAttributesFromPropertyAsync(Document document, SyntaxNode root, AttributeSyntax attributeNode,
																		 PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel,
																		 Func<FieldTypeAttributeInfo, bool> removePredicate, CancellationToken cancellationToken)
		{	
			var rewriterWalker = new MultipleAttributesRemover(document, semanticModel, attributeNode, removePredicate, cancellationToken);
			var propertyModified = rewriterWalker.Visit(propertyDeclaration) as PropertyDeclarationSyntax;

			cancellationToken.ThrowIfCancellationRequested();

			var modifiedRoot = root.ReplaceNode(propertyDeclaration, propertyModified);
			return Task.FromResult(document.WithSyntaxRoot(modifiedRoot));		
		}



		private class MultipleAttributesRemover : CSharpSyntaxRewriter
		{
			private const int MaxVisitedAttributesCount = 100;
			private const int MaxRemovedAttributesCount = 100;

			private int _visitedAttributeListCounter;
			private int _attributeListsRemovedCounter;
			
			private readonly Document _document;
			private readonly SemanticModel _semanticModel;
			private readonly FieldTypeAttributesRegister _attributesRegister;
			private readonly AttributeSyntax _remainingAttribute;
			private readonly Func<FieldTypeAttributeInfo, bool> _attributeToRemovePredicate;
			private readonly CancellationToken _cancellationToken;

			public MultipleAttributesRemover(Document document, SemanticModel semanticModel, AttributeSyntax remainingAttribute,
											 Func<FieldTypeAttributeInfo, bool> attributeToRemovePredicate, CancellationToken cToken)
			{
				_document = document;
				_semanticModel = semanticModel;
				_cancellationToken = cToken;
				_remainingAttribute = remainingAttribute;
				_attributeToRemovePredicate = attributeToRemovePredicate;

				PXContext pxContext = new PXContext(_semanticModel.Compilation, codeAnalysisSettings: null);
				_attributesRegister = new FieldTypeAttributesRegister(pxContext);
			}

            public override SyntaxNode VisitAttributeList(AttributeListSyntax attributeListNode)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (_visitedAttributeListCounter < MaxVisitedAttributesCount)
					_visitedAttributeListCounter++;

				var attributesToCheck = attributeListNode.Attributes;
				var modifiedAttributes = new List<AttributeSyntax>(attributesToCheck.Count);

				foreach (AttributeSyntax attribute in attributesToCheck)
				{
					_cancellationToken.ThrowIfCancellationRequested();

					if (!IsAttributeToRemove(attribute))
						modifiedAttributes.Add(attribute);
				}

				bool allPreviousAttributeListWereRemoved = _attributeListsRemovedCounter > 0 &&
														   _attributeListsRemovedCounter == (_visitedAttributeListCounter - 1);

				if (modifiedAttributes.Count == attributesToCheck.Count && !allPreviousAttributeListWereRemoved)
					return attributeListNode;
				else if (modifiedAttributes.Count == 0)
				{
					if (_attributeListsRemovedCounter < MaxRemovedAttributesCount)
					{
						_attributeListsRemovedCounter++;
					}

					return null;
				}

				AttributeListSyntax modifiedAttributeListNode = attributeListNode.WithAttributes(SyntaxFactory.SeparatedList(modifiedAttributes));

				if (allPreviousAttributeListWereRemoved)
				{
					var trivia = modifiedAttributeListNode.GetLeadingTrivia().Prepend(SyntaxFactory.CarriageReturnLineFeed);
					modifiedAttributeListNode = modifiedAttributeListNode.WithLeadingTrivia(trivia);
				}

				return modifiedAttributeListNode;
			}

			public bool IsAttributeToRemove(AttributeSyntax attributeNode)
			{
				if (attributeNode.Equals(_remainingAttribute))
					return false;

				ITypeSymbol attributeType = _semanticModel.GetTypeInfo(attributeNode, _cancellationToken).Type;
				_cancellationToken.ThrowIfCancellationRequested();

				if (attributeType == null)
					return false;
				
				var attributeInfos = _attributesRegister.GetFieldTypeAttributeInfos(attributeType);
				return attributeInfos.Any(_attributeToRemovePredicate);
			}
		}
	}
}