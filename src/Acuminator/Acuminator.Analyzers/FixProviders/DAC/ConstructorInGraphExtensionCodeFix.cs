using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Acuminator.Analyzers.FixProviders
{
	public class ConstructorInGraphExtensionCodeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			//var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			//var node = root.FindNode(context.Span) as PropertyDeclarationSyntax;

			//if (node != null)
			//{
			//	string title = nameof(Resources.PX1014Fix).GetLocalized().ToString();
			//	context.RegisterCodeFix(CodeAction.Create(title,
			//			c => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node.Type, SyntaxFactory.NullableType(node.Type)))), title),
			//		context.Diagnostics);
			//}
		}
	}
}
