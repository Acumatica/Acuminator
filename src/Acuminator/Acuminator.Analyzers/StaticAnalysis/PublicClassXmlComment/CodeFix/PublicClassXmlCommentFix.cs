#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			var diagnostics = context.Diagnostics;

			if (diagnostics.IsDefaultOrEmpty)
				return;

			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
																.ConfigureAwait(false);
			if (semanticModel == null)
				return;

			List<Task> allTasks = new(capacity: diagnostics.Length);

			foreach (Diagnostic diagnostic in context.Diagnostics)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (diagnostic.Id != Descriptors.PX1007_PublicClassNoXmlComment.Id &&
					diagnostic.Id != Descriptors.PX1007_InvalidProjectionDacFieldDescription.Id)
					continue;

				var task = RegisterCodeFixesForDiagnosticAsync(context, diagnostic, semanticModel);
				allTasks.Add(task);
			}

			await Task.WhenAll(allTasks).ConfigureAwait(false);
		}

		private Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic, SemanticModel semanticModel)
		{
			if (diagnostic?.Properties == null || !diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.ParseResult, out string value) ||
				!Enum.TryParse(value, out XmlCommentParseResult parseResult) || IsCorrectParseResult(parseResult))
			{
				return Task.CompletedTask;
			}

			bool isProjectionDacProperty = diagnostic.IsFlagSet(DocumentationDiagnosticProperties.IsProjectionDacProperty);

			if (isProjectionDacProperty)
				RegisterCodeFixForProjectionDacProperty(context, diagnostic, semanticModel, parseResult);
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

		private void RegisterCodeFixForProjectionDacProperty(CodeFixContext context, Diagnostic diagnostic, SemanticModel semanticModel,
															 XmlCommentParseResult parseResult)
		{
			if (GetProjectionDacOriginalBqlField(diagnostic, semanticModel) is not INamedTypeSymbol projectionDacOriginalBqlField)
				return;

			var addInheritdocTitle = nameof(Resources.PX1007FixAddInheritdocTag).GetLocalized().ToString();
			var addInheritdocAction = CodeAction.Create(addInheritdocTitle,
														cancellation => AddInheritdocTagAsync(context.Document, context.Span, parseResult,
																							  projectionDacOriginalBqlField, cancellation),
														equivalenceKey: addInheritdocTitle);

			context.RegisterCodeFix(addInheritdocAction, context.Diagnostics);
		}

		private INamedTypeSymbol? GetProjectionDacOriginalBqlField(Diagnostic diagnostic, SemanticModel semanticModel)
		{
			if (!diagnostic.Properties.TryGetValue(DocumentationDiagnosticProperties.MappedDacBqlFieldMetadataName, out string projectionDacOriginalBqlFieldName) ||
				projectionDacOriginalBqlFieldName.IsNullOrWhiteSpace())
				return null;

			return semanticModel.Compilation.GetTypeByMetadataName(projectionDacOriginalBqlFieldName);
		}

		

		
	}
}
