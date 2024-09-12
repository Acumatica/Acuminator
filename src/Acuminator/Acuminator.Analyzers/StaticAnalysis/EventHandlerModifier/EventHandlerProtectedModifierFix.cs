using System.Composition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeActions;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class EventHandlerProtectedModifierFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.Id,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations.Id);

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

			var isOverride = GetPropertyValue(diagnostic, "IsOverride");
			var isExplicitInterfaceImplementation = GetPropertyValue(diagnostic, "IsExplicitInterfaceImplementation");
			var implementsInterface = GetPropertyValue(diagnostic, "ImplementsInterface");
			var isContainingTypeSealed = GetPropertyValue(diagnostic, "IsContainingTypeSealed");

			context.CancellationToken.ThrowIfCancellationRequested();

			if (isOverride)
			{
				return;
			}

			if (isExplicitInterfaceImplementation)
			{
				var removeExplicitInterface = nameof(Resources.PX1077Fix_RemoveExplicitInterface).GetLocalized().ToString();
				var codeFixAction = new RemoveExplicitInterfaceAction(removeExplicitInterface, context.Document, node);

				context.RegisterCodeFix(codeFixAction, diagnostic);
			}
			else
			{
				if (!implementsInterface)
				{
					var accessibilityModifier = isContainingTypeSealed
						? SyntaxKind.PublicKeyword
						: SyntaxKind.ProtectedKeyword;

					var addVirtualModifier = !isContainingTypeSealed;

					var localizedText = SyntaxFactory.Token(accessibilityModifier).Text;
					if (addVirtualModifier)
					{
						localizedText += " " + SyntaxFactory.Token(SyntaxKind.VirtualKeyword).Text;
					}

					var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized(localizedText).ToString();
					var codeFixAction = new ChangeAccessibilityModifierAction(makeProtectedTitle, context.Document, node, accessibilityModifier, addVirtualModifier);

					context.RegisterCodeFix(codeFixAction, diagnostic);
				}
			}
		}

		private static bool GetPropertyValue(Diagnostic diagnostic, string propertyName)
		{
			if (diagnostic.TryGetPropertyValue(propertyName, out string? strValue) && bool.TryParse(strValue, out bool value))
			{
				return value;
			}

			return false;
		}

		internal class ChangeAccessibilityModifierAction : CodeAction
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
				SyntaxKind.OverrideKeyword
			];

			public override string Title => _title;
			public override string EquivalenceKey => _title;

			public ChangeAccessibilityModifierAction(string title, Document document, MethodDeclarationSyntax method, SyntaxKind accessibilityModifier, bool addVirtual)
			{
				_title = title;
				_document = document;
				_method = method;
				_accessibilityModifier = accessibilityModifier;
				_addVirtual = addVirtual;
			}

			protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
			{
				var index = -1;
				for (var i = 0; i < _method.Modifiers.Count; i++)
				{
					if (SyntaxKinds.Contains(_method.Modifiers[i].Kind()))
					{
						index = i;
						break;
					}
				}

				var newToken = SyntaxFactory.Token(_accessibilityModifier);
				if (index > -1)
				{
					newToken = newToken.WithTriviaFrom(_method.Modifiers[index]);
				}

				var newModifiers = _method.Modifiers.Where(m => !SyntaxKinds.Contains(m.Kind()));

				var syntaxModifiers = SyntaxFactory.TokenList(newToken);

				if (_addVirtual)
				{
					syntaxModifiers = syntaxModifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
				}

				syntaxModifiers = syntaxModifiers.AddRange(newModifiers);

				var localDeclaration = _method.WithModifiers(syntaxModifiers);

				var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
				var newRoot = oldRoot!.ReplaceNode(_method, localDeclaration);

				return _document.WithSyntaxRoot(newRoot);
			}
		}

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
}
