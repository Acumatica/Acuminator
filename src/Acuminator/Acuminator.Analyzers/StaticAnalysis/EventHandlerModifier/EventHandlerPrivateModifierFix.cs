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
	public class EventHandlerPrivateModifierFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1077_EventHandlersShouldNotBePrivate.Id);

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

			var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized().ToString();
			var codeFixAction = new MakeProtectedAction(makeProtectedTitle, context.Document, node);

			context.RegisterCodeFix(codeFixAction, diagnostic);
		}
	}

	internal class MakeProtectedAction : CodeAction
	{
		private readonly string _title;
		private readonly Document _document;
		private readonly MethodDeclarationSyntax _method;

		public override string Title => _title;
		public override string EquivalenceKey => _title;

		public MakeProtectedAction(string title, Document document, MethodDeclarationSyntax method)
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
													!m.IsKind(SyntaxKind.InternalKeyword));

			var syntaxModifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
				.AddRange(newModifiers);

			var localDeclaration = _method.WithModifiers(syntaxModifiers);

			var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration);

			return _document.WithSyntaxRoot(newRoot);
		}
	}
}
