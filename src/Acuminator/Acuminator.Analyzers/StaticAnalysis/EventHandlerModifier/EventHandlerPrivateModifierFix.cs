using System.Composition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.CodeActions;

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

			var semanticModel = await context.Document.GetSemanticModelAsync();
			var methodSymbol = semanticModel.GetDeclaredSymbol(node);

			context.CancellationToken.ThrowIfCancellationRequested();

			if (methodSymbol?.MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				var removeExplicitInterface = nameof(Resources.PX1078Fix_RemoveExplicitInterface).GetLocalized().ToString();
				var codeFixAction = new RemoveExplicitInterfaceAction(removeExplicitInterface, context.Document, node);

				context.RegisterCodeFix(codeFixAction, diagnostic);
			}
			else
			{
				var accessibilityModifier = methodSymbol?.ContainingType?.IsSealed == true
					? SyntaxKind.PublicKeyword
					: SyntaxKind.ProtectedKeyword;
				
				var makeProtectedTitle = nameof(Resources.PX1077Fix).GetLocalized(SyntaxFactory.Token(accessibilityModifier).Text).ToString();
				var codeFixAction = new ChangeAccessibilityModifierAction(makeProtectedTitle, context.Document, node, accessibilityModifier);

				context.RegisterCodeFix(codeFixAction, diagnostic);
			}
		}
	}


}
