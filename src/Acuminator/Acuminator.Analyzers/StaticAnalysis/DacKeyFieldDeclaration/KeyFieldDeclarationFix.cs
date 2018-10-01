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
		private const string IsKey = nameof(PX.Data.PXDBFieldAttribute.IsKey);
		
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.Id);

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1055_DacKeyFieldsWithIdentityKeyField.Id);

			context.CancellationToken.ThrowIfCancellationRequested();

			if (diagnostic == null)
				return;

			string codeActionIdentityKeyName = nameof(Resources.PX1055Fix1).GetLocalized().ToString();
			CodeAction codeActionIdentityKey = CodeAction.Create(codeActionIdentityKeyName,
																cToken => RemoveKeysFromFieldsAsync(context,
																									cToken, 
																									diagnostic, 
																									editIdentityAttribute:false),
																equivalenceKey: codeActionIdentityKeyName);

			string codeActionBoundKeysName = nameof(Resources.PX1055Fix2).GetLocalized().ToString();
			CodeAction codeActionBoundKeys = CodeAction.Create(	codeActionBoundKeysName,
																cToken => RemoveKeysFromFieldsAsync(context,
																									cToken, 
																									diagnostic, 
																									editIdentityAttribute: true),
																equivalenceKey: codeActionBoundKeysName);

			string codeActionRemoveIdentityColumnName = nameof(Resources.PX1055Fix3).GetLocalized().ToString();
			CodeAction codeActionRemoveIdentityColumn = CodeAction.Create(codeActionRemoveIdentityColumnName,
																			cToken => RemoveKeysFromFieldsAsync(context, 
																												cToken, 
																												diagnostic,
																												removeIdentityAttribute: true),
																			equivalenceKey: codeActionRemoveIdentityColumnName);

			context.RegisterCodeFix(codeActionIdentityKey, context.Diagnostics);
			context.RegisterCodeFix(codeActionBoundKeys, context.Diagnostics);
			context.RegisterCodeFix(codeActionRemoveIdentityColumn, context.Diagnostics);

			return ;
		}

		private async Task<Document> RemoveKeysFromFieldsAsync(CodeFixContext context, CancellationToken cToken, Diagnostic diagnostic, bool editIdentityAttribute = false, bool removeIdentityAttribute = false)
		{

			Document document = context.Document;
			Document tempDocument = document;

			SemanticModel semanticModel = await document.GetSemanticModelAsync(cToken).ConfigureAwait(false);
			var pxContext = new PXContext(semanticModel.Compilation);
			var attributeInformation = new AttributeInformation(pxContext);

			cToken.ThrowIfCancellationRequested();

			Location[] attributeLocations = diagnostic.AdditionalLocations.ToArray();

			SyntaxNode root = await document.GetSyntaxRootAsync(cToken).ConfigureAwait(false);

			List<SyntaxNode> deletedNodes = new List<SyntaxNode>();

			foreach (var attributeLocation in attributeLocations)
			{
				AttributeSyntax attributeNode = root?.FindNode(attributeLocation.SourceSpan) as AttributeSyntax;

				if (attributeNode == null)
					return document;

				ITypeSymbol attributeType = semanticModel.GetTypeInfo(attributeNode, cToken).Type;

				if (attributeType == null)
					return document;

				bool isIdentityAttribute = attributeInformation.IsAttributeDerivedFromClass(attributeType, pxContext.FieldAttributes.PXDBIdentityAttribute) || 
										   attributeInformation.IsAttributeDerivedFromClass(attributeType, pxContext.FieldAttributes.PXDBLongIdentityAttribute);

				if (removeIdentityAttribute == false && !isIdentityAttribute ^ editIdentityAttribute)
				{
					var deletedNode = attributeNode.ArgumentList.Arguments.Where(a => a.NameEquals?.Name.Identifier.ValueText.Equals(IsKey)??false && 
																					  (a.Expression as LiteralExpressionSyntax).Token.ValueText.Equals(bool.TrueString));

					deletedNodes.AddRange(deletedNode);
				}
				if (removeIdentityAttribute && isIdentityAttribute)
				{
					if ((attributeNode.Parent as AttributeListSyntax).Attributes.Count == 1)
						deletedNodes.Add(attributeNode.Parent);
					else
						deletedNodes.Add(attributeNode);
				}

			}

			var newRoot = root.RemoveNodes(deletedNodes,SyntaxRemoveOptions.KeepNoTrivia);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
