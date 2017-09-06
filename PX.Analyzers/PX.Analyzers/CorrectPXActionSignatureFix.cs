using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Rename;

namespace PX.Analyzers
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class CorrectPXActionSignatureFix : CodeFixProvider
	{
		private class ChangeSignatureAction : CodeAction
		{
			private readonly string _title;
			private readonly Document _document;
			private readonly MethodDeclarationSyntax _method;

			public override string Title => _title;
			public override string EquivalenceKey => _title;

			protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
			{
				var semanticModel = await _document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
				var pxContext = new PXContext(semanticModel.Compilation);

				var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken);
				var generator = SyntaxGenerator.GetGenerator(_document);
				var IEnumerableType = (TypeSyntax) generator.TypeExpression(semanticModel.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable));
				var PXAdapterType = generator.TypeExpression(pxContext.PXAdapterType);

				var adapterPar = (ParameterSyntax) generator.ParameterDeclaration("adapter", PXAdapterType);
				var oldParameters = _method.ParameterList;
				var newParameters = SyntaxFactory.SeparatedList<ParameterSyntax>(new[] { adapterPar });
				var newMethod = _method.WithReturnType(IEnumerableType);
				newMethod = newMethod.WithParameterList(oldParameters.WithParameters(newParameters));

				var newRoot = oldRoot.ReplaceNode(_method, newMethod);

				return _document.WithSyntaxRoot(newRoot);
			}

			public ChangeSignatureAction(string title, Document document, MethodDeclarationSyntax method)
			{
				_title = title;
				_document = document;
				_method = method;
			}
		}

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync().ConfigureAwait(false);
			var method = root.FindNode(context.Span).FirstAncestorOrSelf<MethodDeclarationSyntax>();
			
			context.RegisterCodeFix(new ChangeSignatureAction(nameof(Resources.PX1000Fix).GetLocalized().ToString(), context.Document, method), context.Diagnostics);
		}
	}
}