using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Symbols;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	public class DacForeignKeyDeclarationAnalyzer : DacKeyDeclarationAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1034_MissingDacForeignKeyDeclaration,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields,
				Descriptors.PX1036_WrongDacForeignKeyName,
				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration
			);

		protected override bool IsKeySymbolDefined(PXContext context) =>
			context.ReferentialIntegritySymbols.IForeignKey != null || 
			context.ReferentialIntegritySymbols.KeysRelation != null;

		protected override RefIntegrityDacKeyType GetRefIntegrityDacKeyType(INamedTypeSymbol key) => RefIntegrityDacKeyType.ForeignKey;

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
					case TypeNames.ReferentialIntegrity.By_TypeName
					when baseType.ContainingType?.Name == TypeNames.ReferentialIntegrity.ForeignKeyOfName:    //Case of foreign key declared via primary key
						{
							var usedFields = baseType.TypeArguments;
							bool areValidFields = !usedFields.IsDefaultOrEmpty && usedFields.All(dacFieldArg => dac.FieldsByNames.ContainsKey(dacFieldArg.Name));

							return areValidFields
								? usedFields.OrderBy(dacField => dacField.MetadataName).ToList(capacity: usedFields.Length)
								: new List<ITypeSymbol>();
						}

					case TypeNames.ReferentialIntegrity.WithTablesOf_TypeName
					when baseType.ContainingType?.Name == TypeNames.ReferentialIntegrity.AsSimpleKeyName:     //Case of simple 1-field foreign key with explicit declaration
						{
							var isRelatedTo_Type = baseType.ContainingType.ContainingType;
							var usedDacField = GetDacFieldFromIsRelatedToType(dac, isRelatedTo_Type);

							return usedDacField != null
								? new List<ITypeSymbol>(capacity: 1) { usedDacField }
								: new List<ITypeSymbol>();
						}

					case TypeNames.ReferentialIntegrity.WithTablesOf_TypeName
					when baseType.ContainingType?.Name == TypeNames.ReferentialIntegrity.CompositeKey:       //Case of complex foreign key with explicit declaration
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

			if (isRelatedTo_Type?.Name != TypeNames.ReferentialIntegrity.IsRelatedTo || field_Type?.Name != TypeNames.ReferentialIntegrity.Field ||
				field_Type.TypeArguments.Length != 1)
			{
				return null;
			}

			var usedDacField = field_Type.TypeArguments[0];
			return dac.FieldsByNames.ContainsKey(usedDacField.Name)
				? usedDacField
				: null;
		}

		protected override void MakeSpecificDacKeysAnalysis(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
															List<INamedTypeSymbol> dacForeignKeys, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			if (dacForeignKeys.Count == 0)
			{
				ReportNoForeignKeyDeclarationsInDac(symbolContext, context, dac);
				return;
			}

			//TODO extend logic to check foreign keys declarations
		}

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
	}
}