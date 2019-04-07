using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MissingTypeListAttributeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var node = root?.FindNode(context.Span) as PropertyDeclarationSyntax;

			if (node == null)
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					Resources.PX1002Fix,
					cancellationToken => InsertTypeAttributeAsync(context.Document, root, node, cancellationToken),
					Resources.PX1002Fix),
				context.Diagnostics);
		}

		private async Task<Document> InsertTypeAttributeAsync(Document document, SyntaxNode root, PropertyDeclarationSyntax propertyDeclaration,
															  CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			IPropertySymbol property = semanticModel?.GetDeclaredSymbol(propertyDeclaration, cancellationToken);

			if (property == null)
				return document;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			var lists = new List<INamedTypeSymbol> {
									pxContext.AttributeTypes.PXIntListAttribute.Type,
									pxContext.AttributeTypes.PXStringListAttribute.Type };

			var attributeTypes = property.GetAttributes()
										 .Select(a => a.AttributeClass);
			var listAttribute = attributeTypes.FirstOrDefault(attributeType =>
													lists.Any(lAttribute => attributeType.InheritsFromOrEquals(lAttribute, true)));

			cancellationToken.ThrowIfCancellationRequested();

			string attributeIdentifier = listAttribute.InheritsFromOrEquals(pxContext.AttributeTypes.PXIntListAttribute.Type)
				? pxContext.FieldAttributes.PXIntAttribute.Name
				: pxContext.FieldAttributes.PXStringAttribute.Name;

			AttributeSyntax attributeNode =
				SyntaxFactory.Attribute(
					SyntaxFactory.IdentifierName(attributeIdentifier));

			var attributes = propertyDeclaration.AttributeLists.Add(
				   SyntaxFactory.AttributeList(
					   SyntaxFactory.SingletonSeparatedList(attributeNode)));

			var modifiedRoot = root.ReplaceNode(propertyDeclaration,
												propertyDeclaration.WithAttributeLists(attributes));
			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}