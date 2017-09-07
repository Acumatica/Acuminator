using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using PX.Data;

namespace PX.Analyzers.FixProviders
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InheritanceFromPXCacheExtensionMakeSealedFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1011_InheritanceFromPXCacheExtension.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			var node = root.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			
			if (node != null)
			{	
				string title = nameof(Resources.PX1011Fix).GetLocalized().ToString();
				context.RegisterCodeFix(CodeAction.Create(title, async c =>
					{
						var newRoot = await context.Document.GetSyntaxRootAsync(c);
						var newNode = node.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword));
						newRoot = newRoot.ReplaceNode(node, newNode);
						return context.Document.WithSyntaxRoot(newRoot);
					}, title),
					context.Diagnostics);
			}
		}
	}
}
