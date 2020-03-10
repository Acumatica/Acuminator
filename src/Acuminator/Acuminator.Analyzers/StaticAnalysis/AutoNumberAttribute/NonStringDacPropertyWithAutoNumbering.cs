using System;
using System.Linq;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

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
	public class NonStringDacPropertyWithAutoNumbering : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
			ImmutableArray.Create(Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1019_AutoNumberOnDacPropertyWithNonStringType.Id);

			if (diagnostic == null || context.CancellationToken.IsCancellationRequested)
				return;

			string format = nameof(Resources.PX1019Fix).GetLocalized().ToString();
			string codeActionName = string.Format(format, mainDacName);
			CodeAction codeAction =
				CodeAction.Create(codeActionName,
								  cToken => ChangePXActionDeclarationAsync(context.Document, context.Span, cToken, mainDacType),
								  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> ChangePXActionDeclarationAsync(Document document, TextSpan span, CancellationToken cancellationToken,
																	INamedTypeSymbol mainDacType)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			GenericNameSyntax pxActionTypeDeclaration = root?.FindNode(span) as GenericNameSyntax;

			if (pxActionTypeDeclaration == null || cancellationToken.IsCancellationRequested)
				return document;

			SyntaxGenerator generator = SyntaxGenerator.GetGenerator(document);
			TypeSyntax mainDacTypeNode = generator.TypeExpression(mainDacType) as TypeSyntax;

			if (mainDacType == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedTypeArgsSyntax = pxActionTypeDeclaration.TypeArgumentList
																.WithArguments(SyntaxFactory.SingletonSeparatedList(mainDacTypeNode));
			GenericNameSyntax modifiedDeclaration = pxActionTypeDeclaration.WithTypeArgumentList(modifiedTypeArgsSyntax);
			SyntaxNode originalNode = null, modifiedNode = null;

			switch (pxActionTypeDeclaration.Parent)
			{
				case VariableDeclarationSyntax variableDeclaration:
					originalNode = variableDeclaration;
					modifiedNode = variableDeclaration.WithType(modifiedDeclaration);
					break;
				case PropertyDeclarationSyntax propertyDeclaration:
					originalNode = propertyDeclaration;
					modifiedNode = propertyDeclaration.WithType(modifiedDeclaration);
					break;
			}

			if (originalNode == null || modifiedNode == null || cancellationToken.IsCancellationRequested)
				return document;

			var modifiedRoot = root.ReplaceNode(originalNode, modifiedNode);
			return document.WithSyntaxRoot(modifiedRoot);
		}
	}
}
