using System;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXCodeFixProvider: CodeFixProvider
	{
		private const string comment = @"// Acuminator disable once {0} {1} [Justification]";

		private static readonly SyntaxTriviaList commentNode =
			SyntaxFactory.TriviaList(
				SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, comment), 
				SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, text:comment));

		public void RegisterSuppressionComment(CodeFixContext context)
		{
			string codeActionName = "Suppress diagnostic";
			CodeAction codeAction = CodeAction.Create(codeActionName,
												cToken => AddSuppressionComment(context.Document, context.Span, cToken),
												codeActionName);
			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> AddSuppressionComment(Document document, TextSpan span,
			CancellationToken cancellationToken)
		{
			SyntaxNode root= await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode diagnosticNode = root?.FindNode(span);

			if (diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;

			if (diagnosticNode.HasLeadingTrivia)
			{
				SyntaxTriviaList leadingTrivia = diagnosticNode.GetLeadingTrivia();
				
				var modifiedRoot = root.InsertTriviaAfter(leadingTrivia.Last(), commentNode);
				return document.WithSyntaxRoot(modifiedRoot);
			}

			return document;

		}
	}
}