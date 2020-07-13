using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
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
	public class DacMissingForeignKeyFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1034_MissingDacForeignKeyDeclaration.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public async override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var rootTask = context.Document.GetSyntaxRootAsync(context.CancellationToken);
			var semanticModelTask = context.Document.GetSemanticModelAsync(context.CancellationToken);

			await Task.WhenAll(rootTask, semanticModelTask).ConfigureAwait(false);

			SyntaxNode root = rootTask.Result;
			SemanticModel semanticModel = semanticModelTask.Result;

			if (!(root?.FindNode(context.Span) is ClassDeclarationSyntax dacNode))
				return;

			INamedTypeSymbol dacTypeSymbol = semanticModel.GetDeclaredSymbol(dacNode, context.CancellationToken);

			if (dacTypeSymbol == null || dacTypeSymbol.MemberNames.Contains(TypeNames.ForeignKeyClassName))
				return;

			var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);

			if (pxContext.ReferentialIntegritySymbols.CompositeKey2 == null)
				return;

			var codeActionTitle = nameof(Resources.PX1034Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(codeActionTitle,
											   cancellation => AddForeignKeyDeclarationTemplateToDacAsync(context.Document, root, semanticModel, pxContext,
																										  dacNode, dacTypeSymbol, cancellation),
											   equivalenceKey: codeActionTitle);

			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private Task<Document> AddForeignKeyDeclarationTemplateToDacAsync(Document document, SyntaxNode root, SemanticModel semanticModel, PXContext pxContext, 
																		  ClassDeclarationSyntax dacNode, INamedTypeSymbol dacTypeSymbol, 
																		  CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();	

			int positionToInsertFK = GetPositionToInsertForeignKeysContainerClass(dacNode, semanticModel, pxContext, cancellation);
			
			var foreignKeyContainerNode = CreateForeignKeysContainerClassNode(pxContext, dacTypeSymbol, cancellation);

			var newDacNode = dacNode.WithMembers(
										dacNode.Members.Insert(positionToInsertFK, foreignKeyContainerNode));

			var changedRoot = root.ReplaceNode(dacNode, newDacNode);
			changedRoot = AddUsingsForReferentialIntegrityNamespace(changedRoot);
			var modifiedDocument = document.WithSyntaxRoot(changedRoot);
			return Task.FromResult(modifiedDocument);
		}

		private int GetPositionToInsertForeignKeysContainerClass(ClassDeclarationSyntax dacNode, SemanticModel semanticModel, PXContext pxContext,
																 CancellationToken cancellation)
		{
			var primaryKeySymbol = pxContext.ReferentialIntegritySymbols.IPrimaryKey;

			if (primaryKeySymbol == null)
				return 0;

			for (int i = 0; i < dacNode.Members.Count; i++)
			{
				if (!(dacNode.Members[i] is ClassDeclarationSyntax nestedType))
					continue;

				var nestedTypeSymbol = semanticModel.GetDeclaredSymbol(nestedType, cancellation);

				if (nestedTypeSymbol != null && nestedTypeSymbol.ImplementsInterface(primaryKeySymbol))
				{
					return i + 1;
				}
			}

			return 0;
		}

		private ClassDeclarationSyntax CreateForeignKeysContainerClassNode(PXContext pxContext, INamedTypeSymbol dacTypeSymbol, CancellationToken cancellation)
		{		
			var dacPropertiesWithForeignKeys = GetDacPropertiesWithForeignKeys(pxContext, dacTypeSymbol, cancellation);
			var examplesTrivia = GetForeignKeyExampleTemplates(dacTypeSymbol, dacPropertiesWithForeignKeys);

			ClassDeclarationSyntax fkClassDeclaration =
				ClassDeclaration(TypeNames.ForeignKeyClassName)
					.WithModifiers(
						TokenList(
							new[]{
								Token(SyntaxKind.PublicKeyword),
								Token(SyntaxKind.StaticKeyword)}))
					.WithCloseBraceToken(
						Token
						(
							TriviaList(examplesTrivia),
							SyntaxKind.CloseBraceToken,
							TriviaList())		
						)
					.WithTrailingTrivia(EndOfLine(Environment.NewLine), EndOfLine(Environment.NewLine));

			return fkClassDeclaration;
		}

		private List<DacPropertyInfo> GetDacPropertiesWithForeignKeys(PXContext pxContext, INamedTypeSymbol dacTypeSymbol, CancellationToken cancellation)
		{
			var foreignKeyAttributes = GetForeignKeyAttributes(pxContext);

			if (foreignKeyAttributes.Count == 0)
				return new List<DacPropertyInfo>();

			var dacSemanticModel = DacSemanticModel.InferModel(pxContext, dacTypeSymbol, cancellation);

			if (dacSemanticModel == null || dacSemanticModel.DacType != DacType.Dac)
				return new List<DacPropertyInfo>();

			AttributeInformation attributeInformation = new AttributeInformation(pxContext);
			var dacPropertiesWithForeignKeys = 
				from dacProperty in dacSemanticModel.DacProperties
				where !dacProperty.Attributes.IsDefaultOrEmpty &&
					   dacProperty.Attributes.Any(attribute => IsForeignKeyAttribute(attribute))
				select dacProperty;

			return dacPropertiesWithForeignKeys.ToList();

			//----------------------------------------Local Function----------------------------------------------------------------
			bool IsForeignKeyAttribute(AttributeInfo attribute) =>
				foreignKeyAttributes.Exists(
					foreignKeyAttribute => attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeType, foreignKeyAttribute));
		}

		private static List<INamedTypeSymbol> GetForeignKeyAttributes(PXContext pxContext)
		{
			var foreignKeyAttributes = new List<INamedTypeSymbol>(capacity: 5);

			var parentAttribute = pxContext.AttributeTypes.PXParentAttribute;

			if (parentAttribute != null)
				foreignKeyAttributes.Add(parentAttribute);

			var dbDefaultAttribute = pxContext.AttributeTypes.PXDBDefaultAttribute;

			if (dbDefaultAttribute != null)
				foreignKeyAttributes.Add(dbDefaultAttribute);

			var foreignReferenceAttribute = pxContext.AttributeTypes.PXForeignReferenceAttribute;

			if (foreignReferenceAttribute != null)
				foreignKeyAttributes.Add(foreignReferenceAttribute);

			var selectorAttribute = pxContext.AttributeTypes.PXSelectorAttribute.Type;

			if (selectorAttribute != null)
				foreignKeyAttributes.Add(selectorAttribute);

			var dimensionSelectorAttribute = pxContext.AttributeTypes.PXDimensionSelectorAttribute;

			if (dimensionSelectorAttribute != null)
				foreignKeyAttributes.Add(dimensionSelectorAttribute);

			return foreignKeyAttributes;
		}

		private IEnumerable<SyntaxTrivia> GetForeignKeyExampleTemplates(INamedTypeSymbol dacTypeSymbol, List<DacPropertyInfo> dacPropertiesWithForeignKeys)
		{
			var emptyLineComment = string.Empty.ToSingleLineComment();

			yield return Resources.PX1034FixTemplateLine1.ToSingleLineComment();
			yield return EndOfLine(Environment.NewLine);

			string fkExampleWithUseOfReferencedDacPrimaryKey = string.Format(Resources.PX1034FixTemplateLine2, dacTypeSymbol.Name);
			yield return fkExampleWithUseOfReferencedDacPrimaryKey.ToSingleLineComment();
			yield return EndOfLine(Environment.NewLine);

			yield return emptyLineComment;
			yield return EndOfLine(Environment.NewLine);

			yield return Resources.PX1034FixTemplateLine3.ToSingleLineComment();
			yield return EndOfLine(Environment.NewLine);

			string fkExampleWithoutPK_WithSimplePrimaryKey = string.Format(Resources.PX1034FixTemplateLine4, dacTypeSymbol.Name);
			yield return fkExampleWithoutPK_WithSimplePrimaryKey.ToSingleLineComment();
			yield return EndOfLine(Environment.NewLine);

			yield return emptyLineComment;
			yield return EndOfLine(Environment.NewLine);

			yield return Resources.PX1034FixTemplateLine5.ToSingleLineComment();
			yield return EndOfLine(Environment.NewLine);

			string fkExampleWithoutPK_WithCompositePrimaryKey = string.Format(Resources.PX1034FixTemplateLine6, dacTypeSymbol.Name);
			string fkExampleWithoutPK_WithCompositePrimaryKeyComment = $"/* {fkExampleWithoutPK_WithCompositePrimaryKey} */";
			yield return Comment(fkExampleWithoutPK_WithCompositePrimaryKeyComment);
			yield return EndOfLine(Environment.NewLine);

			if (dacPropertiesWithForeignKeys.Count == 0)
				yield break;

			yield return emptyLineComment;
			yield return EndOfLine(Environment.NewLine);

			yield return Resources.PX1034FixTemplateLine7.ToSingleLineComment();
			yield return EndOfLine(Environment.NewLine);

			foreach (DacPropertyInfo propertyWithForeignKey in dacPropertiesWithForeignKeys)
			{
				yield return propertyWithForeignKey.Symbol.Name.ToSingleLineComment();
				yield return EndOfLine(Environment.NewLine);
			}
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
