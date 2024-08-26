
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class ConstructorInGraphExtensionCodeFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension.Id);

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var root = await context.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			var constructorNode = root?.FindNode(context.Span)?.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();

			if (constructorNode != null)
			{
				string title = nameof(Resources.PX1040Fix).GetLocalized().ToString();
				var codeAction = CodeAction.Create(title,
												   cToken => MoveCodeFromConstructorToInitialize(context.Document, root!, constructorNode, cToken),
												   equivalenceKey: title);
				context.RegisterCodeFix(codeAction, diagnostic);
			}
		}

		private async Task<Document> MoveCodeFromConstructorToInitialize(Document document, SyntaxNode root, 
																		 ConstructorDeclarationSyntax constructorNode,
																		 CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			if (semanticModel == null)
				return document;

			var methodSymbol = semanticModel.GetDeclaredSymbol(constructorNode, cancellationToken);
			var initializeSymbol = methodSymbol?.ContainingType
												.GetMethods("Initialize")
												.FirstOrDefault(m => m.IsOverride
																	 && m.DeclaredAccessibility == Accessibility.Public
																	 && m.ReturnsVoid
																	 && m.Parameters.IsEmpty);
			var initializeNode = await initializeSymbol.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) as MethodDeclarationSyntax;

			if (initializeNode != null)
			{
				// Copy the body from the constructor to the beggining of the existing Initialize() method
				root = root.TrackNodes(constructorNode, initializeNode);
				initializeNode = root.GetCurrentNode(initializeNode);

				var statements = CombineStatements(constructorNode, initializeNode!);
				var blockNode = SyntaxFactory.Block(statements);
				var newInitializeNode = initializeNode!.WithBody(blockNode);

				root = root.ReplaceNode(initializeNode, newInitializeNode);
			}
			else
			{
				// Generate Initialize() method declaration with the body from the constructor
				var graphExtNode = constructorNode.Parent<ClassDeclarationSyntax>();

				if (graphExtNode != null)
				{
					int constructorNodeIndex = graphExtNode.Members.IndexOf(constructorNode);

					if (constructorNodeIndex < 0)
						constructorNodeIndex = graphExtNode.Members.Count - 1;

					var constructorStatements = GetStatements(constructorNode.Body, constructorNode.ExpressionBody) ?? default;
					var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

					var newInitializeNode = (MethodDeclarationSyntax) syntaxGenerator.MethodDeclaration(
						"Initialize",
						accessibility: Accessibility.Public,
						modifiers: DeclarationModifiers.Override,
						statements: constructorStatements);

					root = root.TrackNodes(constructorNode, graphExtNode);
					graphExtNode = root.GetCurrentNode(graphExtNode);

					var newMembers = graphExtNode!.Members.Insert(constructorNodeIndex, newInitializeNode);
					var newGraphExtNode = graphExtNode.WithMembers(newMembers);

					root = root.ReplaceNode(graphExtNode, newGraphExtNode);
				}
			}

			cancellationToken.ThrowIfCancellationRequested();

			// Remove the constructor
			var constructorNodeToRemove = root.GetCurrentNode(constructorNode) ?? constructorNode;
			root = root.RemoveNode(constructorNodeToRemove, SyntaxRemoveOptions.KeepUnbalancedDirectives)!;

			return document.WithSyntaxRoot(root);
		}

		private static SyntaxList<StatementSyntax> CombineStatements(ConstructorDeclarationSyntax constructorNode,
																	 MethodDeclarationSyntax initializeNode)
		{
			var constructorStatements = GetStatements(constructorNode.Body, constructorNode.ExpressionBody);
			var initializeStatements = GetStatements(initializeNode.Body, initializeNode.ExpressionBody);

			bool constructorStatementsEmpty = constructorStatements?.Count is null or 0;
			bool initializeStatementsEmpty = initializeStatements?.Count is null or 0;

			if (constructorStatementsEmpty && initializeStatementsEmpty)
				return default;
			else if (constructorStatementsEmpty)
				return initializeStatements!.Value;
			else if (initializeStatementsEmpty)
				return constructorStatements!.Value;
			else
				return constructorStatements!.Value.AddRange(initializeStatements!.Value);
		}

		private static SyntaxList<StatementSyntax>? GetStatements(BlockSyntax? body, ArrowExpressionClauseSyntax? expressionBody)
		{
			if (body != null)
				return body.Statements;
			else if (expressionBody != null)
			{
				var expression = expressionBody.Expression;
				var expressionStatement = SyntaxFactory.ExpressionStatement(expression);

				return SyntaxFactory.SingletonList<StatementSyntax>(expressionStatement);
			}
			else
				return null;
		}
	}
}
