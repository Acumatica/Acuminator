using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acuminator.Analyzers.StaticAnalysis.DacUiAttributes
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class DacUiAttributesFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1094_DacShouldHaveUiAttribute.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var addPXHiddenTitle = nameof(Resources.PX1094FixPXHiddenAttribute).GetLocalized().ToString();
			var addPXHiddenAction = CodeAction.Create(
				addPXHiddenTitle,
				cancellation => AddPXHiddenAttribute(context.Document, context.Span, cancellation));

			context.RegisterCodeFix(addPXHiddenAction, context.Diagnostics);

			var addPXCacheNameTitle = nameof(Resources.PX1094FixPXCacheNameAttribute).GetLocalized().ToString();
			var addPXCacheNameAction = CodeAction.Create(
				addPXCacheNameTitle,
				cancellation => AddPXCacheNameAttribute(context.Document, context.Span, cancellation));
		}

		private async Task<Document> AddPXHiddenAttribute(Document document, TextSpan span, CancellationToken cancellation)
		{
			return document;
		}

		private async Task<Document> AddPXCacheNameAttribute(Document document, TextSpan span, CancellationToken cancellation)
		{
			return document;
		}
	}
}
