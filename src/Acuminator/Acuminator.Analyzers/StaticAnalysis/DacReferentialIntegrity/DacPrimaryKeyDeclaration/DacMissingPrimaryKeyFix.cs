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
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class DacMissingPrimaryKeyFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1033_MissingDacPrimaryKeyDeclaration.Id);

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
			var dacSemanticModel = DacSemanticModel.InferModel(pxContext, dacTypeSymbol, cancellation);
			List<DacPropertyInfo> dacKeys = dacSemanticModel?.DacProperties.Where(property => property.IsKey).ToList(capacity: 4);

			if (dacKeys.IsNullOrEmpty())
				return document;

			var primaryKeyNode = CreatePrimaryKeyNode(dacKeys);
			var newDacNode = dacNode.WithMembers(
										dacNode.Members.Insert(0, primaryKeyNode));
			var newRoot = root.ReplaceNode(dacNode, newDacNode);
			var newDocument = document.WithSyntaxRoot(newRoot);

			return newDocument;			
		}

        private ClassDeclarationSyntax CreatePrimaryKeyNode(List<DacPropertyInfo> dacKeys)
        {
            ClassDeclaration(TypeNames.PrimaryKeyClassName)
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(
                    )
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        MethodDeclaration(
                            IdentifierName("SOOrder"),
                            Identifier("Find"))
                        .WithModifiers(
                            TokenList(
                                new[]{
                                    Token(SyntaxKind.PublicKeyword),
                                    Token(SyntaxKind.StaticKeyword)}))
                        .WithParameterList(
                            ParameterList(
                                SeparatedList<ParameterSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        Parameter(
                                            Identifier("graph"))
                                        .WithType(
                                            IdentifierName("PXGraph")),
                                        Token(SyntaxKind.CommaToken),
                                        Parameter(
                                            Identifier("orderType"))
                                        .WithType(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword))),
                                        Token(SyntaxKind.CommaToken),
                                        Parameter(
                                            Identifier("orderNbr"))
                                        .WithType(
                                            PredefinedType(
                                                Token(SyntaxKind.StringKeyword)))})))
                        .WithExpressionBody(
                            ArrowExpressionClause(
                                InvocationExpression(
                                    IdentifierName("FindBy"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            new SyntaxNodeOrToken[]{
                                                Argument(
                                                    IdentifierName("graph")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("orderType")),
                                                Token(SyntaxKind.CommaToken),
                                                Argument(
                                                    IdentifierName("orderNbr"))})))))
                        .WithSemicolonToken(
                            Token(SyntaxKind.SemicolonToken))))
                .NormalizeWhitespace();

        }

		private BaseListSyntax MakeBaseListNode()
		{
            BaseList(
                        SingletonSeparatedList<BaseTypeSyntax>(
                            SimpleBaseType(
                                QualifiedName(
                                    GenericName(
                                        Identifier("PrimaryKeyOf"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SingletonSeparatedList<TypeSyntax>(
                                                IdentifierName("SOOrder")))),
                                    GenericName(
                                        Identifier("By"))
                                    .WithTypeArgumentList(
                                        TypeArgumentList(
                                            SeparatedList<TypeSyntax>(
                                                new SyntaxNodeOrToken[]{
                                                    IdentifierName("orderType"),
                                                    Token(SyntaxKind.CommaToken),
                                                    IdentifierName("orderNbr")})))))))

        }
	}
}
