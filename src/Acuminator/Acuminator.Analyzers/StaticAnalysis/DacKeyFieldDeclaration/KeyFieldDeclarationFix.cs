#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class KeyFieldDeclarationFix : CodeFixProvider
	{
		private enum CodeFixModes
		{
			EditIdentityAttribute,
			EditKeyFieldAttributes,
			RemoveIdentityAttribute
		}

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
		
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.Id);

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.Id);

			if (diagnostic == null)
				return Task.FromResult(false);

			Document document = context.Document;
			List<Location> attributeLocations = diagnostic.AdditionalLocations.ToList(capacity: diagnostic.AdditionalLocations.Count + 1);
			attributeLocations.Add(diagnostic.Location);

			context.CancellationToken.ThrowIfCancellationRequested();

			string codeActionIdentityKeyName = nameof(Resources.PX1055FixEditKeyFieldAttributes).GetLocalized().ToString();
			CodeAction codeActionIdentityKey = CodeAction.Create(codeActionIdentityKeyName,
																cToken => RemoveKeysFromFieldsAsync(document,
																									cToken,
																									attributeLocations,
																									CodeFixModes.EditKeyFieldAttributes),
																equivalenceKey: codeActionIdentityKeyName);

			string codeActionBoundKeysName = nameof(Resources.PX1055FixEditIdentityAttribute).GetLocalized().ToString();
			CodeAction codeActionBoundKeys = CodeAction.Create(codeActionBoundKeysName,
																cToken => RemoveKeysFromFieldsAsync(document,
																									cToken,
																									attributeLocations,
																									CodeFixModes.EditIdentityAttribute),
																equivalenceKey: codeActionBoundKeysName);

			string codeActionRemoveIdentityColumnName = nameof(Resources.PX1055FixRemoveIdentityAttribute).GetLocalized().ToString();
			CodeAction codeActionRemoveIdentityColumn = CodeAction.Create(codeActionRemoveIdentityColumnName,
																			cToken => RemoveKeysFromFieldsAsync(document,
																												cToken,
																												attributeLocations,
																												CodeFixModes.RemoveIdentityAttribute),
																			equivalenceKey: codeActionRemoveIdentityColumnName);
			context.RegisterCodeFix(codeActionIdentityKey, diagnostic);
			context.RegisterCodeFix(codeActionBoundKeys, diagnostic);
			context.RegisterCodeFix(codeActionRemoveIdentityColumn, diagnostic);

			return Task.FromResult(true);
		}

		private async Task<Document> RemoveKeysFromFieldsAsync(Document document, CancellationToken cancellationToken,
															   List<Location> attributeLocations, CodeFixModes mode)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var semanticModelTask = document.GetSemanticModelAsync(cancellationToken);
			var rootTask = document.GetSyntaxRootAsync(cancellationToken);

			await Task.WhenAll(semanticModelTask, rootTask).ConfigureAwait(false);
			var (semanticModel, root) = (semanticModelTask.Result, rootTask.Result);

			if (semanticModel == null || root == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			List<SyntaxNode>? nodesToDelete = GetNodesToDelete(attributeLocations, mode, root, semanticModel, cancellationToken);
			if (nodesToDelete.IsNullOrEmpty())
				return document;

			SyntaxNode newRoot;
			if (mode == CodeFixModes.RemoveIdentityAttribute)
				newRoot = root.RemoveNodes(nodesToDelete, SyntaxRemoveOptions.KeepExteriorTrivia);
			else
				newRoot = root.RemoveNodes(nodesToDelete, SyntaxRemoveOptions.KeepNoTrivia);

			return document.WithSyntaxRoot(newRoot);
		}

		private List<SyntaxNode>? GetNodesToDelete(List<Location> attributeLocations, CodeFixModes mode, SyntaxNode root,
												   SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			var pxContext                 = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			var attributesInfoProvider    = new AcumaticaAttributesInfoProvider(pxContext);
			List<SyntaxNode> deletedNodes = new List<SyntaxNode>();

			foreach (var attributeLocation in attributeLocations)
			{
				cancellationToken.ThrowIfCancellationRequested();

				AttributeSyntax? attributeNode = root.FindNode(attributeLocation.SourceSpan) as AttributeSyntax;
				if (attributeNode == null)
					return null;

				ITypeSymbol? attributeType = semanticModel.GetTypeInfo(attributeNode, cancellationToken).Type;
				if (attributeType == null)
					return null;

				bool isIdentityAttribute = 
					attributesInfoProvider.IsAttributeDerivedFromOtherAttribute(attributeType, pxContext.FieldAttributes.PXDBIdentityAttribute) ||
					attributesInfoProvider.IsAttributeDerivedFromOtherAttribute(attributeType, pxContext.FieldAttributes.PXDBLongIdentityAttribute);

				if ((mode == CodeFixModes.EditIdentityAttribute && isIdentityAttribute) ||
					(mode == CodeFixModes.EditKeyFieldAttributes && !isIdentityAttribute))
				{
					IEnumerable<AttributeArgumentSyntax> deletedAttributeArgumentNodes = GetIsKeyEQTrueArguments(attributeNode);

					deletedNodes.AddRange(deletedAttributeArgumentNodes);
				}

				if (mode == CodeFixModes.RemoveIdentityAttribute && isIdentityAttribute)
				{
					if (attributeNode.Parent is AttributeListSyntax attributeListNode && attributeListNode.Attributes.Count == 1)
						deletedNodes.Add(attributeListNode);
					else
						deletedNodes.Add(attributeNode);
				}
			}

			return deletedNodes;
		}

		private IEnumerable<AttributeArgumentSyntax> GetIsKeyEQTrueArguments(AttributeSyntax attributeNode)
		{
			var arguments = attributeNode.ArgumentList.Arguments;

			if (arguments.Count == 0)
				return Enumerable.Empty<AttributeArgumentSyntax>();

			return arguments.Where(attributeArgument => QueryIfIsKeyAttributeArgument(attributeArgument) && 
														CheckIfAttributeArgumentValueIsTrue(attributeArgument));
		}

		private bool QueryIfIsKeyAttributeArgument(AttributeArgumentSyntax? attributeArgument) =>
			PropertyNames.Attributes.IsKey.Equals(attributeArgument?.NameEquals?.Name?.Identifier.ValueText, StringComparison.OrdinalIgnoreCase);

		private bool CheckIfAttributeArgumentValueIsTrue(AttributeArgumentSyntax? attributeArgument) =>
			attributeArgument?.Expression is LiteralExpressionSyntax argumentValue
				? bool.TrueString.Equals(argumentValue.Token.ValueText, StringComparison.OrdinalIgnoreCase)
				: false;
	}
}