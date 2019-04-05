using System;
using System.Collections;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InvalidPXActionSignatureFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
											 .ConfigureAwait(false);

			var method = root?.FindNode(context.Span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (method == null || context.CancellationToken.IsCancellationRequested)
				return;

			string codeActionName = nameof(Resources.PX1000Fix).GetLocalized().ToString();
			context.RegisterCodeFix(
				new ChangeSignatureAction(codeActionName, context.Document, method),
				context.Diagnostics);
		}

		//-------------------------------------Code Action for Fix---------------------------------------------------------------------------
		internal class ChangeSignatureAction : CodeAction
		{
			private const string AdapterParameterName = "adapter";
			private const string AdapterGetMethodName = "Get";

			private readonly string _title;
			private readonly Document _document;
			private readonly MethodDeclarationSyntax _method;

			public override string Title => _title;
			public override string EquivalenceKey => _title;

			public ChangeSignatureAction(string title, Document document, MethodDeclarationSyntax method)
			{
				_title = title;
				_document = document;
				_method = method;
			}

			protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
			{
				var semanticModel = await _document.GetSemanticModelAsync(cancellationToken)
												   .ConfigureAwait(false);

				if (semanticModel == null || cancellationToken.IsCancellationRequested)
					return _document;

				var pxContext = new PXContext(semanticModel.Compilation);
				var oldRoot = await _document.GetSyntaxRootAsync(cancellationToken)
											 .ConfigureAwait(false);

				if (oldRoot == null || cancellationToken.IsCancellationRequested)
					return _document;

				var newRoot = oldRoot;
				var generator = SyntaxGenerator.GetGenerator(_document);
				var newReturnType = GetNewReturnType(generator, semanticModel, cancellationToken);
				var newParametersList = GetNewParametersList(generator, pxContext, cancellationToken);

				if (newReturnType == null || newParametersList == null || cancellationToken.IsCancellationRequested)
					return _document;

				var newMethod = _method.WithReturnType(newReturnType)
									   .WithParameterList(newParametersList);

				ControlFlowAnalysis controlFlow = semanticModel.AnalyzeControlFlow(_method.Body);

				if (controlFlow != null && controlFlow.Succeeded && controlFlow.ReturnStatements.IsEmpty)
				{
					newMethod = AddReturnStatement(newMethod, generator);
				}

				newRoot = newRoot.ReplaceNode(_method, newMethod);
				newRoot = AddCollectionsUsing(newRoot, generator, cancellationToken);

				if (cancellationToken.IsCancellationRequested)
					return _document;

				return _document.WithSyntaxRoot(newRoot);
			}

			private TypeSyntax GetNewReturnType(SyntaxGenerator generator, SemanticModel semanticModel, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
					return null;

				var ienumerableType = semanticModel.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
				var ienumerableTypeNode = generator.TypeExpression(ienumerableType) as TypeSyntax;
				return ienumerableTypeNode;
			}

			private ParameterListSyntax GetNewParametersList(SyntaxGenerator generator, PXContext pxContext, CancellationToken cancellationToken)
			{
				var pxAdapterTypeNode = generator.TypeExpression(pxContext.PXAdapterType);

				if (pxAdapterTypeNode == null || cancellationToken.IsCancellationRequested)
					return null;

				var adapterPar = generator.ParameterDeclaration(AdapterParameterName, pxAdapterTypeNode) as ParameterSyntax;

				if (adapterPar == null || cancellationToken.IsCancellationRequested)
					return null;

				var oldParameters = _method.ParameterList;
				var newParameters = SyntaxFactory.SeparatedList(new[] { adapterPar });
				return oldParameters.WithParameters(newParameters);
			}

			private MethodDeclarationSyntax AddReturnStatement(MethodDeclarationSyntax newMethod, SyntaxGenerator generator)
			{
				var getMethodInvocation =
					generator.InvocationExpression(
						generator.MemberAccessExpression(generator.IdentifierName(AdapterParameterName), 
														 AdapterGetMethodName));

				var returnStatement = generator.ReturnStatement(getMethodInvocation)
											  ?.WithAdditionalAnnotations(Formatter.Annotation) as StatementSyntax;

				return returnStatement != null 
					? newMethod.AddBodyStatements(returnStatement)
					: newMethod;
			}

			private SyntaxNode AddCollectionsUsing(SyntaxNode root, SyntaxGenerator generator, CancellationToken cancellationToken)
			{
				if (!(root is CompilationUnitSyntax compilationUnit))
					return root;

				var oldUsings = compilationUnit.Usings;
				var usingCollectionsNamespace = generator.NamespaceImportDeclaration(typeof(IEnumerable).Namespace) as UsingDirectiveSyntax;

				if (usingCollectionsNamespace == null || cancellationToken.IsCancellationRequested)
					return root;

				bool usingExists = oldUsings.Any(usingDir => SyntaxFactory.AreEquivalent(usingDir, usingCollectionsNamespace));

				if (usingExists || cancellationToken.IsCancellationRequested)
					return root;

				string usingCollectionsNsName = usingCollectionsNamespace.Name.ToString();
				int indexToInsert = oldUsings.IndexOf(usingDirective =>
														String.CompareOrdinal(usingDirective.Name.ToString(), usingCollectionsNsName) > 0);
				var newUsings = indexToInsert >= 0 
					? oldUsings.Insert(indexToInsert, usingCollectionsNamespace)
					: oldUsings.Add(usingCollectionsNamespace);
		
				return compilationUnit.WithUsings(newUsings);
			}
		}
	}
}