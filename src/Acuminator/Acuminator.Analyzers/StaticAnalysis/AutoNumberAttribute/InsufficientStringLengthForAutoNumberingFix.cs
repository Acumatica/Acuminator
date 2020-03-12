using System;
using System.Linq;
using System.Collections.Generic;
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
	public class InsufficientStringLengthForAutoNumberingFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
			ImmutableArray.Create(Descriptors.PX1020_InsufficientStringLengthForDacPropertyWithAutoNumbering.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			SemanticModel semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
																.ConfigureAwait(false);
			if (semanticModel == null)
				return;

			PXContext pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			int minLengthForAutoNumbering = pxContext.AttributeTypes.AutoNumberAttribute.MinAutoNumberLength;

			string codeActionName = nameof(Resources.PX1020Fix).GetLocalized().ToString();
			CodeAction codeAction = CodeAction.Create(codeActionName,
													  cToken => MakeStringLengthSufficientForAutoNumberingAsync(context.Document, context.Span, minLengthForAutoNumbering, cToken),
													  equivalenceKey: codeActionName);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private async Task<Document> MakeStringLengthSufficientForAutoNumberingAsync(Document document, TextSpan diagnosticSpan, 
																					 int minLengthForAutoNumbering, CancellationToken cancellationToken)
		{
			SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			AttributeArgumentSyntax attributeArgument = GetAttributeArgumentNodeToBeReplaced(root, diagnosticSpan);

			if (attributeArgument == null || cancellationToken.IsCancellationRequested)
				return document;
		
			AttributeArgumentSyntax modifiedArgument =
				attributeArgument.WithExpression(
					SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression,
													SyntaxFactory.Literal(minLengthForAutoNumbering)));
			if (modifiedArgument == null)
				return document;

			var modifiedRoot = root.ReplaceNode(attributeArgument, modifiedArgument);
			return document.WithSyntaxRoot(modifiedRoot);
		}		

		private AttributeArgumentSyntax GetAttributeArgumentNodeToBeReplaced(SyntaxNode root, TextSpan diagnosticSpan)
		{
			SyntaxNode node = root?.FindNode(diagnosticSpan);

			switch (node)
			{
				case IdentifierNameSyntax namedConstant:
					return namedConstant.Parent<AttributeArgumentSyntax>();

				case LiteralExpressionSyntax literal 
				when literal.Kind() == SyntaxKind.NumericLiteralExpression && literal.Token.Value is int:
					return literal.Parent<AttributeArgumentSyntax>();

				case AttributeArgumentSyntax attributeArgument:
					return attributeArgument;

				case AttributeSyntax attribute:
					return SearchForAttributeArgumentToBeReplaced(attribute);

				default:
					return null;
			}
		}

		private AttributeArgumentSyntax SearchForAttributeArgumentToBeReplaced(AttributeSyntax attribute)
		{
			var arguments = attribute.ArgumentList.Arguments;
			var candidateAttributes = new List<AttributeArgumentSyntax>(capacity: 1);

			for (int i = 0; i < arguments.Count; i++)
			{
				AttributeArgumentSyntax attributeArgument = arguments[i];

				if (attributeArgument.NameEquals != null)
					continue;

				switch (attributeArgument.Expression)
				{
					case IdentifierNameSyntax _:
					case LiteralExpressionSyntax literal 
					when literal.Kind() == SyntaxKind.NumericLiteralExpression && literal.Token.Value is int:
						candidateAttributes.Add(attributeArgument);
						continue;
				}
			}

			return candidateAttributes.Count == 1
				? candidateAttributes[0]
				: null;
		}
	}
}
