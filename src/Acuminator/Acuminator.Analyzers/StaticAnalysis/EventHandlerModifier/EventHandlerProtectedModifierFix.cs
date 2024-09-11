using System.Composition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class EventHandlerProtectedModifierFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var root = await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			if (root?.FindNode(context.Span) is not MethodDeclarationSyntax node)
			{
				return;
			}

			var semanticModel = await context.Document.GetSemanticModelAsync();
			var methodSymbol = semanticModel.GetDeclaredSymbol(node);

			context.CancellationToken.ThrowIfCancellationRequested();

			if (methodSymbol?.IsOverride == true)
			{
				if (methodSymbol?.IsSealed == true)
				{
					var removeSealedTitle = nameof(Resources.PX1078Fix_RemoveSealed).GetLocalized().ToString();
					var codeFixAction = new RemoveSealedAction(removeSealedTitle, context.Document, node);

					context.RegisterCodeFix(codeFixAction, diagnostic);
				}
			}
			else
			{
				var makeProtectedTitle = nameof(Resources.PX1078Fix).GetLocalized().ToString();
				var codeFixAction = new MakeProtectedVirtualAction(makeProtectedTitle, context.Document, node);

				context.RegisterCodeFix(codeFixAction, diagnostic);
			}
		}
	}

	internal class RemoveSealedAction : CodeAction
	{
		private readonly string _title;
		private readonly Document _document;
		private readonly MethodDeclarationSyntax _method;

		public override string Title => _title;
		public override string EquivalenceKey => _title;

		public RemoveSealedAction(string title, Document document, MethodDeclarationSyntax method)
		{
			_title = title;
			_document = document;
			_method = method;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var newModifiers = _method.Modifiers.Where(m => !m.IsKind(SyntaxKind.SealedKeyword));

			var localDeclaration = _method.WithModifiers(SyntaxFactory.TokenList(newModifiers));

			var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration);

			return _document.WithSyntaxRoot(newRoot);
		}
	}

	internal class MakeProtectedVirtualAction : CodeAction
	{
		private readonly string _title;
		private readonly Document _document;
		private readonly MethodDeclarationSyntax _method;

		public override string Title => _title;
		public override string EquivalenceKey => _title;

		public MakeProtectedVirtualAction(string title, Document document, MethodDeclarationSyntax method)
		{
			_title = title;
			_document = document;
			_method = method;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var newModifiers = _method.Modifiers.Where(m =>
													!m.IsKind(SyntaxKind.PrivateKeyword) &&
													!m.IsKind(SyntaxKind.PublicKeyword) &&
													!m.IsKind(SyntaxKind.ProtectedKeyword) &&
													!m.IsKind(SyntaxKind.InternalKeyword) &&
													!m.IsKind(SyntaxKind.VirtualKeyword) &&
													!m.IsKind(SyntaxKind.OverrideKeyword) &&
													!m.IsKind(SyntaxKind.SealedKeyword));

			var syntaxModifiers = SyntaxFactory.TokenList(
				SyntaxFactory.Token(SyntaxKind.ProtectedKeyword),
				SyntaxFactory.Token(SyntaxKind.VirtualKeyword)
				);

			syntaxModifiers = syntaxModifiers.AddRange(newModifiers);

			var localDeclaration = _method.WithModifiers(syntaxModifiers);

			var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration);

			return _document.WithSyntaxRoot(newRoot);
		}
	}
}
