using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Acuminator.Analyzers.FixProviders
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class ConstructorInGraphExtensionCodeFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			if (context.CancellationToken.IsCancellationRequested) return;

			var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			var node = root.FindNode(context.Span)?.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();

			if (node != null)
			{
				string title = nameof(Resources.PX1040Fix).GetLocalized().ToString();
				context.RegisterCodeFix(CodeAction.Create(title, 
					c => MoveCodeFromConstructorToInitialize(context.Document, node, c), title),
					context.Diagnostics);
			}
		}

		private async Task<Document> MoveCodeFromConstructorToInitialize(
			Document document,
			ConstructorDeclarationSyntax constructorNode,
			CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) return document;

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var methodSymbol = semanticModel.GetDeclaredSymbol(constructorNode);
			var newRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			var initializeSymbol = methodSymbol.ContainingType
				.GetMembers("Initialize")
				.OfType<IMethodSymbol>()
				.FirstOrDefault(m => m.IsOverride
				                     && m.DeclaredAccessibility == Accessibility.Public
				                     && m.ReturnsVoid
				                     && m.Parameters.IsEmpty);

			if (initializeSymbol != null)
			{
				// Copy the body from the constructor to the beggining of the existing Initialize() method
				var initializeNode = await initializeSymbol
					.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as MethodDeclarationSyntax;
				if (initializeNode == null) return document;

				var statements = constructorNode.Body.Statements.AddRange(initializeNode.Body.Statements);
				newRoot = newRoot.TrackNodes(constructorNode, initializeNode);
				initializeNode = newRoot.GetCurrentNode(initializeNode);
				var newInitializeNode = initializeNode.WithBody(initializeNode.Body.WithStatements(statements));
				newRoot = newRoot.ReplaceNode(initializeNode, newInitializeNode);
			}
			else
			{
				// Generate Initialize() method declaration with the body from the constructor
				var classNode = constructorNode.Parent<ClassDeclarationSyntax>();
				if (classNode == null || cancellationToken.IsCancellationRequested) return document;

				var syntaxGenerator = SyntaxGenerator.GetGenerator(document);
				var initializeNode = (MethodDeclarationSyntax) syntaxGenerator.MethodDeclaration(
					"Initialize",
					accessibility: Accessibility.Public,
					modifiers: DeclarationModifiers.Override,
					statements: constructorNode.Body.Statements);

				newRoot = newRoot.TrackNodes(constructorNode, classNode);
				classNode = newRoot.GetCurrentNode(classNode);
				var newClassNode = classNode.AddMembers(initializeNode);
				newRoot = newRoot.ReplaceNode(classNode, newClassNode);
			}

			if (cancellationToken.IsCancellationRequested) return document;

			// Remove the constructor
			newRoot = newRoot.RemoveNode(newRoot.GetCurrentNode(constructorNode) ?? constructorNode,
				SyntaxRemoveOptions.KeepUnbalancedDirectives);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
