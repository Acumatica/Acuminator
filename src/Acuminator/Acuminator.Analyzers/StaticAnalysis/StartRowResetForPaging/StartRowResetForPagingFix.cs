using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Data;

namespace Acuminator.Analyzers.StaticAnalysis.StartRowResetForPaging
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class StartRowResetForPagingFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(Descriptors.PX1010_StartRowResetForPaging.Id);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1010_StartRowResetForPaging.Id);

			if (diagnostic?.IsRegisteredForCodeFix() != true || context.CancellationToken.IsCancellationRequested)
				return;

			SyntaxNode root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			var invocation = root?.FindNode(context.Span) as InvocationExpressionSyntax;

			if (invocation == null)
				return;

			string codeActionName = nameof(Resources.PX1010Fix).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName, 
													  cToken => InsertStartRowAssigmentsAsync(context.Document, root, invocation, cToken),
													  equivalenceKey: codeActionName);
			context.RegisterCodeFix(codeAction, context.Diagnostics);			
		}

		private Task<Document> InsertStartRowAssigmentsAsync(Document document, SyntaxNode root, InvocationExpressionSyntax invocation,
															 CancellationToken cToken)
		{
			return Task.Run(() => InsertStartRowAssigments(document, invocation, root, cToken),
							cToken);
		}

		private Document InsertStartRowAssigments(Document document, InvocationExpressionSyntax invocationDeclaration,
												 SyntaxNode root, CancellationToken cancellationToken)
		{
			MethodDeclarationSyntax methodDeclaration = invocationDeclaration.GetDeclaringMethodNode();

			if (methodDeclaration?.Body == null || cancellationToken.IsCancellationRequested)
				return document;

			int invocationSpanEnd = invocationDeclaration.Span.End;
			var returnsAfterInvocation = (from returnNode in methodDeclaration.Body.DescendantNodes()
																			  .OfType<ReturnStatementSyntax>()
										  where returnNode.SpanStart > invocationSpanEnd
										  select returnNode)
										 .ToList();

			if (returnsAfterInvocation.Count == 0 || cancellationToken.IsCancellationRequested)
				return document;

			SyntaxNode trackingRoot = root.TrackNodes(returnsAfterInvocation);

			foreach (ReturnStatementSyntax returnNode in returnsAfterInvocation)
			{
				var returnNodeFromModifiedTree = trackingRoot.GetCurrentNode(returnNode);

				if (returnNodeFromModifiedTree != null)
				{
					trackingRoot = InsertStartRowAssignmentBeforeReturn(trackingRoot, returnNodeFromModifiedTree);
				}
			}

			return document.WithSyntaxRoot(trackingRoot);
		}

		private static SyntaxNode InsertStartRowAssignmentBeforeReturn(SyntaxNode root, ReturnStatementSyntax returnStatement)
		{
			var startRowAssignment = SyntaxFactory.ExpressionStatement(
											SyntaxFactory.ParseExpression($"{nameof(PXView)}.{nameof(PXView.StartRow)} = 0")
														 .WithLeadingTrivia(returnStatement.GetLeadingTrivia()));
			switch (returnStatement.Parent)
			{
				case IfStatementSyntax ifStatement:
					var ifStatementWithBlock = ifStatement.WithStatement(
													SyntaxFactory.Block(startRowAssignment, returnStatement));
					return root.ReplaceNode(ifStatement, ifStatementWithBlock);
				case ElseClauseSyntax elseClause:
					var elseClauseWithBlock = elseClause.WithStatement(
													SyntaxFactory.Block(startRowAssignment, returnStatement));
					return root.ReplaceNode(elseClause, elseClauseWithBlock);
				case WhileStatementSyntax whileStatement:
					var whileStatementWithBlock = whileStatement.WithStatement(
													SyntaxFactory.Block(startRowAssignment, returnStatement));
					return root.ReplaceNode(whileStatement, whileStatementWithBlock);
				case DoStatementSyntax doStatement:
					var doStatementWithBlock = doStatement.WithStatement(
													SyntaxFactory.Block(startRowAssignment, returnStatement));
					return root.ReplaceNode(doStatement, doStatementWithBlock);
				case ForStatementSyntax forStatement:
					var forStatementWithBlock = forStatement.WithStatement(
													SyntaxFactory.Block(startRowAssignment, returnStatement));
					return root.ReplaceNode(forStatement, forStatementWithBlock);
				case ForEachStatementSyntax forEachStatement:
					var forEachStatementWithBlock = forEachStatement.WithStatement(
													SyntaxFactory.Block(startRowAssignment, returnStatement));
					return root.ReplaceNode(forEachStatement, forEachStatementWithBlock);
				case BlockSyntax block:
				case SwitchSectionSyntax switchSection:
					return root.InsertNodesBefore(returnStatement, startRowAssignment.ToEnumerable());
				default:
					return root;
			}
		}
    }
}