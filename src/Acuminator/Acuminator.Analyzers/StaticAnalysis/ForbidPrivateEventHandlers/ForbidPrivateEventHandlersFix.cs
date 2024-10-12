using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.ForbidPrivateEventHandlers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class ForbidPrivateEventHandlersFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.Id,
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.Id,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.IsRegisteredForCodeFix(considerRegisteredByDefault: false))
				return Task.CompletedTask;

			var isContainingTypeSealed = diagnostic.IsFlagSet(PX1077DiagnosticProperty.IsContainingTypeSealed);
			var addVirtualModifier	   = diagnostic.IsFlagSet(PX1077DiagnosticProperty.AddVirtualModifier);

			var accessibilityModifier = isContainingTypeSealed
				? SyntaxKind.PublicKeyword
				: SyntaxKind.ProtectedKeyword;

			var modifierFormatArg = ForbidPrivateEventHandlersAnalyzer.GetModifiersText(isPublic: isContainingTypeSealed, addVirtualModifier);
			var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized(modifierFormatArg).ToString();

			context.CancellationToken.ThrowIfCancellationRequested();

			var codeFixAction = CodeAction.Create(
				makeProtectedTitle,
				cToken => ChangeAccessibilityModifierAsync(context.Document, context.Span, accessibilityModifier, addVirtualModifier, cToken),
				equivalenceKey: Resources.PX1077Fix);

			context.RegisterCodeFix(codeFixAction, diagnostic);
			return Task.CompletedTask;
		}

		private static async Task<Document> ChangeAccessibilityModifierAsync(Document document, TextSpan span, SyntaxKind accessibilityModifier,
																			 bool addVirtual, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			if (root?.FindNode(span) is not MethodDeclarationSyntax eventHandler)
			{
				return document;
			}

			var newToken = SyntaxFactory.Token(accessibilityModifier);
			var firstToken = eventHandler.Modifiers.FirstOrDefault();
			var hasModifiers = firstToken != default;

			// Preserve the leading trivia of the first token, if it exists. If not, take over the leading trivia from the return type.
			newToken = hasModifiers ?
				newToken.WithTriviaFrom(firstToken) :
				newToken.WithLeadingTrivia(eventHandler.ReturnType.GetLeadingTrivia());

			if (hasModifiers)
			{
				// Preserve the leading and trailing trivia of the modifiers that are about to be removed.

				var modifiersToBeRemoved = eventHandler.Modifiers
					.Skip(1) // skip the first token, as the trivia from it was already handled.
					.Where(m => ShouldModifierBeRemoved(m.Kind()));

				var leadingTrivia = modifiersToBeRemoved.SelectMany(m => m.LeadingTrivia);
				var trailingTrivia = modifiersToBeRemoved.SelectMany(m => m.TrailingTrivia);

				if (leadingTrivia.Count() > 0)
				{
					newToken = newToken.WithLeadingTrivia(leadingTrivia);
				}

				if (trailingTrivia.Count() > 0)
				{
					newToken = newToken.WithTrailingTrivia(trailingTrivia);
				}
			}

			var modifiers = new List<SyntaxToken>
			{
				newToken
			};

			if (addVirtual)
			{
				modifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
			}

			if (hasModifiers && !ShouldModifierBeRemoved(firstToken.Kind()))
			{
				// if the previously first token was not a modifier to be removed, we need to add it back _without_ the leading trivia.
				// That's why we add it separately first, and then we add the rest.

				modifiers.Add(firstToken.WithoutTrivia().WithTrailingTrivia(firstToken.TrailingTrivia));
				modifiers.AddRange(FilterModifiers(eventHandler.Modifiers, 1));
			}
			else
			{
				modifiers.AddRange(FilterModifiers(eventHandler.Modifiers, 0));
			}

			var newEventHandler = eventHandler.WithModifiers(SyntaxFactory.TokenList(modifiers));

			if (!hasModifiers)
			{
				// if there are no modifiers in the original method, we took over the leading trivia from the return type to the new modifier token.
				// now we need to remove it (the leading trivia) from the return type token.
				newEventHandler = newEventHandler.WithReturnType(newEventHandler.ReturnType.WithoutLeadingTrivia());
			}

			var newRoot = root.ReplaceNode(eventHandler, newEventHandler);

			return document.WithSyntaxRoot(newRoot);
		}

		private static IEnumerable<SyntaxToken> FilterModifiers(SyntaxTokenList modifiers, int skip)
			=> modifiers.Skip(skip)
						.Where(m => !ShouldModifierBeRemoved(m.Kind()));

		private static bool ShouldModifierBeRemoved(SyntaxKind modifierKind)
		{
			switch (modifierKind)
			{
				case SyntaxKind.PrivateKeyword:
				case SyntaxKind.PublicKeyword:
				case SyntaxKind.ProtectedKeyword:
				case SyntaxKind.InternalKeyword:
				case SyntaxKind.VirtualKeyword:
					return true;
				default:
					return false;
			}
		}
	}
}
