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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.DiagnosticSuppression.CodeActions;
using Acuminator.Utilities.Roslyn.CodeActions;

namespace Acuminator.Analyzers.StaticAnalysis
{
	[Shared]
	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class SuppressDiagnosticFix : CodeFixProvider
	{
		private const string _comment = @"// Acuminator disable once {0} {1} {2}";

		private static readonly ImmutableArray<string> _fixableDiagnosticIds;

		static SuppressDiagnosticFix()
		{
			Type diagnosticsType = typeof(Descriptors);
			var propertiesInfo = diagnosticsType.GetRuntimeProperties();

			_fixableDiagnosticIds = propertiesInfo
				.Where(property => property.PropertyType == typeof(DiagnosticDescriptor))
				.Select(property =>
				{
					var descriptor = property.GetValue(null) as DiagnosticDescriptor;
					return descriptor?.Id;
				})
				.Where(id => id != null)
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
									 equivalenceKey: commentCodeActionName + diagnostic.Id);
		}

		protected virtual CodeAction GetSuppressWithSuppressionFileCodeAction(Diagnostic diagnostic, CodeFixContext context, bool isNested)
		{
			string suppressionFileCodeActionName = nameof(Resources.SuppressDiagnosticInSuppressionFileCodeActionTitle).GetLocalized().ToString();
			return new SuppressWithSuppressionFileCodeAction(context, diagnostic, suppressionFileCodeActionName,
															 equivalenceKey: suppressionFileCodeActionName + diagnostic.Id); 														 
		}

		private async Task<Document> AddSuppressionCommentAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
		{
			var document = context.Document;
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var node = root?.FindNode(context.Span);

			if (diagnostic == null || node == null || cancellationToken.IsCancellationRequested)
				return document;

			var (diagnosticShortName, diagnosticJustification) = GetDiagnosticShortNameAndJustification(diagnostic);

			if (!string.IsNullOrWhiteSpace(diagnosticShortName))
			{
				SyntaxTriviaList commentNode = SyntaxFactory.TriviaList(
					SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia,
											   string.Format(_comment, diagnostic.Id, diagnosticShortName, diagnosticJustification)),
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

		private (string DiagnosticShortName, string DiagnosticJustification) GetDiagnosticShortNameAndJustification(Diagnostic diagnostic)
		{
			string[] customTags = diagnostic.Descriptor.CustomTags?.ToArray();

			if (customTags.IsNullOrEmpty())
				return default;

			string diagnosticShortName = customTags[0];
			string diagnosticJustification = customTags.Length > 1
				? customTags[1]
				: DiagnosticsDefaultJustification.Default;

			return (diagnosticShortName, diagnosticJustification);
		}
	}
}
