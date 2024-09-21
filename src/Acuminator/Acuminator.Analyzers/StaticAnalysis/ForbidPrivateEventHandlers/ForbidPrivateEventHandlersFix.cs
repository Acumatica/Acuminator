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
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;

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

			var registerCodeFix = DiagnosticUtils.IsFlagSet(diagnostic, StaticAnalysis.DiagnosticProperty.RegisterCodeFix);
			var isContainingTypeSealed = DiagnosticUtils.IsFlagSet(diagnostic, DiagnosticProperty.IsContainingTypeSealed);
			var addVirtualModifier = DiagnosticUtils.IsFlagSet(diagnostic, DiagnosticProperty.AddVirtualModifier);

			if (!registerCodeFix)
			{
				return Task.CompletedTask;
			}

			var accessibilityModifier = isContainingTypeSealed
				? SyntaxKind.PublicKeyword
				: SyntaxKind.ProtectedKeyword;

			var modifierFormatArg = ForbidPrivateEventHandlersAnalyzer.GetModifierFormatArg(accessibilityModifier, addVirtualModifier);

			var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized(modifierFormatArg).ToString();


			context.CancellationToken.ThrowIfCancellationRequested();


			var codeFixAction = CodeAction.Create(
				makeProtectedTitle,
				cToken => ChangeAccessibilityModifierAsync(context.Document, context.Span, accessibilityModifier, addVirtualModifier, cToken),
				equivalenceKey: Resources.PX1077Fix);

			context.RegisterCodeFix(codeFixAction, diagnostic);
			return Task.CompletedTask;
		}

		private static readonly List<SyntaxKind> ModifiersToBeRemoved =
		[
			SyntaxKind.PrivateKeyword,
			SyntaxKind.PublicKeyword,
			SyntaxKind.ProtectedKeyword,
			SyntaxKind.InternalKeyword,
			SyntaxKind.VirtualKeyword
		];

		private static async Task<Document> ChangeAccessibilityModifierAsync(
			Document document,
			TextSpan span,
			SyntaxKind accessibilityModifier,
			bool addVirtual,
			CancellationToken cancellationToken)
		{
			var root = await document
				.GetSyntaxRootAsync(cancellationToken)
				.ConfigureAwait(false);

			if (root?.FindNode(span) is not MethodDeclarationSyntax method)
			{
				return document;
			}

			var newToken = SyntaxFactory.Token(accessibilityModifier);
			var firstToken = method.Modifiers.FirstOrDefault();
			var removeLeadingTriviaFromReturnType = firstToken == default;

			if (firstToken != default)
			{
				newToken = newToken.WithTriviaFrom(firstToken);
			}
			else
			{
				newToken = newToken.WithLeadingTrivia(method.ReturnType.GetLeadingTrivia());

			}

			var modifiers = new List<SyntaxToken>
			{
				newToken
			};

			if (addVirtual)
			{
				modifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
			}

			var restOfModifiers = method.Modifiers.Select(m => SyntaxFactory.Token(m.Kind())).ToList();

			if (firstToken != default && !ModifiersToBeRemoved.Contains(firstToken.Kind()))
			{
				modifiers.Add(SyntaxFactory.Token(firstToken.Kind()));
				modifiers.AddRange(restOfModifiers.Skip(1).Where(m => !ModifiersToBeRemoved.Contains(m.Kind())));
			}
			else
			{
				modifiers.AddRange(restOfModifiers.Where(m => !ModifiersToBeRemoved.Contains(m.Kind())));
			}

			var localDeclaration = method.WithModifiers(SyntaxFactory.TokenList(modifiers));
			if (removeLeadingTriviaFromReturnType)
			{
				localDeclaration = localDeclaration.WithReturnType(localDeclaration.ReturnType.WithoutLeadingTrivia());
			}

			var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(method, localDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
