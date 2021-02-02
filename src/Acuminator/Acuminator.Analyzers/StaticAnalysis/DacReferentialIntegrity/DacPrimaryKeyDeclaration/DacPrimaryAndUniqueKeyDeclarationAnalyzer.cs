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
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using static Acuminator.Utilities.Roslyn.Constants.TypeNames;


namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	public class DacPrimaryAndUniqueKeyDeclarationAnalyzer : DacKeyDeclarationAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields,

				Descriptors.PX1036_WrongDacPrimaryKeyName,
				Descriptors.PX1036_WrongDacSingleUniqueKeyName,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations,

				Descriptors.PX1037_UnboundDacFieldInKeyDeclaration
			);

		protected override bool IsKeySymbolDefined(PXContext context) => context.ReferentialIntegritySymbols.IPrimaryKey != null;

		protected override List<INamedTypeSymbol> GetDacKeysDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken) =>
			 dac.Symbol.GetFlattenedNestedTypes(shouldWalkThroughNestedTypesPredicate: nestedType => !nestedType.IsDacOrExtension(context),
												cancellationToken)
					   .Where(nestedType => nestedType.ImplementsInterface(context.ReferentialIntegritySymbols.IPrimaryKey))
					   .ToList(capacity: 1);

		/// <summary>
		/// Gets the list of DAC fields ordered by their metadataname which are used by the DAC primary or unique <paramref name="key"/>.
		/// </summary>
		/// <param name="dac">The DAC.</param>
		/// <param name="primaryOrUniqueKey">The primary or unique key.</param>
		/// <returns>
		/// An ordered list of DAC fields used in primary or unique <paramref name="key"/> declaration.
		/// </returns>
		protected override List<ITypeSymbol> GetOrderedDacFieldsUsedByKey(DacSemanticModel dac, INamedTypeSymbol primaryOrUniqueKey)
		{
			// We don't support custom IPrimaryKey implementations since it will be impossible to deduce referenced set of DAC fields in a general case.
			// Instead we only analyze primary keys made with generic class By<,...,> or derived from it. This should handle 99% of PK use cases
			var byType = primaryOrUniqueKey.GetBaseTypesAndThis()
										   .OfType<INamedTypeSymbol>()
										   .FirstOrDefault(type => type.Name == ReferentialIntegrity.By_TypeName && !type.TypeArguments.IsDefaultOrEmpty &&
																   type.TypeArguments.All(dacFieldArg => dac.FieldsByNames.ContainsKey(dacFieldArg.Name)));

			return byType?.TypeArguments.OrderBy(dacField => dacField.MetadataName)
										.ToList(capacity: byType.TypeArguments.Length) 
						 ?? new List<ITypeSymbol>();
		}

		protected override ITypeSymbol GetParentDacFromKey(PXContext context, INamedTypeSymbol primaryOrUniqueKey)
		{
			// We support only the most frequent case - the primary and unique keys which implement generic IPrimaryKey<TDAC> interface
			INamedTypeSymbol primaryKeyInterface = primaryOrUniqueKey.AllInterfaces
																	 .FirstOrDefault(i => i.MetadataName == TypeFullNames.IPrimaryKey1);

			if (primaryKeyInterface == null || primaryKeyInterface.TypeArguments.Length != 1)
				return null;

			return primaryKeyInterface.TypeArguments[0];
		}

		protected override Location GetUnboundDacFieldLocation(ClassDeclarationSyntax keyNode, ITypeSymbol unboundDacFieldInKey)
		{
			if (keyNode.BaseList.Types.Count == 0)
				return null;

			BaseTypeSyntax baseTypeNode = keyNode.BaseList.Types[0];

			if (!(baseTypeNode.Type is QualifiedNameSyntax qualifiedName) || !(qualifiedName.Right is GenericNameSyntax byTypeNode))
				return null;

			return GetUnboundDacFieldLocationFromTypeArguments(byTypeNode, unboundDacFieldInKey);
		}

		protected override bool ShouldMakeSpecificAnalysisForDacKeys(PXContext context, DacSemanticModel dac) =>
			base.ShouldMakeSpecificAnalysisForDacKeys(context, dac) &&
			dac.DacProperties.Any(property => property.IsKey);

		protected override void MakeSpecificDacKeysAnalysis(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac, 
															List<INamedTypeSymbol> keyDeclarations, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			switch (keyDeclarations?.Count)
			{
				case 0:
				case null:
					ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
					return;

				case 1:
					AnalyzeSinglePrimaryKeyDeclaration(symbolContext, context, dac, keyDeclarations[0], dacFieldsByKey);
					return;

				case 2:		
					AnalyzeDeclarationOfTwoPrimaryKeys(symbolContext, context, dac, keyDeclarations, dacFieldsByKey);					
					return;

				default:			
					CheckBigGroupOfKeysForPrimaryKeyAndUniqueKeysContainer(symbolContext, context, dac, keyDeclarations, dacFieldsByKey);
					return;
			}
		}

		private void ReportNoPrimaryKeyDeclarationsInDac(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			Location location = dac.Node.Identifier.GetLocation() ?? dac.Node.GetLocation();

			if (location != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1033_MissingDacPrimaryKeyDeclaration, location),
					context.CodeAnalysisSettings);
			} 
		}

		private void AnalyzeSinglePrimaryKeyDeclaration(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac, 
														INamedTypeSymbol keyDeclaration, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			if (keyDeclaration.Name != ReferentialIntegrity.PrimaryKeyClassName)
			{
				string keysHash = GetHashForDacKeys(dac);

				if (keysHash == GetHashForSetOfDacFieldsUsedByKey(keyDeclaration, dacFieldsByKey))
					ReportKeyDeclarationWithWrongName(symbolContext, context, dac, keyDeclaration, RefIntegrityDacKeyType.PrimaryKey);
				else
					ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
			}		
		}

		private void AnalyzeDeclarationOfTwoPrimaryKeys(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac, 
														List<INamedTypeSymbol> keyDeclarations, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			INamedTypeSymbol firstKeyDeclaration = keyDeclarations[0];
			INamedTypeSymbol secondKeyDeclaration = keyDeclarations[1];

			var primaryKey = firstKeyDeclaration.Name == ReferentialIntegrity.PrimaryKeyClassName
				? firstKeyDeclaration
				: secondKeyDeclaration.Name == ReferentialIntegrity.PrimaryKeyClassName
					? secondKeyDeclaration
					: null;

			if (primaryKey == null)
			{
				//If there is no primary key - try to find suitable unique key and rename it. Otherwise report no primary key in DAC
				ProcessDacWithoutPrimaryKeyAndWithSeveralUniqueKeys(symbolContext, context, dac, keyDeclarations, dacFieldsByKey);
				return;
			}

			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var uniqueKeyDeclaration = ReferenceEquals(primaryKey, firstKeyDeclaration)
				? secondKeyDeclaration
				: firstKeyDeclaration;

			//The second key is a unique key. If it does not named "UK" we should rename it
			if (uniqueKeyDeclaration.Name != ReferentialIntegrity.UniqueKeyClassName)
			{
				ReportKeyDeclarationWithWrongName(symbolContext, context, dac, uniqueKeyDeclaration, RefIntegrityDacKeyType.UniqueKey);
			}			
		}

		private void CheckBigGroupOfKeysForPrimaryKeyAndUniqueKeysContainer(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
																			List<INamedTypeSymbol> keyDeclarations, 
																			Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			var primaryKey = keyDeclarations.Find(key => key.Name == ReferentialIntegrity.PrimaryKeyClassName);
	
			if (primaryKey == null)
			{
				//If there is no primary key - try to find suitable unique key and rename it. Otherwise report no primary key in DAC
				ProcessDacWithoutPrimaryKeyAndWithSeveralUniqueKeys(symbolContext, context, dac, keyDeclarations, dacFieldsByKey);
				return;
			}

			INamedTypeSymbol uniqueKeysContainer = dac.Symbol.GetTypeMembers(ReferentialIntegrity.UniqueKeyClassName)
															 .FirstOrDefault();
			
			//We can register code fix only if there is no UK nested type in DAC or there is a public static UK class. Otherwise we will break the code.
			bool registerCodeFix = uniqueKeysContainer == null || 
								   (uniqueKeysContainer.DeclaredAccessibility == Accessibility.Public && uniqueKeysContainer.IsStatic);

			List<INamedTypeSymbol> keysNotInContainer = GetKeysNotInContainer(keyDeclarations, uniqueKeysContainer, primaryKey);

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
				{ nameof(RefIntegrityDacKeyType), RefIntegrityDacKeyType.UniqueKey.ToString() },
				{ nameof(UniqueKeyCodeFixType), UniqueKeyCodeFixType.MultipleUniqueKeys.ToString() },
				{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
			}
			.ToImmutableDictionary();	
			
			foreach (Location keyLocation in keysNotInContainerLocations)
			{
				var otherKeyLocations = keysNotInContainerLocations.Where(location => location != keyLocation);
				var additionalLocations = dacLocationArray.Concat(otherKeyLocations);

				symbolContext.ReportDiagnosticWithSuppressionCheck(
									Diagnostic.Create(Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations, keyLocation, additionalLocations, diagnosticProperties),
									context.CodeAnalysisSettings);			
			}
		}

		private List<INamedTypeSymbol> GetKeysNotInContainer(List<INamedTypeSymbol> keyDeclarations, INamedTypeSymbol uniqueKeysContainer, INamedTypeSymbol primaryKey)
		{
			bool containerDeclaredIncorrectly = uniqueKeysContainer?.DeclaredAccessibility != Accessibility.Public || !uniqueKeysContainer.IsStatic;

			if (containerDeclaredIncorrectly)
			{
				return keyDeclarations.Where(key => key != primaryKey).ToList(capacity: keyDeclarations.Count - 1);
			}
			else
			{
				return keyDeclarations.Where(key => key != primaryKey && key.ContainingType != uniqueKeysContainer && 
													!key.GetContainingTypes().Contains(uniqueKeysContainer))
									  .ToList(capacity: keyDeclarations.Count - 1);
			}
		}

		private void ProcessDacWithoutPrimaryKeyAndWithSeveralUniqueKeys(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
																		 List<INamedTypeSymbol> keyDeclarations, 
																		 Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			string dacKeysHash = GetHashForDacKeys(dac);

			foreach (INamedTypeSymbol uniqueKey in keyDeclarations)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				string uniqueKeyHash = GetHashForSetOfDacFieldsUsedByKey(uniqueKey, dacFieldsByKey);
				
				if (dacKeysHash == uniqueKeyHash)	
				{
					// Report suitable unique keys for renaming. 
					// All key declarations were already checked for duplicates, therefore we can return after we find unique key with a suitable set of fields
					ReportKeyDeclarationWithWrongName(symbolContext, context, dac, uniqueKey, RefIntegrityDacKeyType.PrimaryKey);
					return;
				}
			}

			//If no suitable unique key is found then show diagnostic for missing primary key
			ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
		}

		private string GetHashForDacKeys(DacSemanticModel dac)
		{
			var dacKeys = dac.DacProperties.Where(property => property.IsKey)
										   .Select(property => dac.FieldsByNames[property.Name].Symbol);

			return GetHashForSetOfDacFields(dacKeys, areFieldsOrdered: false);
		}
	}
}