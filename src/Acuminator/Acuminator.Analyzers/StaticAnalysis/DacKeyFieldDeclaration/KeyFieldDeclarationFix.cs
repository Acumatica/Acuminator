using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
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
		private const string IsKey = "IsKey";
		private const string True = "true";
			
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1055_DacKeyFieldBound.Id);

		public override  Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return Task.Run(() =>
			{
				var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1055_DacKeyFieldBound.Id);

				if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
					return;

				string codeActionIdentityKeyName = nameof(Resources.PX1055Fix1).GetLocalized().ToString();
				CodeAction codeActionIdentityKey = CodeAction.Create(codeActionIdentityKeyName,
																	cToken => RemoveKeysFromFieldsAsync(context.Document, context, context.Span, cToken, diagnostic, false),
																	equivalenceKey: codeActionIdentityKeyName);

				string codeActionBoundKeysName = nameof(Resources.PX1055Fix2).GetLocalized().ToString();
				CodeAction codeActionBoundKeys = CodeAction.Create(	codeActionBoundKeysName,
																	cToken => RemoveKeysFromFieldsAsync(context.Document, context, context.Span, cToken, diagnostic, true),
																	equivalenceKey: codeActionBoundKeysName);
				context.RegisterCodeFix(codeActionIdentityKey, context.Diagnostics);
				context.RegisterCodeFix(codeActionBoundKeys, context.Diagnostics);
			}, context.CancellationToken);
			
		}

		private async Task<Document> RemoveKeysFromFieldsAsync(Document document,CodeFixContext context, TextSpan span, CancellationToken cToken, Diagnostic diagnostic, bool editIdentityAttribute)
		{

			SemanticModel semanticModel = await document.GetSemanticModelAsync(cToken).ConfigureAwait(false);
			PXContext pxContext = new PXContext(semanticModel.Compilation);
			AttributeInformation attributeInformation = new AttributeInformation(pxContext);

			cToken.ThrowIfCancellationRequested();

			Location[] attributeLocations = diagnostic.AdditionalLocations.ToArray();
			var identityAttributeType = pxContext.FieldAttributes.PXDBIdentityAttribute;
			var longIdentityAttributeType = pxContext.FieldAttributes.PXDBLongIdentityAttribute;

			if (attributeLocations == null)
				return document;

			SyntaxNode root = await document.GetSyntaxRootAsync(cToken).ConfigureAwait(false);

			Document tempDocument = document;

			List<AttributeArgumentSyntax> deletedNodes = new List<AttributeArgumentSyntax>();

			foreach (var attributeLocation in attributeLocations)
			{
				AttributeSyntax attributeNode = root?.FindNode(attributeLocation.SourceSpan) as AttributeSyntax;

				if (attributeNode == null)
					return document;

				ITypeSymbol attributeType = semanticModel.GetTypeInfo(attributeNode, cToken).Type;

				if (attributeType == null)
					return document;

				bool isIdentityAttribute = attributeInformation.IsAttributeDerivedFromClass(attributeType, identityAttributeType) || 
										   attributeInformation.IsAttributeDerivedFromClass(attributeType, longIdentityAttributeType);

				if (!isIdentityAttribute ^ editIdentityAttribute)
				{
					var deletedNode = attributeNode.ArgumentList.Arguments.Where(a => a.NameEquals?.Name.Identifier.ValueText.Equals(IsKey)??false && 
																					  (a.Expression as LiteralExpressionSyntax).Token.ValueText.Equals(True));

					deletedNodes.AddRange(deletedNode);
				}

			}


			var newRoot = root.RemoveNodes(deletedNodes,SyntaxRemoveOptions.KeepNoTrivia);

			return document.WithSyntaxRoot(newRoot);
		}

		
	}
}
