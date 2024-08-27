
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Acuminator.Analyzers.StaticAnalysis.PublicClassXmlComment.CodeFix
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class PublicClassXmlCommentFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			new HashSet<string>
			{
				Descriptors.PX1007_PublicClassNoXmlComment.Id,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id
			}
			.ToImmutableArray();

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			if (diagnostic?.Properties == null || 
				!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.ParseResult, out string? value) ||
				!Enum.TryParse(value, out XmlCommentParseResult parseResult) || IsCorrectParseResult(parseResult))
			{
				return Task.CompletedTask;
			}

			bool isProjectionDacProperty = diagnostic.IsFlagSet(DocumentationDiagnosticProperties.IsProjectionDacProperty);

			if (isProjectionDacProperty)
			{
				RegisterCodeFixForProjectionDacProperty(context, diagnostic, parseResult);
			}
			else if (parseResult != XmlCommentParseResult.IncorrectInheritdocTagOnProjectionDacProperty &&
					 parseResult != XmlCommentParseResult.NonInheritdocTagOnProjectionDacProperty)
			{
				var addSummaryCodeAction = new AddSummaryTagCodeAction(context.Document, context.Span, parseResult);
				context.RegisterCodeFix(addSummaryCodeAction, diagnostic);
			}

			var addExcludeTagAction = new AddExcludeTagCodeAction(context.Document, context.Span);
			context.RegisterCodeFix(addExcludeTagAction, diagnostic);

			return Task.CompletedTask;
		}

		private bool IsCorrectParseResult(XmlCommentParseResult parseResult) =>
			parseResult is XmlCommentParseResult.HasExcludeTag or XmlCommentParseResult.HasNonEmptySummaryTag or
						   XmlCommentParseResult.CorrectInheritdocTag or XmlCommentParseResult.HasNonEmptySummaryAndCorrectInheritdocTags;

		private void RegisterCodeFixForProjectionDacProperty(CodeFixContext context, Diagnostic diagnostic, XmlCommentParseResult parseResult)
		{
			if (!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.MappedDacMetadataName, out string? mappedOriginalDacName) ||
				mappedOriginalDacName.IsNullOrWhiteSpace())
			{
				return;
			}

			if (!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.MappedDacPropertyName, out string? mappedPropertyName) ||
				mappedPropertyName.IsNullOrWhiteSpace())
			{
				return;
			}

			var addInheritdocAction = new AddInheritdocTagCodeAction(context.Document, context.Span, parseResult, mappedOriginalDacName, mappedPropertyName);
			context.RegisterCodeFix(addInheritdocAction, diagnostic);
		}
	}
}
