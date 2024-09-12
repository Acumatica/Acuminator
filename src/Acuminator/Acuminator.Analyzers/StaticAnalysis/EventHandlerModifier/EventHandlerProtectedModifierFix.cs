using System.Composition;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.CodeActions;
using Acuminator.Utilities.Roslyn.Semantic;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class EventHandlerProtectedModifierFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(
				Descriptors.PX1078_EventHandlersShouldBeProtectedVirtual.Id,
				Descriptors.PX1078_EventHandlersShouldNotBeSealed.Id,
				Descriptors.PX1078_EventHandlersShouldNotBeExplicitInterfaceImplementations.Id,
				Descriptors.PX1078_EventHandlersInSealedClassesShouldNotBePrivate.Id);

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

			if (methodSymbol == null)
			{
				return;
			}

			IMethodSymbol methodSymbolNotNull = methodSymbol;

			context.CancellationToken.ThrowIfCancellationRequested();

			if (methodSymbolNotNull.IsOverride == true)
			{
				if (methodSymbolNotNull.IsSealed == true)
				{
					var removeSealedTitle = nameof(Resources.PX1078Fix_RemoveSealed).GetLocalized().ToString();
					var codeFixAction = new RemoveSealedAction(removeSealedTitle, context.Document, node);

					context.RegisterCodeFix(codeFixAction, diagnostic);
				}
			}
			else if (methodSymbolNotNull.MethodKind == MethodKind.ExplicitInterfaceImplementation)
			{
				var removeExplicitInterface = nameof(Resources.PX1078Fix_RemoveExplicitInterface).GetLocalized().ToString();
				var codeFixAction = new RemoveExplicitInterfaceAction(removeExplicitInterface, context.Document, node);

				context.RegisterCodeFix(codeFixAction, diagnostic);
			}
			else
			{
				if (!methodSymbolNotNull.ImplementsInterface())
				{
					var accessibilityModifier = methodSymbolNotNull.ContainingType.IsSealed
						? SyntaxKind.PublicKeyword
						: SyntaxKind.ProtectedKeyword;

					var makeProtectedTitle = methodSymbolNotNull.ContainingType.IsSealed ?
						nameof(Resources.PX1077Fix).GetLocalized(SyntaxFactory.Token(accessibilityModifier).Text).ToString() :
						nameof(Resources.PX1078Fix).GetLocalized().ToString();
					var codeFixAction = new MakeProtectedVirtualAction(makeProtectedTitle, context.Document, node, accessibilityModifier, !methodSymbolNotNull.ContainingType.IsSealed);

					context.RegisterCodeFix(codeFixAction, diagnostic);
				}
			}
		}
	}


}
