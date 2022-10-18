#nullable enable

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.StaticFieldOrPropertyInGraph
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class StaticFieldOrPropertyInGraphFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1062_StaticFieldOrPropertyInGraph.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));

			if (diagnostic == null)
				return Task.CompletedTask;
			
			string? codeFixFormatArg = GetCodeFixFormatArg(diagnostic);

			if (codeFixFormatArg.IsNullOrWhiteSpace())
				return Task.CompletedTask;

			bool isViewOrAction = IsViewOrAction(diagnostic);

			if (!isViewOrAction)
			{
				string makeReadOnlyCodeActionFormat = nameof(Resources.PX1062FixMakeReadOnlyFormat).GetLocalized().ToString();
				string makeReadOnlyCodeActionName = string.Format(makeReadOnlyCodeActionFormat, codeFixFormatArg);

				CodeAction makeReadOnlyCodeAction =
					CodeAction.Create(makeReadOnlyCodeActionName,
									  cToken => ChangeModifiersAsync(context.Document, context.Span, 
																	 AddReadOnlyToModifiers, cToken),
									  equivalenceKey: makeReadOnlyCodeActionName);
				context.RegisterCodeFix(makeReadOnlyCodeAction, diagnostic);
			}

			string makeNonStaticCodeActionFormat = nameof(Resources.PX1062FixMakeNonStaticFormat).GetLocalized().ToString();
			string makeNonStaticCodeActionName = string.Format(makeNonStaticCodeActionFormat, codeFixFormatArg);

			CodeAction makeNonStaticCodeAction =
					CodeAction.Create(makeNonStaticCodeActionName, 
									  cToken => ChangeModifiersAsync(context.Document, context.Span,
																	 RemoveStaticModifier, cToken),
									  equivalenceKey: makeNonStaticCodeActionName);
			context.RegisterCodeFix(makeNonStaticCodeAction, diagnostic);
			return Task.CompletedTask;
		}

		private bool IsViewOrAction(Diagnostic diagnostic) =>
			diagnostic.Properties?.Count > 0 &&
			diagnostic.Properties.TryGetValue(StaticFieldOrPropertyInGraphDiagnosticProperties.IsViewOrAction, out string value) &&
			bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase);

		private string? GetCodeFixFormatArg(Diagnostic diagnostic)
		{
			if (diagnostic.Properties?.Count is null or 0)
				return null;

			return diagnostic.Properties.TryGetValue(StaticFieldOrPropertyInGraphDiagnosticProperties.CodeFixFormatArg, out string value)
				? value
				: null;
		}

		private async Task<Document> ChangeModifiersAsync(Document document, TextSpan span, Func<SyntaxTokenList, SyntaxTokenList?> modifiersChanger,
														  CancellationToken cancellationToken)
		{
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? diagnosticNode = root?.FindNode(span);
			var memberDeclaration = diagnosticNode?.ParentOrSelf<MemberDeclarationSyntax>();

			if (memberDeclaration == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var memberDeclarationWithReadOnly = ChangeModifiers(memberDeclaration, modifiersChanger);

			if (memberDeclarationWithReadOnly == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var modifiedRoot = root.ReplaceNode(memberDeclaration, memberDeclarationWithReadOnly);
			return document.WithSyntaxRoot(modifiedRoot);
		}

		private static MemberDeclarationSyntax? ChangeModifiers(MemberDeclarationSyntax memberDeclaration,
																Func<SyntaxTokenList, SyntaxTokenList?> modifiersChanger)
		{
			switch (memberDeclaration)
			{
				case FieldDeclarationSyntax fieldDeclaration:
				{ 
					var changedModifiers = modifiersChanger(fieldDeclaration.Modifiers);
					return changedModifiers.HasValue
						? fieldDeclaration.WithModifiers(changedModifiers.Value)
						: null;
				}
				case PropertyDeclarationSyntax propertyDeclaration:
				{
					var changedModifiers = modifiersChanger(propertyDeclaration.Modifiers);
					return changedModifiers.HasValue
						? propertyDeclaration.WithModifiers(changedModifiers.Value)
						: null;
				}
				case IndexerDeclarationSyntax indexerDeclaration:
					{
						var changedModifiers = modifiersChanger(indexerDeclaration.Modifiers);
						return changedModifiers.HasValue
							? indexerDeclaration.WithModifiers(changedModifiers.Value)
							: null;
					}
				default:
					return null;
			}
		}

		private static SyntaxTokenList? AddReadOnlyToModifiers(SyntaxTokenList modifiers)
		{
			var readOnlyToken = SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword);

			// place readonly right after static if it is present
			int staticIndex = modifiers.IndexOf(SyntaxKind.StaticKeyword);
			var modifiersWithReadOnly = staticIndex >= 0 && staticIndex < (modifiers.Count - 1)
				? modifiers.Insert(staticIndex, readOnlyToken)
				: modifiers.Add(readOnlyToken);

			return modifiersWithReadOnly;
		}

		private static SyntaxTokenList? RemoveStaticModifier(SyntaxTokenList modifiers)
		{
			int staticIndex = modifiers.IndexOf(SyntaxKind.StaticKeyword);

			if (staticIndex < 0)
				return null;

			var modifiersWithoutStatic = modifiers.RemoveAt(staticIndex);
			return modifiersWithoutStatic;
		}
	}
}