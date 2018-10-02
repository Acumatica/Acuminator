using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class PXGraphCreationForBqlQueriesFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1072_PXGraphCreationForBqlQueries.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			var node = root?.FindNode(context.Span) as ArgumentSyntax;

			if (node == null || context.CancellationToken.IsCancellationRequested)
				return;

			string codeActionName = nameof(Resources.PX1072Fix).GetLocalized().ToString();

			foreach (var diagnostic in context.Diagnostics)
			{
				for (int i = 0;
					diagnostic.Properties.TryGetValue(
						PXGraphCreationForBqlQueriesAnalyzer.IdentifierNamePropertyPrefix + i,
						out string value);
					i++)
				{
					string identifierName = value;

					if (identifierName.IsNullOrWhiteSpace()) continue;
					
					var codeAction = CodeAction.Create(codeActionName, 
						ct => ReplaceIdentifier(context.Document, root, node, identifierName, context.CancellationToken),
						equivalenceKey: codeActionName + identifierName);
					context.RegisterCodeFix(codeAction, diagnostic);
				}
			}
		}

		private Task<Document> ReplaceIdentifier(Document document, SyntaxNode root, ArgumentSyntax nodeToReplace,
			string identifierName, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			ExpressionSyntax newExpression;
			if (identifierName == "this")
				newExpression = SyntaxFactory.ThisExpression();
			else
				newExpression = SyntaxFactory.IdentifierName(identifierName);

			var newNode = nodeToReplace.WithExpression(newExpression);
			var newRoot = root.ReplaceNode(nodeToReplace, newNode);

			return Task.FromResult(document.WithSyntaxRoot(newRoot));
		}
	}
}
