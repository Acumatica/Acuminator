#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.CodeGeneration;
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

			var (semanticModel, root) = await context.Document.GetSemanticModelAndRootAsync(context.CancellationToken).ConfigureAwait(false);

			if (semanticModel == null || root?.FindNode(context.Span) is not ClassDeclarationSyntax dacNode)
				return;

			INamedTypeSymbol? dacTypeSymbol = semanticModel.GetDeclaredSymbol(dacNode, context.CancellationToken);

			if (dacTypeSymbol == null || dacTypeSymbol.MemberNames.Contains(TypeNames.ReferentialIntegrity.ForeignKeyClassName))
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

			var foreignKeyContainerNode = CreateForeignKeysContainerClassNode(pxContext, dacTypeSymbol, isInsertedFirst: positionToInsertFK == 0, cancellation);

			var newDacNode = dacNode.WithMembers(
										dacNode.Members.Insert(positionToInsertFK, foreignKeyContainerNode));

			var changedRoot = root.ReplaceNode(dacNode, newDacNode) as CompilationUnitSyntax;

			if (changedRoot == null)
				return Task.FromResult(document);

			changedRoot = changedRoot.AddMissingUsingDirectiveForNamespace(NamespaceNames.ReferentialIntegrityAttributes);
			var modifiedDocument = document.WithSyntaxRoot(changedRoot);
			return Task.FromResult(modifiedDocument);
		}

		private int GetPositionToInsertForeignKeysContainerClass(ClassDeclarationSyntax dacNode, SemanticModel semanticModel, PXContext pxContext,
																 CancellationToken cancellation)
		{
			var primaryKeySymbol = pxContext.ReferentialIntegritySymbols.IPrimaryKey;
			int? lastPrimaryOrUniqueKeyIndex = null;

			for (int i = 0; i < dacNode.Members.Count; i++)
			{
				if (!(dacNode.Members[i] is ClassDeclarationSyntax nestedType))
					continue;

				var nestedTypeSymbol = semanticModel.GetDeclaredSymbol(nestedType, cancellation);

				if (nestedTypeSymbol == null)
					continue;

				bool isUniqueKeysContainer = nestedTypeSymbol.Name == TypeNames.ReferentialIntegrity.UniqueKeyClassName &&
											 nestedTypeSymbol.DeclaredAccessibility == Accessibility.Public && nestedTypeSymbol.IsStatic;  
				
				if (isUniqueKeysContainer || (primaryKeySymbol != null && nestedTypeSymbol.ImplementsInterface(primaryKeySymbol)))
				{
					lastPrimaryOrUniqueKeyIndex = i + 1;
				}
			}

			return lastPrimaryOrUniqueKeyIndex ?? 0;
		}

		private ClassDeclarationSyntax CreateForeignKeysContainerClassNode(PXContext pxContext, INamedTypeSymbol dacTypeSymbol, bool isInsertedFirst,
																		   CancellationToken cancellation)
		{		
			var dacPropertiesWithForeignKeys = GetDacPropertiesWithForeignKeys(pxContext, dacTypeSymbol, cancellation);
			var examplesTrivia = GetForeignKeyExampleTemplates(dacTypeSymbol, dacPropertiesWithForeignKeys);

			ClassDeclarationSyntax fkClassDeclaration =
				ClassDeclaration(TypeNames.ReferentialIntegrity.ForeignKeyClassName)
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
						);

			fkClassDeclaration = isInsertedFirst
				? fkClassDeclaration.WithTrailingTrivia(EndOfLine(Environment.NewLine), EndOfLine(Environment.NewLine))
				: fkClassDeclaration.WithTrailingTrivia(EndOfLine(Environment.NewLine));

			return fkClassDeclaration;
		}

		private List<DacPropertyInfo> GetDacPropertiesWithForeignKeys(PXContext pxContext, INamedTypeSymbol dacTypeSymbol, CancellationToken cancellation)
		{
			var foreignKeyAttributes = GetForeignKeyAttributes(pxContext);

			if (foreignKeyAttributes.Count == 0)
				return new List<DacPropertyInfo>();
			
			var dacSemanticModel = DacSemanticModel.InferModel(pxContext, dacTypeSymbol, cancellation: cancellation);

			if (dacSemanticModel == null || dacSemanticModel.DacType != DacType.Dac)
				return new List<DacPropertyInfo>();

			var selectorAttribute = pxContext.AttributeTypes.PXSelectorAttribute.Type!;
			var dimensionSelectorAttribute = pxContext.AttributeTypes.PXDimensionSelectorAttribute;
			var dacPropertiesWithForeignKeys = 
				from dacProperty in dacSemanticModel.DacFieldProperties
				where !dacProperty.Attributes.IsDefaultOrEmpty && 
					   dacProperty.DeclaredDbBoundness == DbBoundnessType.DbBound &&								//only Bound FKs should work correctly
					   dacProperty.Attributes.Any(attribute => IsForeignKeyAttribute(attribute))
				orderby dacProperty.DeclarationOrder ascending
				select dacProperty;

			return dacPropertiesWithForeignKeys.ToList();

			//----------------------------------------Local Function----------------------------------------------------------------
			bool IsForeignKeyAttribute(DacFieldAttributeInfo attribute)
			{
				const string selectorAttributeProperty = "SelectorAttribute";

				if (attribute.AttributeType == null)
					return false;

				bool isDerivedFromOrAggregateForeignKeyAttribute =
					foreignKeyAttributes.Exists(
						foreignKeyAttribute => attribute.AttributeType.IsDerivedFromOrAggregatesAttribute(foreignKeyAttribute, pxContext));

				if (isDerivedFromOrAggregateForeignKeyAttribute)
					return true;

				var selectorAttributeMembers = attribute.AttributeType.GetBaseTypesAndThis()
																	  .SelectMany(type => type.GetMembers(selectorAttributeProperty));

				var selectorAttributeCandidateMemberTypes = 
					from type in attribute.AttributeType.GetBaseTypesAndThis()
														.TakeWhile(attrType => !pxContext.AttributeTypes.PXEventSubscriberAttribute.Equals(attrType))
					from member in type.GetMembers(selectorAttributeProperty)
					select member switch
					{
						IPropertySymbol property => property.Type,
						IFieldSymbol field => field.Type,
						_ => null
					};

				return selectorAttributeCandidateMemberTypes
						.Any(memberType => memberType != null &&
										   (memberType.IsDerivedFromOrAggregatesAttribute(selectorAttribute, pxContext) ||
											memberType.IsDerivedFromOrAggregatesAttribute(dimensionSelectorAttribute, pxContext))); 
			}
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

			for (int i = 0; i < dacPropertiesWithForeignKeys.Count; i++)
			{
				DacPropertyInfo propertyWithForeignKey = dacPropertiesWithForeignKeys[i];
				string propertyName = propertyWithForeignKey.Symbol.Name;

				if (i < dacPropertiesWithForeignKeys.Count - 1)
					propertyName += ",";

				yield return propertyName.ToSingleLineComment();
				yield return EndOfLine(Environment.NewLine);
			}
		}
	}
}
