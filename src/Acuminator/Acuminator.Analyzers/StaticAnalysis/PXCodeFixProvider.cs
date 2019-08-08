using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using PX.Common;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXCodeFixProvider: CodeFixProvider
	{
		private const string _comment = @"// Acuminator disable once {0} {1} [Justification]";

		public void RegisterSuppressionComment(CodeFixContext context)
		{
			string codeActionName = "Suppress diagnostic";
			CodeAction codeAction = CodeAction.Create(codeActionName,
												cToken => AddSuppressionComment(context, cToken),
												codeActionName);
			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> AddSuppressionComment(CodeFixContext context, CancellationToken cancellationToken)
		{
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticNode = root?.FindNode(context.Span);

			var diagnostic = context.Diagnostics.FirstOrDefault();
			SyntaxTriviaList commentNode;

			if (diagnostic == null || diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;

			if (context.Diagnostics.Length > 1)
			{
				commentNode = SyntaxFactory.TriviaList(
					SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, string.Format(_comment, "all", "diagnostics")),
					SyntaxFactory.ElasticEndOfLine(""));
			}
			else
			{
				commentNode =
					SyntaxFactory.TriviaList(
						SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, string.Format(_comment, diagnostic.Id, "Description")),
						SyntaxFactory.ElasticEndOfLine(""));
			}
			
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