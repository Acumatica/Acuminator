using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

		private const string IsKey = nameof(PX.Data.PXDBFieldAttribute.IsKey);

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.Id);

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.Id);

			if (diagnostic == null)
				return;

			Document document = context.Document;

			string codeActionIdentityKeyName = nameof(Resources.PX1055Fix1).GetLocalized().ToString();
			CodeAction codeActionIdentityKey = CodeAction.Create(codeActionIdentityKeyName,
																cToken => RemoveKeysFromFieldsAsync(document,
																									cToken,
																									diagnostic,
																									CodeFixModes.EditKeyFieldAttributes),
																equivalenceKey: codeActionIdentityKeyName);

			string codeActionBoundKeysName = nameof(Resources.PX1055Fix2).GetLocalized().ToString();
			CodeAction codeActionBoundKeys = CodeAction.Create(codeActionBoundKeysName,
																cToken => RemoveKeysFromFieldsAsync(document,
																									cToken,
																									diagnostic,
																									CodeFixModes.EditIdentityAttribute),
																equivalenceKey: codeActionBoundKeysName);

			string codeActionRemoveIdentityColumnName = nameof(Resources.PX1055Fix3).GetLocalized().ToString();
			CodeAction codeActionRemoveIdentityColumn = CodeAction.Create(codeActionRemoveIdentityColumnName,
																			cToken => RemoveKeysFromFieldsAsync(document,
																												cToken,
																												diagnostic,
																												CodeFixModes.RemoveIdentityAttribute),
																			equivalenceKey: codeActionRemoveIdentityColumnName);

			context.RegisterCodeFix(codeActionIdentityKey, context.Diagnostics);
			context.RegisterCodeFix(codeActionBoundKeys, context.Diagnostics);
			context.RegisterCodeFix(codeActionRemoveIdentityColumn, context.Diagnostics);

			return;
		}

		private async Task<Document> RemoveKeysFromFieldsAsync(Document document,
																CancellationToken cancellationToken,
																Diagnostic diagnostic,
																CodeFixModes mode)
		{


			SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var pxContext = new PXContext(semanticModel.Compilation);
			var attributeInformation = new AttributeInformation(pxContext);

			cancellationToken.ThrowIfCancellationRequested();

			Location[] attributeLocations = diagnostic.AdditionalLocations.ToArray();

			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			List<SyntaxNode> deletedNodes = new List<SyntaxNode>();

			foreach (var attributeLocation in attributeLocations)
			{
				AttributeSyntax attributeNode = root?.FindNode(attributeLocation.SourceSpan) as AttributeSyntax;

				if (attributeNode == null)
					return document;

				ITypeSymbol attributeType = semanticModel.GetTypeInfo(attributeNode, cancellationToken).Type;

				if (attributeType == null)
					return document;

				bool isIdentityAttribute = attributeInformation.IsAttributeDerivedFromClass(attributeType, pxContext.FieldAttributes.PXDBIdentityAttribute) ||
										   attributeInformation.IsAttributeDerivedFromClass(attributeType, pxContext.FieldAttributes.PXDBLongIdentityAttribute);


				if ((mode == CodeFixModes.EditIdentityAttribute && isIdentityAttribute) ||
					(mode == CodeFixModes.EditKeyFieldAttributes && !isIdentityAttribute))
				{
					IEnumerable<AttributeArgumentSyntax> deletedNode = GetIsKeyEQTrueArguments(attributeNode);

					deletedNodes.AddRange(deletedNode);
				}
				if (mode == CodeFixModes.RemoveIdentityAttribute && isIdentityAttribute)
				{
					if ((attributeNode.Parent as AttributeListSyntax).Attributes.Count == 1)
						deletedNodes.Add(attributeNode.Parent);
					else
						deletedNodes.Add(attributeNode);
				}
			}

			SyntaxNode newRoot;
			if (mode == CodeFixModes.RemoveIdentityAttribute)
				newRoot = root.RemoveNodes(deletedNodes, SyntaxRemoveOptions.KeepExteriorTrivia);
			else
				newRoot = root.RemoveNodes(deletedNodes, SyntaxRemoveOptions.KeepNoTrivia);

			return document.WithSyntaxRoot(newRoot);
		}

		private IEnumerable<AttributeArgumentSyntax> GetIsKeyEQTrueArguments(AttributeSyntax attributeNode)
		{
			return attributeNode.ArgumentList.Arguments.Where(a => a.NameEquals?.Name.Identifier.ValueText.Equals(IsKey) ?? false &&
															(a.Expression as LiteralExpressionSyntax).Token.ValueText.Equals(bool.TrueString));
		}

	}
}