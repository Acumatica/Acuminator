#nullable enable

using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class PXGraphCreateInstanceFix : CodeFixProvider
	{
		private class Rewriter : CSharpSyntaxRewriter
		{
			private readonly PXContext _pxContext;
			private readonly Document _document;
			private readonly SemanticModel _semanticModel;
			private readonly CancellationToken _cancellation;

			public Rewriter(PXContext pxContext, Document document, SemanticModel semanticModel, CancellationToken cancellation)
			{
				_pxContext = pxContext;
				_document = document;
				_semanticModel = semanticModel;
				_cancellation = cancellation;
			}

			public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				_cancellation.ThrowIfCancellationRequested();

				var generator = SyntaxGenerator.GetGenerator(_document);
				var typeSymbol = _semanticModel.GetSymbolInfo(node.Type, _cancellation).Symbol as ITypeSymbol;

				if (typeSymbol != null)
				{
					return generator.InvocationExpression(
						generator.MemberAccessExpression(
							generator.TypeExpression(_pxContext.PXGraph.Type),
							generator.GenericName(DelegateNames.CreateInstance, typeSymbol)));
				}

				return base.VisitObjectCreationExpression(node);
			}
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1001_PXGraphCreateInstance.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			if (!context.Diagnostics.Any(diagnostic => FixableDiagnosticIds.Contains(diagnostic.Id)))
				return Task.CompletedTask;

			string title = nameof(Resources.PX1001Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(title, cancellation => RewriteGraphConstructionAsync(context.Document, context.Span, cancellation),
											   equivalenceKey: title);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
			return Task.CompletedTask;
		}

		private static async Task<Document> RewriteGraphConstructionAsync(Document document, TextSpan span, CancellationToken cancellation)
		{
			var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var nodeWithDiagnostic = root?.FindNode(span);

			if (nodeWithDiagnostic == null)
				return document;

			var semanticModel = await document.GetSemanticModelAsync(cancellation).ConfigureAwait(false);

			if (semanticModel == null)
				return document;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			var rewriter = new Rewriter(pxContext, document, semanticModel, cancellation);
			var newNode = rewriter.Visit(nodeWithDiagnostic);
			var newRoot = root.ReplaceNode(nodeWithDiagnostic, newNode);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
