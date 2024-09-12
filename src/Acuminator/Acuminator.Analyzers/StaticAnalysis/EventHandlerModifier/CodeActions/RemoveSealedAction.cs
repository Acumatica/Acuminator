using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.CodeActions
{
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
}
