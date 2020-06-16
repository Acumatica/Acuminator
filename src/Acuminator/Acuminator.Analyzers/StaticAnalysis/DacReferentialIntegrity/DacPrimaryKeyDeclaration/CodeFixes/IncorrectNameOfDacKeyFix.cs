﻿using System.Collections.Generic;
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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

using PXReferentialIntegritySymbols = Acuminator.Utilities.Roslyn.Semantic.Symbols.PXReferentialIntegritySymbols;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class IncorrectNameOfDacKeyFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableHashSet.Create
            (      
                Descriptors.PX1036_WrongDacPrimaryKeyName.Id,
                Descriptors.PX1036_WrongDacForeignKeyName.Id
            )
            .ToImmutableArray();

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var codeAtionTitle = nameof(Resources.PX1033Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(codeAtionTitle,
											   cancellation => AddPrimaryKeyDeclarationToDac(context.Document, context.Span, cancellation),
											   equivalenceKey: codeAtionTitle);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
			return Task.CompletedTask;
		}

		private async Task<Document> AddPrimaryKeyDeclarationToDac(Document document, TextSpan span, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var rootTask = document.GetSyntaxRootAsync(cancellation);
			var semanticModelTask = document.GetSemanticModelAsync(cancellation);

			await Task.WhenAll(rootTask, semanticModelTask).ConfigureAwait(false);

			var root = rootTask.Result;
			SemanticModel semanticModel = semanticModelTask.Result;

			if (!(root?.FindNode(span) is ClassDeclarationSyntax dacNode))
				return document;

			var dacTypeSymbol = semanticModel?.GetDeclaredSymbol(dacNode, cancellation);

			if (dacTypeSymbol == null)
				return document;
		
			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);

            if (pxContext.ReferentialIntegritySymbols.PrimaryKeyOf == null)
                return document;

			var dacSemanticModel = DacSemanticModel.InferModel(pxContext, dacTypeSymbol, cancellation);
			List<DacPropertyInfo> dacKeys = dacSemanticModel?.DacProperties.Where(property => property.IsKey).ToList(capacity: 4);

			if (dacKeys.IsNullOrEmpty() || dacKeys.Count > PXReferentialIntegritySymbols.MaxPrimaryKeySize)
				return document;

			var primaryKeyNode = CreatePrimaryKeyNode(document, pxContext, dacSemanticModel, dacKeys);
			var newDacNode = dacNode.WithMembers(
										dacNode.Members.Insert(0, primaryKeyNode));
			var newRoot = root.ReplaceNode(dacNode, newDacNode);
            newRoot = AddUsingsForReferentialIntegrityNamespace(newRoot);

			var newDocument = document.WithSyntaxRoot(newRoot);
            var formattedDocument = await Formatter.FormatAsync(newDocument, cancellationToken: cancellation);
			return formattedDocument;			
		}

        private ClassDeclarationSyntax CreatePrimaryKeyNode(Document document, PXContext pxContext, DacSemanticModel dacSemanticModel,
                                                            List<DacPropertyInfo> dacKeys)
        {
            var generator = SyntaxGenerator.GetGenerator(document);
            SyntaxNode baseClassNode = MakeBaseClassNode(generator, pxContext, dacSemanticModel, dacKeys);
            var findMethod = MakeFindMethodNode(generator, pxContext, dacSemanticModel, dacKeys);
            var keyDeclaration = generator.ClassDeclaration(TypeNames.PrimaryKeyClassName, accessibility: Accessibility.Public,
                                                            baseType: baseClassNode, members: findMethod.ToEnumerable());
            return keyDeclaration as ClassDeclarationSyntax;
        }

        private SyntaxNode MakeBaseClassNode(SyntaxGenerator generator, PXContext pxContext, DacSemanticModel dacSemanticModel, 
                                             List<DacPropertyInfo> dacKeys)
        {
            const string ByTypeName = "By";
            var primaryKeyOfTypeNode = generator.GenericName(pxContext.ReferentialIntegritySymbols.PrimaryKeyOf.Name,
                                                             generator.TypeExpression(dacSemanticModel.Symbol));
            var dacFieldTypeArgNodes = dacKeys.Select(keyProperty => dacSemanticModel.FieldsByNames[keyProperty.Name])
                                              .Select(keyField => generator.TypeExpression(keyField.Symbol));

            return generator.QualifiedName(primaryKeyOfTypeNode, generator.GenericName(ByTypeName, dacFieldTypeArgNodes));
        }

        private SyntaxNode MakeFindMethodNode(SyntaxGenerator generator, PXContext pxContext, DacSemanticModel dacSemanticModel,
                                              List<DacPropertyInfo> dacKeys)
        {
            var returnType = generator.TypeExpression(dacSemanticModel.Symbol);
            var parameters = MakeParameterNodesForFindMethod(generator, pxContext, dacSemanticModel, dacKeys);
            var findMethodNode = generator.MethodDeclaration(DelegateNames.PrimaryKeyFindMethod, parameters, 
                                                             typeParameters: null, returnType,
                                                             Accessibility.Public, DeclarationModifiers.Static) as MethodDeclarationSyntax;

            var findByCallArguments = parameters.OfType<ParameterSyntax>()
                                                .Select(parameter => Argument(
                                                                            IdentifierName(parameter.Identifier)));
            var findByInvocation = 
                generator.InvocationExpression(
                            IdentifierName(DelegateNames.PrimaryKeyFindByMethod), findByCallArguments) as InvocationExpressionSyntax;

            if (findMethodNode.Body != null)
            {
                findMethodNode = findMethodNode.RemoveNode(findMethodNode.Body, SyntaxRemoveOptions.KeepNoTrivia);
            }

            return findMethodNode.WithExpressionBody(
                                        ArrowExpressionClause(findByInvocation))
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