using System;
using System.Linq;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;

namespace Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class NonStringDacPropertyWithAutoNumberingFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
			ImmutableArray.Create(Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			string codeActionName = nameof(Resources.PX1019Fix).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName,
													  cToken => ChangePropertyTypeToStringAsync(context.Document, context.Span, cToken),
													  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
			return Task.CompletedTask;
		}

		private async Task<Document> ChangePropertyTypeToStringAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var propertyNode = GetPropertyNodeWithDiagnostic(root, diagnosticSpan);

			if (propertyNode == null)
				return document;

			PredefinedTypeSyntax stringType = SyntaxFactory.PredefinedType(
														SyntaxFactory.Token(SyntaxKind.StringKeyword));
			if (stringType == null)
				return document;

			var modifiedPropertyNode = propertyNode.WithType(stringType);

			if (modifiedPropertyNode == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedRoot = root.ReplaceNode(propertyNode, modifiedPropertyNode);
			return document.WithSyntaxRoot(modifiedRoot);
		}

		private PropertyDeclarationSyntax GetPropertyNodeWithDiagnostic(SyntaxNode root, TextSpan diagnosticSpan)
		{
			SyntaxNode node = root?.FindNode(diagnosticSpan);

			switch (node)
			{
				case PropertyDeclarationSyntax propertyDeclaration:
					return propertyDeclaration;

				case IdentifierNameSyntax propertyTypeDeclaration
				when propertyTypeDeclaration.Parent is PropertyDeclarationSyntax propertyDeclaration:
					return propertyDeclaration;

				default:
					return null;
			}
		}
	}
}
