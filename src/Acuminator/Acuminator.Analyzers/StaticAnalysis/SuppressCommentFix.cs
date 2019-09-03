using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class SuppressCommentFix : CodeFixProvider
	{
		private const string _comment = @"// Acuminator disable once {0} {1} [Justification]";
		private const string _diagnosticName = @"Suppress diagnostic {0}";

		private static readonly ImmutableArray<string> _fixableDiagnosticIds;

		static SuppressCommentFix()
		{
			Type diagnosticsType = typeof(Descriptors);
			var propertiesInfo = diagnosticsType.GetRuntimeProperties();

			_fixableDiagnosticIds = propertiesInfo
				.Where(x => x.PropertyType == typeof(DiagnosticDescriptor))
				.Select(x =>
				{
					var descriptor = (DiagnosticDescriptor)x.GetValue(x);
					return descriptor.Id;
				})
				.ToImmutableArray();
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			_fixableDiagnosticIds;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return Task.Run(() =>
			{
				ParallelOptions parallelOptions = new ParallelOptions
				{
					CancellationToken = context.CancellationToken
				};

				Parallel.ForEach(context.Diagnostics, parallelOptions, (diagnostic) => {
					parallelOptions.CancellationToken.ThrowIfCancellationRequested();

					string codeActionName = string.Format(_diagnosticName, diagnostic.Id);
					CodeAction codeAction = CodeAction.Create(codeActionName,
						cToken => AddSuppressionComment(context, diagnostic, cToken),
						codeActionName);
					context.RegisterCodeFix(codeAction, diagnostic);
				});
			}, context.CancellationToken);
		}

		private async Task<Document> AddSuppressionComment(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root?.FindNode(context.Span);

			if (diagnostic == null || node == null || cancellationToken.IsCancellationRequested)
				return document;

			var diagnosticShortName = diagnostic.Descriptor.CustomTags.FirstOrDefault();

			if (!string.IsNullOrWhiteSpace(diagnosticShortName))
			{
				SyntaxTriviaList commentNode = SyntaxFactory.TriviaList(
					SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia,
						string.Format(_comment, diagnostic.Id, diagnosticShortName)),
					SyntaxFactory.ElasticEndOfLine(""));

				while (!(node == null || node is StatementSyntax || node is MemberDeclarationSyntax))
				{
					node = node.Parent;
				}

				SyntaxTriviaList leadingTrivia = node.GetLeadingTrivia();
				var modifiedRoot = root.InsertTriviaAfter(leadingTrivia.Last(), commentNode);

				return document.WithSyntaxRoot(modifiedRoot);
			}

			return document;
		}
	}
}
