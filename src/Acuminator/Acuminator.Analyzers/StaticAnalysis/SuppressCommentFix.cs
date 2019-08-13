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
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class SuppressCommentFix : CodeFixProvider
	{
		private const string _comment = @"// Acuminator disable once {0} {1} [Justification]";
		private const string _diagnosticName = @"Suppress diagnostic {0}";

		public static readonly ImmutableDictionary<string, string> _fixableDiagnosticIds;

		static SuppressCommentFix()
		{
			Type diagnosticsType = typeof(Descriptors);
			var propertiesInfo = diagnosticsType.GetRuntimeProperties();

			Dictionary<string,string> idsDiagnosticDescriptors = new Dictionary<string, string>();

			idsDiagnosticDescriptors = propertiesInfo.Where(x => x.PropertyType == typeof(DiagnosticDescriptor))
													.Select(x => new {
														Key = ((DiagnosticDescriptor)x.GetValue(x, null)).Id,
														Name = ((DiagnosticDescriptor)x.GetValue(x, null)).CustomTags.FirstOrDefault()
													}).Distinct()
													.ToDictionary(x => x.Key, x => x.Name);
			
			_fixableDiagnosticIds = idsDiagnosticDescriptors.ToImmutableDictionary();
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			_fixableDiagnosticIds.Keys.ToImmutableArray();

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

			var diagnosticName = _fixableDiagnosticIds.GetValueOrDefault(diagnostic.Id);

			if (!string.IsNullOrWhiteSpace(diagnosticName))
			{
				SyntaxTriviaList commentNode = SyntaxFactory.TriviaList(
					SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia,
						string.Format(_comment, diagnostic.Id, diagnosticName)),
					SyntaxFactory.ElasticEndOfLine(""));

				while (!diagnosticNode.HasLeadingTrivia)
				{
					diagnosticNode = diagnosticNode.Parent;
				}

				SyntaxTriviaList leadingTrivia = diagnosticNode.GetLeadingTrivia();
				var modifiedRoot = root.InsertTriviaAfter(leadingTrivia.Last(), commentNode);
				return document.WithSyntaxRoot(modifiedRoot);

			}

			return document;
		}
	}
}