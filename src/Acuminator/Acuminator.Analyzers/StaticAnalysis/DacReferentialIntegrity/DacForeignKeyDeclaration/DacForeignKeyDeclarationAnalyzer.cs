using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using static Acuminator.Utilities.Roslyn.Constants.TypeNames;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	public class DacForeignKeyDeclarationAnalyzer : DacKeyDeclarationAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1034_MissingDacForeignKeyDeclaration,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields,
				Descriptors.PX1036_WrongDacForeignKeyDeclaration,
				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration
			);

		protected override bool IsKeySymbolDefined(PXContext context) =>
			context.ReferentialIntegritySymbols.IForeignKey != null ||
			context.ReferentialIntegritySymbols.KeysRelation != null;

		protected override List<INamedTypeSymbol> GetDacKeysDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken) =>
			GetForeignKeyDeclarations(context, dac, cancellationToken).ToList();

		private IEnumerable<INamedTypeSymbol> GetForeignKeyDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken)
		{
			var allNestedTypes = dac.Symbol.GetFlattenedNestedTypes(shouldWalkThroughNestedTypesPredicate: nestedType => !nestedType.IsDacOrExtension(context),
																	cancellationToken);

			if (context.ReferentialIntegritySymbols.IForeignKey != null)
			{
				return allNestedTypes.Where(type => type.ImplementsInterface(context.ReferentialIntegritySymbols.IForeignKey));
			}

			return from nestedType in allNestedTypes
				   where nestedType.InheritsFromOrEqualsGeneric(context.ReferentialIntegritySymbols.KeysRelation) &&
						 nestedType.GetBaseTypesAndThis().Any(IsForeignKey)
				   select nestedType;
		}

		private bool IsForeignKey(ITypeSymbol type)
		{
			ITypeSymbol currentType = type.ContainingType;

			while (currentType != null)
			{
				if (PXReferentialIntegritySymbols.ForeignKeyContainerNames.Contains(currentType.Name))
					return true;

				currentType = currentType.ContainingType;
			}

			return false;
		}

		protected override ITypeSymbol GetParentDacFromKey(PXContext context, INamedTypeSymbol foreignKey)
		{
			ITypeSymbol parentDAC = GetParentDacFromForeighKeyToInterface(context, foreignKey); // effective implementation via interface for Acumatica 2019R2 and later

			if (parentDAC != null)
				return parentDAC.IsDAC() ? parentDAC : null;

			var baseKeyTypes = foreignKey.GetBaseTypes().OfType<INamedTypeSymbol>();

			// We don't support custom foreign key implementations since it will be impossible to deduce target DAC in a general case.
			// Instead we only analyze foreign keys for general three cases:
			// 1. Keys declared via primary key of other DAC. This should handle 90% of FK use cases:
			//    SOOrder.PK.ForeignKeyOf<SOLine>.By<orderType, orderNbr>. 
			//  
			// 2. Keys declared with explicit declaration of relationship between DACs:
			//  a. For simple keys consisting of 1 field:  Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>.AsSimpleKey.WithTablesOf<InventoryItem, SOLine>
			//  b. For complex keys: 
			//			CompositeKey<
			//						 Field<orderType>.IsRelatedTo<SOOrder.orderType>,
			//						 Field<orderNbr>.IsRelatedTo<SOOrder.orderNbr>
			//						>
			//						.WithTablesOf<SOOrder, SOLine>
			foreach (INamedTypeSymbol baseType in baseKeyTypes)
			{
				switch (baseType.Name)
				{
					case ReferentialIntegrity.By_TypeName
					when baseType.ContainingType?.Name == ReferentialIntegrity.ForeignKeyOfName:    //Case of foreign key declared via primary or unique key
						{
							INamedTypeSymbol topMostContainingType = baseType.TopMostContainingType();

							if (topMostContainingType?.Name == ReferentialIntegrity.PrimaryKeyOfName && topMostContainingType.TypeArguments.Length == 1)
							{
								parentDAC = topMostContainingType.TypeArguments[0];
								return parentDAC.IsDAC() ? parentDAC : null;
							}

							return null;
						}

					case ReferentialIntegrity.WithTablesOf_TypeName:								//Case of foreign key with explicit declaration
						{
							var typeArguments = baseType.TypeArguments;

							if (typeArguments.Length != 2)
								return null;

							parentDAC = typeArguments[0];
							return parentDAC.IsDAC() ? parentDAC : null;
						}
				}
			}

			return null;
		}

		private ITypeSymbol GetParentDacFromForeighKeyToInterface(PXContext context, INamedTypeSymbol foreignKey)
		{
			if (context.ReferentialIntegritySymbols.IForeignKeyTo1 == null)
				return null;

			INamedTypeSymbol foreignKeyInterface = foreignKey.AllInterfaces
															 .FirstOrDefault(i => i.TypeArguments.Length == 1 && 
																				  i.Name == ReferentialIntegrity.IForeignKeyToName);
			return foreignKeyInterface.TypeArguments[0];
		}

		/// <summary>
		/// Gets ordered DAC fields used by <paramref name="foreignKey"/>.
		/// </summary>
		/// <param name="dac">The DAC.</param>
		/// <param name="foreignKey">The foreign key.</param>
		/// <returns>
		/// The ordered DAC fields used by <paramref name="foreignKey"/>.
		/// </returns>
		protected override List<ITypeSymbol> GetOrderedDacFieldsUsedByKey(DacSemanticModel dac, INamedTypeSymbol foreignKey)
		{
			var baseKeyTypes = foreignKey.GetBaseTypes().OfType<INamedTypeSymbol>();

			// We don't support custom foreign key implementations since it will be impossible to deduce referenced set of DAC fields in a general case.
			// Instead we only analyze foreign keys for general three cases:
			// 1. Keys declared via primary key of other DAC. This should handle 90% of FK use cases:
			//    SOOrder.PK.ForeignKeyOf<SOLine>.By<orderType, orderNbr>. 
			//  
			// 2. Keys declared with explicit declaration of relationship between DACs:
			//  a. For simple keys consisting of 1 field:  Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>.AsSimpleKey.WithTablesOf<InventoryItem, SOLine>
			//  b. For complex keys: 
			//			CompositeKey<
			//						 Field<orderType>.IsRelatedTo<SOOrder.orderType>,
			//						 Field<orderNbr>.IsRelatedTo<SOOrder.orderNbr>
			//						>
			//						.WithTablesOf<SOOrder, SOLine>
			foreach (INamedTypeSymbol baseType in baseKeyTypes)
			{
				switch (baseType.Name)
				{
					case ReferentialIntegrity.By_TypeName
					when baseType.ContainingType?.Name == ReferentialIntegrity.ForeignKeyOfName:    //Case of foreign key declared via primary or unique key
						{
							var usedFields = baseType.TypeArguments;
							bool areValidFields = !usedFields.IsDefaultOrEmpty && usedFields.All(dacFieldArg => dac.FieldsByNames.ContainsKey(dacFieldArg.Name));

							return areValidFields
								? usedFields.OrderBy(dacField => dacField.MetadataName).ToList(capacity: usedFields.Length)
								: new List<ITypeSymbol>();
						}

					case ReferentialIntegrity.WithTablesOf_TypeName
					when baseType.ContainingType?.Name == ReferentialIntegrity.AsSimpleKeyName:     //Case of simple 1-field foreign key with explicit declaration
						{
							var isRelatedTo_Type = baseType.ContainingType.ContainingType;
							var usedDacField = GetDacFieldFromIsRelatedToType(dac, isRelatedTo_Type);

							return usedDacField != null
								? new List<ITypeSymbol>(capacity: 1) { usedDacField }
								: new List<ITypeSymbol>();
						}

					case ReferentialIntegrity.WithTablesOf_TypeName
					when baseType.ContainingType?.Name == ReferentialIntegrity.CompositeKey:       //Case of complex foreign key with explicit declaration
						{
							var compositeKey = baseType.ContainingType;

							if (compositeKey.TypeArguments.IsDefaultOrEmpty)
								return new List<ITypeSymbol>();

							var usedDacFields = compositeKey.TypeArguments.Select(isRelatedTo_Type => GetDacFieldFromIsRelatedToType(dac, isRelatedTo_Type))
																		  .Where(dacField => dacField != null)
																		  .OrderBy(dacField => dacField.MetadataName)
																		  .ToList(capacity: compositeKey.TypeArguments.Length);

							return usedDacFields.Count == compositeKey.TypeArguments.Length
								? usedDacFields
								: new List<ITypeSymbol>();
						}
				}
			}

			return new List<ITypeSymbol>();
		}	

		private ITypeSymbol GetDacFieldFromIsRelatedToType(DacSemanticModel dac, ITypeSymbol isRelatedTo_Type)
		{
			var field_Type = isRelatedTo_Type?.ContainingType;

			if (isRelatedTo_Type?.Name != ReferentialIntegrity.IsRelatedTo || field_Type?.Name != ReferentialIntegrity.Field ||
				field_Type.TypeArguments.Length != 1)
			{
				return null;
			}

			var usedDacField = field_Type.TypeArguments[0];
			return dac.FieldsByNames.ContainsKey(usedDacField.Name)
				? usedDacField
				: null;
		}

		protected override Location GetUnboundDacFieldLocation(ClassDeclarationSyntax keyNode, ITypeSymbol unboundDacFieldInKey)
		{
			if (keyNode.BaseList.Types.Count == 0)
				return null;

			BaseTypeSyntax baseTypeNode = keyNode.BaseList.Types[0];

			if (!(baseTypeNode.Type is QualifiedNameSyntax fullBaseTypeQualifiedName) || !(fullBaseTypeQualifiedName.Right is GenericNameSyntax byOrWithTablesOfNode))
				return null;

			if (byOrWithTablesOfNode.Identifier.Text == ReferentialIntegrity.By_TypeName)
			{
				var byTypeNode = byOrWithTablesOfNode;
				return GetUnboundDacFieldLocationFromTypeArguments(byTypeNode, unboundDacFieldInKey);
			}
			else if (byOrWithTablesOfNode.Identifier.Text == ReferentialIntegrity.WithTablesOf_TypeName)
			{
				switch (fullBaseTypeQualifiedName.Left)
				{
					//AsSimpleKey case
					case QualifiedNameSyntax partialBaseTypeQualifiedName												  //Case of Field<>.IsRelatedTo<> AsSimpleKey.WithTablesOf<,>
					when partialBaseTypeQualifiedName.Right is IdentifierNameSyntax { Identifier: { Text: ReferentialIntegrity.AsSimpleKeyName } } asSimpleKeyNode:

						return GetUnboundDacFieldLocationFromAsSimpleKeyNode(partialBaseTypeQualifiedName.Left as QualifiedNameSyntax, unboundDacFieldInKey);

					//CompositeKey cases
					case GenericNameSyntax { Identifier: { Text: ReferentialIntegrity.CompositeKey } } compositeKeyNode:   //Case of CompositeKey<,...,>.WithTablesOf<,>
						return GetUnboundDacFieldLocationFromCompositeKeyNode(compositeKeyNode, unboundDacFieldInKey);

					case QualifiedNameSyntax compositeKeyNodeWithNamespace												   //Case of Namespace.CompositeKey<,...,> and Alias::Namespace.CompositeKey<,...,>
					when compositeKeyNodeWithNamespace.Right is GenericNameSyntax { Identifier: { Text: ReferentialIntegrity.CompositeKey } } compositeKeyNode:

						return GetUnboundDacFieldLocationFromCompositeKeyNode(compositeKeyNode, unboundDacFieldInKey);

					case AliasQualifiedNameSyntax compositeKeyNodeWithAlias
					when compositeKeyNodeWithAlias.Name is GenericNameSyntax { Identifier: { Text: ReferentialIntegrity.CompositeKey } } compositeKeyNode:

						return GetUnboundDacFieldLocationFromCompositeKeyNode(compositeKeyNode, unboundDacFieldInKey);
				}
			}

			return null;
		}

		private Location GetUnboundDacFieldLocationFromAsSimpleKeyNode(QualifiedNameSyntax asSimpleKeyLeftPartOfNameNode, ITypeSymbol unboundDacFieldInKey)
		{
			if (asSimpleKeyLeftPartOfNameNode?.Right?.Identifier.Text != ReferentialIntegrity.IsRelatedTo)
				return null;

			GenericNameSyntax fieldNode = GetFieldNodeWithPrefixConsideration(asSimpleKeyLeftPartOfNameNode.Left);
			return fieldNode != null
				? GetUnboundDacFieldLocationFromTypeArguments(fieldNode, unboundDacFieldInKey)
				: null;
		}

		private Location GetUnboundDacFieldLocationFromCompositeKeyNode(GenericNameSyntax compositeKeyNode, ITypeSymbol unboundDacFieldInKey)
		{
			var fieldLocations = 
				compositeKeyNode.TypeArgumentList.Arguments
												 .OfType<QualifiedNameSyntax>()
												 .Select(fieldWithRelatedToNode => 
															GetFieldNodeWithPrefixConsideration(fieldWithRelatedToNode.Left) is GenericNameSyntax fieldNode
																 ? GetUnboundDacFieldLocationFromTypeArguments(fieldNode, unboundDacFieldInKey)
																 : null);

			return fieldLocations.FirstOrDefault(location => location != null);
		}

		private GenericNameSyntax GetFieldNodeWithPrefixConsideration(NameSyntax fieldNodeWithPossibleNamespaceAndAlias) =>
			fieldNodeWithPossibleNamespaceAndAlias switch
			{
				GenericNameSyntax fieldNode when fieldNode.Identifier.Text == ReferentialIntegrity.Field							   => fieldNode,  //case Field<> without prefix
				QualifiedNameSyntax		 { Right: GenericNameSyntax { Identifier: { Text: ReferentialIntegrity.Field } } fieldNode } _ => fieldNode,  //cases Namespace.Field<> and Alias::Namespace.Field<>
				AliasQualifiedNameSyntax { Name: GenericNameSyntax  { Identifier: { Text: ReferentialIntegrity.Field } } fieldNode } _ => fieldNode,  //case Alias::Field<>
				_ => null
			};

		protected override void MakeSpecificDacKeysAnalysis(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
															List<INamedTypeSymbol> dacForeignKeys, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (dacForeignKeys.Count == 0)
			{
				ReportNoForeignKeyDeclarationsInDac(symbolContext, context, dac);
				return;
			}

			INamedTypeSymbol foreignKeysContainer = dac.Symbol.GetTypeMembers(ReferentialIntegrity.ForeignKeyClassName)
															  .FirstOrDefault();

			//We can register code fix only if there is no FK nested type in DAC or there is a public static FK class. Otherwise we will break the code.
			bool registerCodeFix = foreignKeysContainer == null ||
								   (foreignKeysContainer.DeclaredAccessibility == Accessibility.Public && foreignKeysContainer.IsStatic);

			List<INamedTypeSymbol> keysNotInContainer = GetKeysNotInContainer(dacForeignKeys, foreignKeysContainer);

			if (keysNotInContainer.Count == 0)
				return;

			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			Location dacLocation = dac.Node.GetLocation();
			var keysNotInContainerLocations = GetKeysLocations(keysNotInContainer, symbolContext.CancellationToken).ToList(capacity: keysNotInContainer.Count);

			if (dacLocation == null || keysNotInContainerLocations.Count == 0)
				return;

			var dacLocationArray = new[] { dacLocation };
			var diagnosticProperties = new Dictionary<string, string>
			{
				{ nameof(RefIntegrityDacKeyType), RefIntegrityDacKeyType.ForeignKey.ToString() },
				{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
			}
			.ToImmutableDictionary();

			foreach (Location keyLocation in keysNotInContainerLocations)
			{
				var otherKeyLocations = keysNotInContainerLocations.Where(location => location != keyLocation);
				var additionalLocations = dacLocationArray.Concat(otherKeyLocations);

				symbolContext.ReportDiagnosticWithSuppressionCheck(
									Diagnostic.Create(Descriptors.PX1036_WrongDacForeignKeyDeclaration, keyLocation, additionalLocations, diagnosticProperties),
									context.CodeAnalysisSettings);
			}
		}

		private void ReportNoForeignKeyDeclarationsInDac(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			Location location = dac.Node.Identifier.GetLocation() ?? dac.Node.GetLocation();

			if (location != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1034_MissingDacForeignKeyDeclaration, location),
					context.CodeAnalysisSettings);
			} 
		}

		private List<INamedTypeSymbol> GetKeysNotInContainer(List<INamedTypeSymbol> keyDeclarations, INamedTypeSymbol foreignKeysContainer)
		{
			bool containerDeclaredIncorrectly = foreignKeysContainer?.DeclaredAccessibility != Accessibility.Public || !foreignKeysContainer.IsStatic;

			return containerDeclaredIncorrectly
				? keyDeclarations
				: keyDeclarations.Where(key => key.ContainingType != foreignKeysContainer && !key.GetContainingTypes().Contains(foreignKeysContainer))
								 .ToList(capacity: keyDeclarations.Count);
		}
	}
}