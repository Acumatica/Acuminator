using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class IncompatibleDacPropertyAndFieldAttributeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty.Id);

			if (diagnostic == null || !diagnostic.IsRegisteredForCodeFix())
				return;

			SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			SyntaxNode codeFixNode = root?.FindNode(context.Span);

			if (codeFixNode == null)
				return;
			
			if (codeFixNode is AttributeSyntax attribute)
			{
				RegisterCodeFix(root, attribute, context);
			}
			else
			{
				RegisterCodeFixForPropertyType(root, codeFixNode, context, diagnostic);
			}		
		}

		private void RegisterCodeFixForPropertyType(SyntaxNode root, SyntaxNode codeFixNode, CodeFixContext context, Diagnostic diagnostic)
		{
			Location attributeLocation = diagnostic.AdditionalLocations.FirstOrDefault();

			if (attributeLocation == null || context.CancellationToken.IsCancellationRequested)
				return;

			AttributeSyntax attributeNode = root.FindNode(attributeLocation.SourceSpan) as AttributeSyntax;

			if (attributeNode == null || context.CancellationToken.IsCancellationRequested)
				return;
			
			RegisterCodeFix(root, attributeNode, context);
		}

		private void RegisterCodeFix(SyntaxNode root, AttributeSyntax attributeNode, CodeFixContext context)
		{
			string codeActionName = nameof(Resources.PX1021PropertyFix).GetLocalized().ToString();

			CodeAction codeAction =
				CodeAction.Create(codeActionName,
								  cToken => ChangePropertyTypeToAttributeType(context.Document, root, attributeNode, cToken),
								  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);		
		}

		private async Task<Document> ChangePropertyTypeToAttributeType(Document document, SyntaxNode root, AttributeSyntax attributeNode,
																	   CancellationToken cancellationToken)
		{
			PropertyDeclarationSyntax propertyNode = attributeNode.Parent<PropertyDeclarationSyntax>();

			if (propertyNode == null || cancellationToken.IsCancellationRequested)
				return document;

			SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null || cancellationToken.IsCancellationRequested)
				return document;

			ITypeSymbol attributeType = semanticModel.GetTypeInfo(attributeNode).Type;

			if (attributeType == null || cancellationToken.IsCancellationRequested)
				return document;
			
			PXContext pxContext = new PXContext(semanticModel.Compilation);
			FieldTypeAttributesRegister attributesRegister = new FieldTypeAttributesRegister(pxContext);
			FieldTypeAttributeInfo info = attributesRegister.GetFieldTypeAttributeInfo(attributeType);

			if (!info.IsFieldAttribute || info.FieldType == null)
				return document;

			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
			TypeSyntax replacingTypeNode = generator.TypeExpression(info.FieldType) as TypeSyntax;

			if (info.FieldType.IsValueType)
			{
				replacingTypeNode = generator.NullableTypeExpression(replacingTypeNode) as TypeSyntax;
			}

			replacingTypeNode = replacingTypeNode.WithTrailingTrivia(propertyNode.Type.GetTrailingTrivia());
	
			if (replacingTypeNode == null || cancellationToken.IsCancellationRequested)
				return document;
;
			var propertyModified = propertyNode.WithType(replacingTypeNode);

			if (propertyModified == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedRoot = root.ReplaceNode(propertyNode, propertyModified);
			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}