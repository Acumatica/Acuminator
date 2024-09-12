using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.CodeActions
{
	internal class RemoveExplicitInterfaceAction : CodeAction
	{
		private readonly string _title;
		private readonly Document _document;
		private readonly MethodDeclarationSyntax _method;

		public override string Title => _title;
		public override string EquivalenceKey => _title;

		public RemoveExplicitInterfaceAction(string title, Document document, MethodDeclarationSyntax method)
		{
			_title = title;
			_document = document;
			_method = method;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (_method.ExplicitInterfaceSpecifier != null)
			{
				var localDeclaration = _method.RemoveNode(_method.ExplicitInterfaceSpecifier!, SyntaxRemoveOptions.KeepLeadingTrivia);

				if (localDeclaration != null)
				{
					var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration!);

					return _document.WithSyntaxRoot(newRoot);
				}
			}

			return _document;
		}
	}
}
