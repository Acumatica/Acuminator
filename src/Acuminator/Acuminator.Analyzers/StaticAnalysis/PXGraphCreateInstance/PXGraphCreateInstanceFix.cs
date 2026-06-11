using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
	public class PXGraphCreateInstanceFix : PXCodeFixProvider
	{
		private static readonly DiagnosticDescriptor _cachedPX1001Descriptor = Descriptors.PX1001_PXGraphCreateInstance;

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			[
				_cachedPX1001Descriptor.Id,
				Descriptors.PX1003_BasePXGraphCreateInstance.Id
			];

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			string title = diagnostic.Id == _cachedPX1001Descriptor.Id
				? nameof(Resources.PX1001Fix).GetLocalized().ToString()
				: nameof(Resources.PX1003Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(title, cancellation => RewriteGraphConstructionAsync(context.Document, context.Span, cancellation),
											   equivalenceKey: title);

			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private static async Task<Document> RewriteGraphConstructionAsync(Document document, TextSpan span, CancellationToken cancellation)
		{
			var (semanticModel, root) = await document.GetSemanticModelAndRootAsync(cancellation).ConfigureAwait(false);
			var nodeWithDiagnostic = root?.FindNode(span);

			if (semanticModel == null || nodeWithDiagnostic == null)
				return document;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
			var rewriter = new Rewriter(pxContext, document, semanticModel, cancellation);
			var newNode = rewriter.Visit(nodeWithDiagnostic);

			if (newNode == null)
				return document;

			var newRoot = root!.ReplaceNode(nodeWithDiagnostic, newNode);

			return document.WithSyntaxRoot(newRoot);
		}


		private class Rewriter : CSharpSyntaxRewriter
		{
			private readonly PXContext _pxContext;
			private readonly Document _document;
			private readonly SyntaxGenerator? _generator;
			private readonly SemanticModel _semanticModel;
			private readonly CancellationToken _cancellation;

			public Rewriter(PXContext pxContext, Document document, SemanticModel semanticModel, CancellationToken cancellation)
			{
				_pxContext 	   = pxContext;
				_document 	   = document;
				_semanticModel = semanticModel;
				_cancellation  = cancellation;
				_generator 	   = SyntaxGenerator.GetGenerator(_document);
			}

			public override SyntaxNode? VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_generator == null) 
					return base.VisitObjectCreationExpression(node);

				var graphTypeSymbol		   = _semanticModel.GetSymbolOrBestCandidate(node.Type, _cancellation) as ITypeSymbol;
				var createInstanceCallNode = GeneratePXGraphCreateInstanceCall(graphTypeSymbol);

				return createInstanceCallNode ?? base.VisitObjectCreationExpression(node);
			}

			public override SyntaxNode? VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (_generator == null)
					return base.VisitImplicitObjectCreationExpression(node);

				var constructor = _semanticModel.GetSymbolOrBestCandidate(node, _cancellation) as IMethodSymbol;

				if (constructor?.ContainingType == null || constructor.MethodKind != MethodKind.Constructor)
					return base.VisitImplicitObjectCreationExpression(node);

				var createInstanceCallNode = GeneratePXGraphCreateInstanceCall(constructor.ContainingType);
				return createInstanceCallNode ?? base.VisitImplicitObjectCreationExpression(node);
			}

			private SyntaxNode? GeneratePXGraphCreateInstanceCall(ITypeSymbol? graphTypeSymbol)
			{
				if (graphTypeSymbol == null)
					return null;

				return _generator!.InvocationExpression(
						_generator.MemberAccessExpression(
							_generator.TypeExpression(_pxContext.PXGraph.Type),
							_generator.GenericName(DelegateNames.CreateInstance, graphTypeSymbol)));
			}

			public override SyntaxNode? DefaultVisit(SyntaxNode node)
			{
				_cancellation.ThrowIfCancellationRequested();
				return base.DefaultVisit(node);
			}
		}
	}
}
