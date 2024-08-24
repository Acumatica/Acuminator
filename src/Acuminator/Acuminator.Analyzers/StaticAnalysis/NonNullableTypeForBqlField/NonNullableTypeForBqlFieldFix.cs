using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class NonNullableTypeForBqlFieldFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1014_NonNullableTypeForBqlField.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

			if (diagnostic == null)
				return;

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (root?.FindNode(context.Span) is not PropertyDeclarationSyntax propertyNode)
				return;

			string title = nameof(Resources.PX1014Fix).GetLocalized().ToString();
			Document document = context.Document;
			var codeAction = CodeAction.Create(title,
											   cancellation => MakePropertyTypeNullable(document, root, propertyNode, cancellation),
											   equivalenceKey: title);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private static Task<Document> MakePropertyTypeNullable(Document document, SyntaxNode root, PropertyDeclarationSyntax propertyNode,
																CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var nullableTypeNode = SyntaxFactory.NullableType(propertyNode.Type);
			var modifiedRoot = root.ReplaceNode(propertyNode.Type, nullableTypeNode);

			if (modifiedRoot == null)
				return Task.FromResult(document);

			cancellation.ThrowIfCancellationRequested();

			var modifiedDocument = document.WithSyntaxRoot(modifiedRoot);
			return Task.FromResult(modifiedDocument);
		}
	}
}
