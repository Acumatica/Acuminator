using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace PX.Analyzers.FixProviders
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class PXGraphCreateInstanceFix : CodeFixProvider
	{
		private class Rewriter : CSharpSyntaxRewriter
		{
			private readonly PXContext _pxContext;
			private readonly Document _document;
			private readonly SemanticModel _semanticModel;

			public Rewriter(PXContext pxContext, Document document, SemanticModel semanticModel)
			{
				_pxContext = pxContext;
				_document = document;
				_semanticModel = semanticModel;
			}

			public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				var generator = SyntaxGenerator.GetGenerator(_document);
				var typeSymbol = _semanticModel.GetSymbolInfo(node.Type).Symbol as ITypeSymbol;
				if (typeSymbol != null)
				{
					return generator.InvocationExpression(
						generator.MemberAccessExpression(
							generator.TypeExpression(_pxContext.PXGraphType),
							generator.GenericName(nameof(PX.Data.PXGraph.CreateInstance), typeSymbol)));
				}

				return base.VisitObjectCreationExpression(node);
			}
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1001_PXGraphCreateInstance.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			var node = root.FindNode(context.Span);
			string title = nameof(Resources.PX1001Fix).GetLocalized().ToString();

			if (node != null)
			{
				var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
				context.RegisterCodeFix(CodeAction.Create(title, c =>
					{
						var rewriter = new Rewriter(new PXContext(semanticModel.Compilation), context.Document, semanticModel);
						var newNode = rewriter.Visit(node);
						return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, newNode)));
					}, title),
					context.Diagnostics);
			}
		}
	}
}
