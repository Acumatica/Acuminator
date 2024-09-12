using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.Helpers;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.CodeActions
{
	internal class ChangeAccessibilityModifierAction : CodeAction
	{
		private readonly string _title;
		private readonly Document _document;
		private readonly MethodDeclarationSyntax _method;
		private readonly SyntaxKind _accessibilityModifier;

		public override string Title => _title;
		public override string EquivalenceKey => _title;

		public ChangeAccessibilityModifierAction(string title, Document document, MethodDeclarationSyntax method, SyntaxKind accessibilityModifier)
		{
			_title = title;
			_document = document;
			_method = method;
			_accessibilityModifier = accessibilityModifier;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var syntaxModifiers = AnalyzerHelper.CreateTokenListWithAccessibilityModifier(_accessibilityModifier, _method.Modifiers, SyntaxFacts.IsAccessibilityModifier, false);

			var localDeclaration = _method.WithModifiers(syntaxModifiers);

			var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration);

			return _document.WithSyntaxRoot(newRoot);
		}
	}
}
