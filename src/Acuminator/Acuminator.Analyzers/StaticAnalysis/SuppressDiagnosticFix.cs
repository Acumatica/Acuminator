using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Analyzers.CodeActions;
using Acuminator.Utilities.DiagnosticSuppression;

using UtilityResources = Acuminator.Utilities.Resources;

namespace Acuminator.Analyzers.StaticAnalysis
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class SuppressDiagnosticFix : CodeFixProvider
	{
		private const string _comment = @"// Acuminator disable once {0} {1} [Justification]";

		private static readonly ImmutableArray<string> _fixableDiagnosticIds;

		static SuppressDiagnosticFix()
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

		public override FixAllProvider GetFixAllProvider() => null;		//explicitly disable fix all support

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			foreach (var diagnostic in context.Diagnostics)
			{
				RegisterCodeActionForDiagnostic(diagnostic, context);
			}
				
			return Task.CompletedTask;
		}

		private void RegisterCodeActionForDiagnostic(Diagnostic diagnostic, CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			CodeAction groupCodeAction = GetCodeActionToRegister(diagnostic, context);

			if (groupCodeAction != null)
			{
				context.RegisterCodeFix(groupCodeAction, diagnostic);
			}	
		}

		protected virtual CodeAction GetCodeActionToRegister(Diagnostic diagnostic, CodeFixContext context)
		{
			if (!SuppressionManager.CheckIfInstanceIsInitialized(throwOnNotInitialized: false))
			{
				return GetSuppressWithCommentCodeAction(diagnostic, context, isNested: false);
			}

			string groupCodeActionNameFormat = nameof(Resources.SuppressDiagnosticGroupCodeActionTitle).GetLocalized().ToString();
			string groupCodeActionName = string.Format(groupCodeActionNameFormat, diagnostic.Id);

			CodeAction suppressWithCommentCodeAction = GetSuppressWithCommentCodeAction(diagnostic, context, isNested: true);
			CodeAction suppressWithSuppressionFileCodeAction = GetSuppressWithSuppressionFileCodeAction(diagnostic, context, isNested: true);
			var suppressionCodeActions = ImmutableArray.CreateBuilder<CodeAction>(initialCapacity: 2);

			if (suppressWithCommentCodeAction != null)
			{
				suppressionCodeActions.Add(suppressWithCommentCodeAction);
			}

			if (suppressWithSuppressionFileCodeAction != null)
			{
				suppressionCodeActions.Add(suppressWithSuppressionFileCodeAction);
			}

			return CodeActionWithNestedActionsFabric.CreateCodeActionWithNestedActions(groupCodeActionName, suppressionCodeActions.ToImmutable());
		}

		protected virtual CodeAction GetSuppressWithCommentCodeAction(Diagnostic diagnostic, CodeFixContext context, bool isNested)
		{
			string commentCodeActionName;

			if (isNested)
			{
				commentCodeActionName = nameof(Resources.SuppressDiagnosticWithCommentNestedCodeActionTitle).GetLocalized().ToString();
			}
			else
			{
				string commentCodeActionFormat = nameof(Resources.SuppressDiagnosticWithCommentNonNestedCodeActionTitle).GetLocalized().ToString();
				commentCodeActionName = string.Format(commentCodeActionFormat, diagnostic.Id);
			}

			return CodeAction.Create(commentCodeActionName,
									 cToken => AddSuppressionCommentAsync(context, diagnostic, cToken),
									 equivalenceKey: commentCodeActionName);
		}

		protected virtual CodeAction GetSuppressWithSuppressionFileCodeAction(Diagnostic diagnostic, CodeFixContext context, bool isNested)
		{
			string suppressionFileCodeActionName = nameof(Resources.SuppressDiagnosticInSuppressionFileCodeActionTitle).GetLocalized().ToString();
			return new SolutionChangeActionWithOptionalPreview(suppressionFileCodeActionName,
															   cToken => SuppressInSuppressionFileAsync(context, diagnostic, cToken),
															   displayPreview: false,
															   equivalenceKey: suppressionFileCodeActionName);
		}

		private async Task<Document> AddSuppressionCommentAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
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

		private async Task<Solution> SuppressInSuppressionFileAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Project project = context.Document.Project;
			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(cancellationToken);

			if (project == null || semanticModel == null)
			{
				return null;
			}

			string suppressionFileName = project.Name + SuppressionFile.SuppressionFileExtension;
			TextDocument projectSuppressionFile = project.AdditionalDocuments.FirstOrDefault(d => string.Equals(suppressionFileName, d.Name,
																								  StringComparison.OrdinalIgnoreCase));
			bool suppressionFileExists = projectSuppressionFile != null;

			if (!suppressionFileExists)
			{
				SuppressionFile suppressionFile = SuppressionManager.CreateSuppressionFileForProject(project);
				suppressionFileExists = suppressionFile != null;
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (!suppressionFileExists || 
				!SuppressionManager.SuppressDiagnostic(semanticModel, diagnostic.Id, diagnostic.Location.SourceSpan, 
													   diagnostic.DefaultSeverity, cancellationToken))
			{
				ShowErrorMessage(projectSuppressionFile, project);
				return null;
			}

			return projectSuppressionFile.Project.Solution;
		}

		private void ShowErrorMessage(TextDocument suppressionFile, Project project)
		{
			LocalizableResourceString errorMessage;

			if (suppressionFile?.FilePath != null)
			{
				errorMessage = new LocalizableResourceString(nameof(UtilityResources.DiagnosticSuppression_FailedToAddToSuppressionFile),
															 UtilityResources.ResourceManager, typeof(UtilityResources), suppressionFile.FilePath);
			}
			else
			{
				errorMessage = new LocalizableResourceString(nameof(UtilityResources.DiagnosticSuppression_FailedToFindSuppressionFile),
															 UtilityResources.ResourceManager, typeof(UtilityResources), project.Name);
			}

			Debug.WriteLine($"{SharedConstants.PackageName.ToUpperInvariant()}: {errorMessage.ToString()}");
		}
	}
}
