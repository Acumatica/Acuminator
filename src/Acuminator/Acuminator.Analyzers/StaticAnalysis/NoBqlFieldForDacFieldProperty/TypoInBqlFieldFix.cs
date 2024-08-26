using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TypoInBqlFieldFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1066_TypoInBqlFieldName.Id);

#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).

		public override FixAllProvider? GetFixAllProvider() => null!;    // no batch fixer for this code fix

#pragma warning restore CS8764

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.DacFieldName, out string? propertyName) ||
				propertyName.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			string? bqlFieldName = GenerateBqlFieldName(propertyName);

			if (bqlFieldName.IsNullOrWhiteSpace() || propertyName == bqlFieldName)
				return Task.CompletedTask;

			// equivalence key should not contain format arguments to allow mass code fixes,
			// but it should be different for different diagnostics on the same node
			int diagnosticIndex = context.Diagnostics.IndexOf(diagnostic);
			string equivalenceKey = nameof(Resources.PX1066FixFormat).GetLocalized().ToString() + diagnosticIndex.ToString();
			string codeActionName = nameof(Resources.PX1066FixFormat).GetLocalized(bqlFieldName).ToString();
			var codeAction = CodeAction.Create(codeActionName,
											   cToken => FixTypoInBqlFieldName(context.Document, context.Span, bqlFieldName, cToken),
											   equivalenceKey);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private static string? GenerateBqlFieldName(string propertyName)
		{
			char firstChar = propertyName[0];

			if (Char.IsUpper(firstChar))
				return propertyName.FirstCharToLower();
			else if (Char.IsLower(firstChar))
				return propertyName.ToPascalCase();
			else
				return null;
		}

		private static async Task<Solution> FixTypoInBqlFieldName(Document document, TextSpan span, string bqlFieldName, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var (semanticModel, root) = await document.GetSemanticModelAndRootAsync(cancellation).ConfigureAwait(false);

			if (semanticModel == null || root == null)
				return document.Project.Solution;

			var diagnosticNode = root.FindNode(span);
			var bqlFieldNode = diagnosticNode?.FirstAncestorOrSelf<ClassDeclarationSyntax>();

			if (bqlFieldNode == null)
				return document.Project.Solution;

			var methodSymbol = semanticModel.GetDeclaredSymbol(bqlFieldNode, cancellation);

			if (methodSymbol == null)
				return document.Project.Solution;

			return await Renamer.RenameSymbolAsync(document.Project.Solution, methodSymbol, bqlFieldName, document.Project.Solution.Options, cancellation);
		}
	}
}