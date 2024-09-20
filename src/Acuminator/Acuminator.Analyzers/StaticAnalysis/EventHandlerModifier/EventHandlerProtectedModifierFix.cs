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
using Acuminator.Analyzers.StaticAnalysis.PrivateEventHandlers;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class EventHandlerProtectedModifierFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.PX1077_EventHandlersShouldNotBePrivate.Id,
				Descriptors.PX1077_EventHandlersShouldBeProtectedVirtual.Id,
				Descriptors.PX1077_EventHandlersShouldNotBeExplicitInterfaceImplementations.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var registerCodeFix = DiagnosticUtils.IsFlagSet(diagnostic, DiagnosticProperty.RegisterCodeFix);
			var isContainingTypeSealed = DiagnosticUtils.IsFlagSet(diagnostic, PrivateEventHandlers.DiagnosticProperty.IsContainingTypeSealed);

			if (!registerCodeFix)
			{
				return Task.CompletedTask;
			}

			var accessibilityModifier = isContainingTypeSealed
				? SyntaxKind.PublicKeyword
				: SyntaxKind.ProtectedKeyword;

			var addVirtualModifier = !isContainingTypeSealed;

			var modifierFormatArg = EventHandlerModifierAnalyzer.GetModifierFormatArg(accessibilityModifier, addVirtualModifier);

			var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized(modifierFormatArg).ToString();


			context.CancellationToken.ThrowIfCancellationRequested();


			var codeFixAction = CodeAction.Create(
				makeProtectedTitle,
				cToken => ChangeAccessibilityModifierAsync(context.Document, context.Span, accessibilityModifier, addVirtualModifier, cToken),
				equivalenceKey: makeProtectedTitle);

			context.RegisterCodeFix(codeFixAction, diagnostic);
			return Task.CompletedTask;
		}

		private static readonly List<SyntaxKind> AccessibilityModifiers =
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

			var index = GetFirstModifierIndex(method);

			var newToken = SyntaxFactory.Token(accessibilityModifier);
			if (index > -1)
			{
				newToken = newToken.WithTriviaFrom(method.Modifiers[index]);
			}

			var newModifiers = method.Modifiers.Where(m => !AccessibilityModifiers.Contains(m.Kind()));

			var syntaxModifiers = SyntaxFactory.TokenList(newToken);

			if (addVirtual)
			{
				syntaxModifiers = syntaxModifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
			}

			syntaxModifiers = syntaxModifiers.AddRange(newModifiers);

			var localDeclaration = method.WithModifiers(syntaxModifiers);

			var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = oldRoot!.ReplaceNode(method, localDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}

		private static int GetFirstModifierIndex(MethodDeclarationSyntax method)
		{
			var index = -1;
			for (var i = 0; i < method.Modifiers.Count; i++)
			{
				if (AccessibilityModifiers.Contains(method.Modifiers[i].Kind()))
				{
					index = i;
					break;
				}
			}

			return index;
		}
	}
}
