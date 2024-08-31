using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Acuminator.Analyzers.StaticAnalysis.TypoInViewAndActionHandlerName
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TypoInViewOrActionDelegateNameFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			new HashSet<string>
			{
				Descriptors.PX1005_TypoInViewDelegateName.Id,
				Descriptors.PX1005_TypoInActionDelegateName.Id
			}
			.ToImmutableArray();

		protected override async Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(TypoInViewAndActionHandlerNameAnalyzer.ViewOrActionNameProperty, out string? viewOrActionName) ||
				viewOrActionName.IsNullOrWhiteSpace() || viewOrActionName.Length <= 1)
			{
				return;
			}

			bool isViewDelegate = diagnostic.IsFlagSet(TypoInViewAndActionHandlerNameAnalyzer.IsViewDelegateFlag);

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var viewOrActionDelegateMethodNode = root?.FindNode(context.Span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (viewOrActionDelegateMethodNode == null)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();

			string title = isViewDelegate
				? nameof(Resources.PX1005ViewDelegateFix).GetLocalized().ToString()
				: nameof(Resources.PX1005ActionDelegateFix).GetLocalized().ToString();
			var document = context.Document;
			var codeAction = CodeAction.Create(title, 
											   cToken => FixTypoInViewDelegateName(document, viewOrActionDelegateMethodNode, viewOrActionName, cToken), 
											   equivalenceKey: title);
			context.RegisterCodeFix(codeAction, diagnostic);
		}

		private static async Task<Solution> FixTypoInViewDelegateName(Document document, MethodDeclarationSyntax viewOrActionDelegateMethodNode, 
																	  string fieldName, CancellationToken cToken)
		{
			cToken.ThrowIfCancellationRequested();

			var semanticModel = await document.GetSemanticModelAsync(cToken).ConfigureAwait(false);
			var viewOrActionDelegateMethodSymbol = semanticModel?.GetDeclaredSymbol(viewOrActionDelegateMethodNode, cToken);

			if (viewOrActionDelegateMethodSymbol == null)
				return document.Project.Solution;

			string? newName = GenerateViewOrActionDelegateName(fieldName);

			if (newName == null)
				return document.Project.Solution;

			return await Renamer.RenameSymbolAsync(document.Project.Solution, viewOrActionDelegateMethodSymbol, newName,
													document.Project.Solution.Options, cToken);
		}

		private static string? GenerateViewOrActionDelegateName(string viewOrActionName)
		{
			char firstChar = viewOrActionName[0];

			if (Char.IsUpper(firstChar))
				return viewOrActionName.FirstCharToLower();
			else if (Char.IsLower(firstChar))
				return viewOrActionName.ToPascalCase();
			else
				return null;
		}
	}
}
