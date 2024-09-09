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
	public class StaticFieldOrPropertyInGraphFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1062_StaticFieldOrPropertyInGraph.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			string? codeFixFormatArg = GetCodeFixFormatArg(diagnostic);

			if (codeFixFormatArg.IsNullOrWhiteSpace())
				return Task.CompletedTask;

			bool isViewOrAction = diagnostic.IsFlagSet(StaticFieldOrPropertyInGraphDiagnosticProperties.IsViewOrAction);

			if (!isViewOrAction)
			{
				string makeReadOnlyCodeActionEquivalenceKey = nameof(Resources.PX1062FixMakeReadOnlyFormat).GetLocalized().ToString();
				string makeReadOnlyCodeActionName = nameof(Resources.PX1062FixMakeReadOnlyFormat).GetLocalized(codeFixFormatArg).ToString();

				bool isProperty = diagnostic.IsFlagSet(DiagnosticProperty.IsProperty);
				Func<CancellationToken, Task<Document>> createChangedDocumentFunc = 
					isProperty
						? cToken => MakePropertyReadOnlyAsync(context.Document, context.Span, cToken)
						: cToken => ChangeModifiersAsync(context.Document, context.Span, AddReadOnlyToModifiers, cToken);

				CodeAction makeReadOnlyCodeAction = CodeAction.Create(makeReadOnlyCodeActionName,
																	   createChangedDocumentFunc,
																	   makeReadOnlyCodeActionEquivalenceKey);
				
				context.RegisterCodeFix(makeReadOnlyCodeAction, diagnostic);
			}

			string makeNonStaticCodeActionEquivalenceKey = nameof(Resources.PX1062FixMakeNonStaticFormat).GetLocalized().ToString();
			string makeNonStaticCodeActionName = nameof(Resources.PX1062FixMakeNonStaticFormat).GetLocalized(codeFixFormatArg).ToString();
			CodeAction makeNonStaticCodeAction =
					CodeAction.Create(makeNonStaticCodeActionName, 
									  cToken => ChangeModifiersAsync(context.Document, context.Span,
																	 RemoveStaticModifier, cToken),
									  makeNonStaticCodeActionEquivalenceKey);
			context.RegisterCodeFix(makeNonStaticCodeAction, diagnostic);
			return Task.CompletedTask;
		}

		private string? GetCodeFixFormatArg(Diagnostic diagnostic)
		{
			if (diagnostic.Properties?.Count is null or 0)
				return null;

			return diagnostic.Properties.TryGetValue(StaticFieldOrPropertyInGraphDiagnosticProperties.CodeFixFormatArg, out string? value)
				? value
				: null;
		}

		private async Task<Document> MakePropertyReadOnlyAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? diagnosticNode = root?.FindNode(span);
			var propertyDeclaration = diagnosticNode?.ParentOrSelf<PropertyDeclarationSyntax>();

			if (propertyDeclaration?.AccessorList?.Accessors.Count is null or 0)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var setAccessorIndex = propertyDeclaration.AccessorList.Accessors.IndexOf(accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));

			if (setAccessorIndex < 0)
				return document;

			var modifiedAccessors = propertyDeclaration.AccessorList.Accessors.RemoveAt(setAccessorIndex);
			var readonlyPropertyDeclaration = 
				propertyDeclaration.WithAccessorList(
					propertyDeclaration.AccessorList.WithAccessors(
						modifiedAccessors));

			cancellationToken.ThrowIfCancellationRequested();

			var modifiedRoot = root?.ReplaceNode(propertyDeclaration, readonlyPropertyDeclaration);
			return modifiedRoot == null
				? document
				: document.WithSyntaxRoot(modifiedRoot);
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

			var memberDeclarationWithChangedModifiers = ChangeModifiers(memberDeclaration, modifiersChanger);

			if (memberDeclarationWithChangedModifiers == null)
				return document;

			cancellationToken.ThrowIfCancellationRequested();

			var modifiedRoot = root?.ReplaceNode(memberDeclaration, memberDeclarationWithChangedModifiers);
			return modifiedRoot == null
				? document
				: document.WithSyntaxRoot(modifiedRoot);
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