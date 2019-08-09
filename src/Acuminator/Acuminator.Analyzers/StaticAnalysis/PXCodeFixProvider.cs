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
using System.Collections.Generic;

namespace Acuminator.Analyzers.StaticAnalysis
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class PXCodeFixProvider: CodeFixProvider
	{
		private const string _comment = @"// Acuminator disable once {0} {1} [Justification]";
		private const string _diagnosticName = @"Suppress diagnostic {0}";

		private static ImmutableArray<string> _FixableDiagnosticIds;

		static PXCodeFixProvider()
		{
			Type diagnosticsType = typeof(Descriptors);
			var fieldInfo = diagnosticsType.GetRuntimeProperties();

			List<string> idsDiagnosticDescriptors = new List<string>();

			foreach (var field in fieldInfo
									.Where(x => x.PropertyType == typeof(DiagnosticDescriptor))
									.Select(x => x))
			{
				DiagnosticDescriptor descriptor = field.GetValue(field, null) as  DiagnosticDescriptor;
				idsDiagnosticDescriptors.Add(descriptor.Id);
			}
			
			_FixableDiagnosticIds = idsDiagnosticDescriptors.ToImmutableArray();
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			_FixableDiagnosticIds;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			return Task.Run(() =>
			{
				foreach (var diagnostic in context.Diagnostics)
				{
					string codeActionName = string.Format(_diagnosticName, diagnostic.Id);
					CodeAction codeAction = CodeAction.Create(codeActionName,
						cToken => AddSuppressionComment(context, diagnostic, cToken),
						codeActionName);
					context.RegisterCodeFix(codeAction, diagnostic);
				}
			}, context.CancellationToken);
		}

		private async Task<Document> AddSuppressionComment(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var diagnosticNode = root?.FindNode(context.Span);

			if (diagnostic == null || diagnosticNode == null || cancellationToken.IsCancellationRequested)
				return document;


			SyntaxTriviaList commentNode = SyntaxFactory.TriviaList(
				SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, string.Format(_comment, diagnostic.Id, "Description")),
				SyntaxFactory.ElasticEndOfLine(""));

			while (!diagnosticNode.HasLeadingTrivia)
			{
				diagnosticNode = diagnosticNode.Parent;
			}

			SyntaxTriviaList leadingTrivia = diagnosticNode.GetLeadingTrivia();
			var modifiedRoot = root.InsertTriviaAfter(leadingTrivia.Last(), commentNode);
			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}