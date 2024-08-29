using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InheritanceFromPXCacheExtensionMakeSealedFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1011_InheritanceFromPXCacheExtension.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var dacExtNode = root?.FindNode(context.Span).FirstAncestorOrSelf<ClassDeclarationSyntax>();
			
			if (dacExtNode != null)
			{	
				string title = nameof(Resources.PX1011Fix).GetLocalized().ToString();
				var document = context.Document;
				var codeAction = CodeAction.Create(title,
												cancellation => MakeDacExtensionSealedAsync(document, root!, dacExtNode, cancellation),
												equivalenceKey: title);

				context.RegisterCodeFix(codeAction, diagnostic);
			}
		}

		private static Task<Document> MakeDacExtensionSealedAsync(Document document, SyntaxNode root, ClassDeclarationSyntax dacExtNode, 
																  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var newNode = dacExtNode.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword));
			var newRoot = root.ReplaceNode(dacExtNode, newNode);
			var modifiedDocument = document.WithSyntaxRoot(newRoot);

			cancellation.ThrowIfCancellationRequested();

			return Task.FromResult(modifiedDocument);
		}
	}
}
