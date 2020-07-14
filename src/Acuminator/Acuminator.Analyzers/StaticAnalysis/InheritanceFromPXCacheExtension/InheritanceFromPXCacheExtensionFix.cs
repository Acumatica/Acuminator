using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Utilities.Roslyn.Constants;
using System;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InheritanceFromPXCacheExtensionFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1009_InheritanceFromPXCacheExtension.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
											 .ConfigureAwait(false);
			var node = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();

			if (node?.BaseList == null)
				return;

			string title = nameof(Resources.PX1009Fix).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(title, 
													  cancellation => ChangeBaseTypeToPXCacheExtensionOverloadAsync(context.Document, root, node, cancellation),
													  equivalenceKey: title);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private static async Task<Document> ChangeBaseTypeToPXCacheExtensionOverloadAsync(Document document, SyntaxNode root, ClassDeclarationSyntax node,
																						  CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken)
											  .ConfigureAwait(false);
			if (semanticModel == null)
				return document;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			INamedTypeSymbol classType = semanticModel.GetDeclaredSymbol(node, cancellationToken);

			if (classType == null)
				return document;
	
			var genericArgs = GetNewGenericArgumentsForFix(classType, pxContext);	
			BaseTypeSyntax oldBaseNode = node.BaseList.Types.FirstOrDefault();

			if (oldBaseNode == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var generator = SyntaxGenerator.GetGenerator(document);
			var cacheExtensionTypeNode = generator.GenericName(TypeNames.PXCacheExtension, genericArgs) as TypeSyntax;
			var newBaseNode = SyntaxFactory.SimpleBaseType(cacheExtensionTypeNode);
			var newRoot = root.ReplaceNode(oldBaseNode, newBaseNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private static List<ITypeSymbol> GetNewGenericArgumentsForFix(INamedTypeSymbol classType, PXContext pxContext)
		{
			var genericArgs = new List<ITypeSymbol>();
			INamedTypeSymbol currentType = classType.BaseType;

			while (currentType != null && !currentType.Equals(pxContext.PXCacheExtensionType))
			{
				if (currentType.Name == TypeNames.PXCacheExtension)
				{
					genericArgs.AddRange(currentType.TypeArguments);
					break;
				}

				genericArgs.Add(currentType);
				currentType = currentType.BaseType;
			}

			return genericArgs;
		}
	}
}
