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
			var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			var node = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			
			if (node?.BaseList != null)
			{
				string title = nameof(Resources.PX1009Fix).GetLocalized().ToString();
				context.RegisterCodeFix(CodeAction.Create(title, async c =>
					{
						var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
						var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
						var classType = semanticModel.GetDeclaredSymbol(node);
						var generator = SyntaxGenerator.GetGenerator(context.Document);

						var genericArgs = new List<ITypeSymbol>();
						INamedTypeSymbol currentType = classType.BaseType;
						while (currentType != null
							&& !currentType.Equals(pxContext.PXCacheExtensionType))
						{
							if (currentType.Name == TypeNames.PXCacheExtension)
							{
								genericArgs.AddRange(currentType.TypeArguments);
								break;
							}

							genericArgs.Add(currentType);
							currentType = currentType.BaseType;
						}
						
						var newRoot = await context.Document.GetSyntaxRootAsync(c);
						var oldBaseNode = node.BaseList.Types.First();
						var newBaseNode = SyntaxFactory.SimpleBaseType((TypeSyntax) generator.GenericName(TypeNames.PXCacheExtension, genericArgs));
						newRoot = newRoot.ReplaceNode(oldBaseNode, newBaseNode);

						return context.Document.WithSyntaxRoot(newRoot);
					}, title),
					context.Diagnostics);
			}
		}
	}
}
