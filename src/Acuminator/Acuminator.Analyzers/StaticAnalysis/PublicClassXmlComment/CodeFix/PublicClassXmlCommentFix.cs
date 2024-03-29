﻿#nullable enable

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
	public class PublicClassXmlCommentFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			new HashSet<string>
			{
				Descriptors.PX1007_PublicClassNoXmlComment.Id,
				Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id
			}
			.ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var diagnostics = context.Diagnostics;

			if (diagnostics.IsDefaultOrEmpty)
				return Task.CompletedTask;
			else if (diagnostics.Length == 1)
			{
				var diagnostic = diagnostics[0];
				
				if (diagnostic.Id != Descriptors.PX1007_PublicClassNoXmlComment.Id && diagnostic.Id != Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id)
					return Task.CompletedTask;

				return RegisterCodeFixesForDiagnosticAsync(context, diagnostic);
			}

			List<Task> allTasks = new(capacity: diagnostics.Length);

			foreach (Diagnostic diagnostic in context.Diagnostics)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (diagnostic.Id != Descriptors.PX1007_PublicClassNoXmlComment.Id &&
					diagnostic.Id != Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id)
					continue;

				var task = RegisterCodeFixesForDiagnosticAsync(context, diagnostic);
				allTasks.Add(task);
			}

			return Task.WhenAll(allTasks);
		}

		private Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			if (diagnostic?.Properties == null || 
				!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.ParseResult, out string value) ||
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
			if (!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.MappedDacMetadataName, out string mappedOriginalDacName) ||
				mappedOriginalDacName.IsNullOrWhiteSpace())
			{
				return;
			}

			if (!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.MappedDacPropertyName, out string mappedPropertyName) ||
				mappedOriginalDacName.IsNullOrWhiteSpace())
			{
				return;
			}

			var addInheritdocAction = new AddInheritdocTagCodeAction(context.Document, context.Span, parseResult, mappedOriginalDacName, mappedPropertyName);
			context.RegisterCodeFix(addInheritdocAction, diagnostic);
		}
	}
}
