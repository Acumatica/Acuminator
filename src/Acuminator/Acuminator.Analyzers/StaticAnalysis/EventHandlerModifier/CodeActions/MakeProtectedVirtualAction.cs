using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.Helpers;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.CodeActions
{
	internal class MakeProtectedVirtualAction : CodeAction
	{
		private readonly string _title;
		private readonly Document _document;
		private readonly MethodDeclarationSyntax _method;
		private readonly SyntaxKind _accessibilityModifier;
		private readonly bool _addVirtual;

		private static readonly List<SyntaxKind> SyntaxKinds =
		[
			SyntaxKind.PrivateKeyword,
			SyntaxKind.PublicKeyword,
			SyntaxKind.ProtectedKeyword,
			SyntaxKind.InternalKeyword,
			SyntaxKind.VirtualKeyword,
			SyntaxKind.OverrideKeyword,
			SyntaxKind.SealedKeyword
		];

		public override string Title => _title;
		public override string EquivalenceKey => _title;

		public MakeProtectedVirtualAction(string title, Document document, MethodDeclarationSyntax method, SyntaxKind accessibilityModifier, bool addVirtual)
		{
			_title = title;
			_document = document;
			_method = method;
			_accessibilityModifier = accessibilityModifier;
			_addVirtual = addVirtual;
		}

		protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
		{
			var syntaxModifiers = AnalyzerHelper.CreateTokenListWithAccessibilityModifier(_accessibilityModifier, _method.Modifiers, SyntaxKinds.Contains, _addVirtual);

			var localDeclaration = _method.WithModifiers(syntaxModifiers);

			var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration);

			return _document.WithSyntaxRoot(newRoot);
		}
	}
}
