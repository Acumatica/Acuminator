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
		private const int ParallelDiagnosticsRegisterThreshold = 16;
		private const string _comment = @"// Acuminator disable once {0} {1} [Justification]";

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
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Diagnostics.Length > ParallelDiagnosticsRegisterThreshold)
			{
				RegisterCodeActionsInParallel(context);
			}
			else
			{
				context.Diagnostics.ForEach(diagnostic => RegisterCodeActionForDiagnostic(diagnostic, context));
			}
	
			return Task.CompletedTask;
		}

		private void RegisterCodeActionsInParallel(CodeFixContext context)
		{
			ParallelOptions parallelOptions = new ParallelOptions
			{
				CancellationToken = context.CancellationToken
			};

			try
			{
				Parallel.ForEach(context.Diagnostics, parallelOptions, diagnostic => RegisterCodeActionForDiagnostic(diagnostic, context));
			}
			catch (AggregateException e)
			{
				var operationCanceledException = e.Flatten().InnerExceptions
												  .OfType<OperationCanceledException>()
												  .FirstOrDefault();

				if (operationCanceledException != null)
				{
					throw operationCanceledException;
				}

				throw;
			}
		}

		private void RegisterCodeActionForDiagnostic(Diagnostic diagnostic, CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			string groupCodeActionNameFormat = nameof(Resources.SuppressDiagnosticGroupCodeActionTitle).GetLocalized().ToString();
			string groupCodeActionName = string.Format(groupCodeActionNameFormat, diagnostic.Id);



			CodeAction codeAction = CodeAction.Create(groupCodeActionName,
				cToken => AddSuppressionComment(context, diagnostic, cToken),
				groupCodeActionName);
			context.RegisterCodeFix(codeAction, diagnostic);
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
