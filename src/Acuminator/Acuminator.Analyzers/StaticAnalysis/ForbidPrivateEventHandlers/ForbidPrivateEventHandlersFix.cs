using System;
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

			SyntaxToken newAccessibilityModifier = SyntaxFactory.Token(accessibilityModifier);
			SyntaxToken firstModifier = eventHandler.Modifiers.FirstOrDefault();
			bool hasModifiers = firstModifier != default;

			newAccessibilityModifier = CopyTriviaForNewAccessModifier(newAccessibilityModifier, hasModifiers, firstModifier, eventHandler);

			var modifiers = new List<SyntaxToken>
			{
				newAccessibilityModifier
			};

			if (addVirtual)
			{
				modifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
			}

			if (hasModifiers && !ShouldModifierBeRemoved(firstModifier.Kind()))
			{
				// if the previously first token was not a modifier to be removed, we need to add it back _without_ the leading trivia.
				// That's why we add it separately first, and then we add the rest.

				modifiers.Add(firstModifier.WithoutTrivia().WithTrailingTrivia(firstModifier.TrailingTrivia));
				modifiers.AddRange(FilterModifiers(eventHandler.Modifiers, skip: 1));
			}
			else
			{
				modifiers.AddRange(FilterModifiers(eventHandler.Modifiers, skip: 0));
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

		private static SyntaxToken CopyTriviaForNewAccessModifier(in SyntaxToken newAccessibilityModifier, bool hasModifiers,
																  SyntaxToken firstModifier, MethodDeclarationSyntax eventHandler)
		{
			// Preserve the leading trivia of the first token, if it exists. If not, take over the leading trivia from the return type.
			SyntaxToken newAccessibilityModifierWithTrivia;

			if (hasModifiers)
			{
				SyntaxTriviaList leadingTriviaToAdd  = firstModifier.LeadingTrivia;
				SyntaxTriviaList trailingTriviaToAdd = firstModifier.TrailingTrivia;

				// Get the leading and trailing trivia of the modifiers that are about to be removed.

				var modifiersToBeRemoved = eventHandler.Modifiers.Skip(1)	// skip the first token, as the trivia from it is always added
																 .Where(m => ShouldModifierBeRemoved(m.Kind()))
																 .ToList();
				if (modifiersToBeRemoved.Count > 0)
				{
					var leadingTriviaFromRemovedModifiers  = modifiersToBeRemoved.Where(m => m.HasLeadingTrivia)
																				 .SelectMany(m => m.LeadingTrivia);
					var trailingTriviaFromRemovedModifiers = modifiersToBeRemoved.Where(m => m.HasTrailingTrivia)
																				 .SelectMany(m => m.TrailingTrivia);

					if (leadingTriviaFromRemovedModifiers.Any())
						leadingTriviaToAdd = leadingTriviaToAdd.AddRange(leadingTriviaFromRemovedModifiers);

					if (trailingTriviaFromRemovedModifiers.Any())
						trailingTriviaToAdd = trailingTriviaToAdd.AddRange(trailingTriviaFromRemovedModifiers);
				}

				newAccessibilityModifierWithTrivia = newAccessibilityModifier.WithLeadingTrivia(leadingTriviaToAdd)
																			 .WithTrailingTrivia(trailingTriviaToAdd);
			}
			else
			{
				newAccessibilityModifierWithTrivia = newAccessibilityModifier.WithLeadingTrivia(eventHandler.ReturnType.GetLeadingTrivia());
			}

			return newAccessibilityModifierWithTrivia;
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
