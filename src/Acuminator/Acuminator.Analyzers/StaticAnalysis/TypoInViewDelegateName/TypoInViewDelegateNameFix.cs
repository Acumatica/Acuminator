﻿#nullable enable

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TypoInViewDelegateNameFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1005_TypoInViewDelegateName.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var methodNode = root?.FindNode(context.Span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (methodNode == null)
				return;

			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1005_TypoInViewDelegateName.Id);
			if (diagnostic == null)
				return;

			if (diagnostic.Properties == null
			    || !diagnostic.Properties.TryGetValue(TypoInViewDelegateNameAnalyzer.ViewFieldNameProperty, out string fieldName)
				|| fieldName.IsNullOrWhiteSpace() || fieldName.Length <= 1)
			{
				return;
			}

			string title = nameof(Resources.PX1005Fix).GetLocalized().ToString();
			context.RegisterCodeFix(
				CodeAction.Create(title, async cToken =>
				{
					var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
					var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);
					string newName = GenerateViewDelegateName(fieldName);
					return await Renamer.RenameSymbolAsync(context.Document.Project.Solution, methodSymbol, newName, null, cToken);
				}, title),
				context.Diagnostics);
		}

		private static string GenerateViewDelegateName(string viewName)
		{
			var chars = viewName.ToCharArray();
			char firstChar = chars[0];
			if (Char.IsUpper(firstChar))
			{
				firstChar = Char.ToLowerInvariant(firstChar);
			}
			else if (Char.IsLower(firstChar))
			{
				firstChar = Char.ToUpperInvariant(firstChar);
			}

			chars[0] = firstChar;

			return new string(chars);
		}
	}
}
