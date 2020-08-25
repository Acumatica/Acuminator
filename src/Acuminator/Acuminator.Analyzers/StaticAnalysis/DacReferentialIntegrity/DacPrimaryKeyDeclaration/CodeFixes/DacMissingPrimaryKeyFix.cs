using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;

using PXReferentialIntegritySymbols = Acuminator.Utilities.Roslyn.Semantic.Symbols.PXReferentialIntegritySymbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class DacMissingPrimaryKeyFix : CodeFixProvider
	{
		private const int MaxNumberOfKeysForOneLineStatement = 3;

		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public async override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var rootTask = context.Document.GetSyntaxRootAsync(context.CancellationToken);
			var semanticModelTask = context.Document.GetSemanticModelAsync(context.CancellationToken);

			await Task.WhenAll(rootTask, semanticModelTask).ConfigureAwait(false);

			var root = rootTask.Result;
			SemanticModel semanticModel = semanticModelTask.Result;

			if (!(root?.FindNode(context.Span) is ClassDeclarationSyntax dacNode))
				return;

			INamedTypeSymbol dacTypeSymbol = semanticModel?.GetDeclaredSymbol(dacNode, context.CancellationToken);

			if (dacTypeSymbol == null || dacTypeSymbol.MemberNames.Contains(TypeNames.PrimaryKeyClassName))
				return;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);

			if (pxContext.ReferentialIntegritySymbols.PrimaryKeyOf == null)
				return;

			var codeActionTitle = nameof(Resources.PX1033Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(codeActionTitle,
											   cancellation => AddPrimaryKeyDeclarationToDacAsync(context.Document, root, dacNode, pxContext, 
																								  dacTypeSymbol, cancellation),
											   equivalenceKey: codeActionTitle);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private Task<Document> AddPrimaryKeyDeclarationToDacAsync(Document document, SyntaxNode root, ClassDeclarationSyntax dacNode, PXContext pxContext,
																  INamedTypeSymbol dacTypeSymbol, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var dacSemanticModel = DacSemanticModel.InferModel(pxContext, dacTypeSymbol, cancellation);
			List<DacPropertyInfo> dacKeys = dacSemanticModel?.DacProperties
															 .Where(property => property.IsKey)
															 .OrderBy(property => property.DeclarationOrder)
															 .ToList(capacity: 4);

			if (dacKeys.IsNullOrEmpty() || dacKeys.Count > PXReferentialIntegritySymbols.MaxPrimaryKeySize)
				return Task.FromResult(document);

			var primaryKeyNode = CreatePrimaryKeyNode(document, pxContext, dacSemanticModel, dacKeys);
			var newDacNode = dacNode.WithMembers(
										dacNode.Members.Insert(0, primaryKeyNode));

			var changedRoot = root.ReplaceNode(dacNode, newDacNode);
			changedRoot = AddUsingsForReferentialIntegrityNamespace(changedRoot);
			var modifiedDocument = document.WithSyntaxRoot(changedRoot);

			return Task.FromResult(modifiedDocument);
		}

		private ClassDeclarationSyntax CreatePrimaryKeyNode(Document document, PXContext pxContext, DacSemanticModel dacSemanticModel,
															List<DacPropertyInfo> dacKeys)
		{
			var generator = SyntaxGenerator.GetGenerator(document);
			var baseClassNode = MakeBaseClassNode(generator, pxContext, dacSemanticModel, dacKeys);
			var findMethod = MakeFindMethodNode(generator, pxContext, dacSemanticModel, dacKeys);
			var keyDeclaration = generator.ClassDeclaration(TypeNames.PrimaryKeyClassName, accessibility: Accessibility.Public,
															baseType: baseClassNode, members: findMethod.ToEnumerable())
										  .WithTrailingTrivia(EndOfLine(Environment.NewLine), EndOfLine(Environment.NewLine));

			return keyDeclaration as ClassDeclarationSyntax;
		}

		private SyntaxNode MakeBaseClassNode(SyntaxGenerator generator, PXContext pxContext, DacSemanticModel dacSemanticModel,
											 List<DacPropertyInfo> dacKeys)
		{
			var primaryKeyOfTypeNode = generator.GenericName(pxContext.ReferentialIntegritySymbols.PrimaryKeyOf.Name,
															 generator.TypeExpression(dacSemanticModel.Symbol));
			var dacFieldTypeArgNodes = dacKeys.Select(keyProperty => dacSemanticModel.FieldsByNames[keyProperty.Name])
											  .Select(keyField => generator.TypeExpression(keyField.Symbol));

			return generator.QualifiedName(primaryKeyOfTypeNode, generator.GenericName(TypeNames.By_TypeName, dacFieldTypeArgNodes));
		}

		private MethodDeclarationSyntax MakeFindMethodNode(SyntaxGenerator generator, PXContext pxContext, DacSemanticModel dacSemanticModel,
														   List<DacPropertyInfo> dacKeys)
		{
			var returnType = generator.TypeExpression(dacSemanticModel.Symbol);
			var parameters = MakeParameterNodesForFindMethod(generator, pxContext, dacSemanticModel, dacKeys);
			var findMethodNode = generator.MethodDeclaration(DelegateNames.PrimaryKeyFindMethod, parameters,
															 typeParameters: null, returnType,
															 Accessibility.Public, DeclarationModifiers.Static) as MethodDeclarationSyntax;
			if (findMethodNode.Body != null)
				findMethodNode = findMethodNode.RemoveNode(findMethodNode.Body, SyntaxRemoveOptions.KeepNoTrivia);

			var findByCallArguments = parameters.OfType<ParameterSyntax>()
												.Select(parameter => Argument(
																			IdentifierName(parameter.Identifier)));
			var findByInvocation =
				generator.InvocationExpression(
								IdentifierName(DelegateNames.PrimaryKeyFindByMethod), findByCallArguments) as InvocationExpressionSyntax;

			bool statementTooLongForOneLine = dacKeys.Count > MaxNumberOfKeysForOneLineStatement;

			if (statementTooLongForOneLine)		
			{
				var trivia = dacSemanticModel.Node.GetLeadingTrivia()
												  .Add(Whitespace("\t\t"))
												  .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia));

				findByInvocation = findByInvocation.WithLeadingTrivia(trivia);
			}

			var arrowExpression = ArrowExpressionClause(findByInvocation);

			if (statementTooLongForOneLine)
			{
				arrowExpression = arrowExpression.WithArrowToken(
													Token(SyntaxKind.EqualsGreaterThanToken)
														.WithTrailingTrivia(Whitespace(" "), EndOfLine(Environment.NewLine)));
			}

			return findMethodNode.WithExpressionBody(arrowExpression)
								 .WithSemicolonToken(
										Token(SyntaxKind.SemicolonToken));
		}

		private List<SyntaxNode> MakeParameterNodesForFindMethod(SyntaxGenerator generator, PXContext pxContext,
																 DacSemanticModel dacSemanticModel, List<DacPropertyInfo> dacKeys)
		{
			const string graphParameterName = "graph";
			var graphParameter = generator.ParameterDeclaration(graphParameterName,
																generator.TypeExpression(pxContext.PXGraph.Type));
			var parameters = new List<SyntaxNode>(capacity: dacKeys.Count + 1)
			{
				graphParameter
			};

			foreach (DacPropertyInfo keyProperty in dacKeys)
			{
				DacFieldInfo keyField = dacSemanticModel.FieldsByNames[keyProperty.Name];
				var parameterType = generator.TypeExpression(keyProperty.PropertyType);
				var parameterNode = generator.ParameterDeclaration(keyField.Name, parameterType);

				parameters.Add(parameterNode);
			}

			return parameters;
		}

		private SyntaxNode AddUsingsForReferentialIntegrityNamespace(SyntaxNode root)
		{
			if (!(root is CompilationUnitSyntax compilationUnit))
				return root;

			bool alreadyHasUsing =
				 compilationUnit.Usings
								.Any(usingDirective => NamespaceNames.ReferentialIntegrityAttributes == usingDirective.Name?.ToString());

			if (alreadyHasUsing)
				return root;

			return compilationUnit.AddUsings(
						UsingDirective(
							ParseName(NamespaceNames.ReferentialIntegrityAttributes)));
		}
	}
}
