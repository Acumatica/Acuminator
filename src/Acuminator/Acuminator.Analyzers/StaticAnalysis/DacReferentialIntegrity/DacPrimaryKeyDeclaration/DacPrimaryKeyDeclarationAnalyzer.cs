using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	public class DacPrimaryKeyDeclarationAnalyzer : DacKeyDeclarationAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration,
				Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields,
				Descriptors.PX1036_WrongDacPrimaryKeyName,
				Descriptors.PX1036_WrongDacSingleUniqueKeyName,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations
			);

		protected override bool IsKeySymbolDefined(PXContext context) => context.ReferentialIntegritySymbols.IPrimaryKey != null;

		protected override bool ShouldAnalyzeDac(PXContext context, DacSemanticModel dac)
		{
			if (!base.ShouldAnalyzeDac(context, dac))
				return false;

			bool hasKeys = false;

			foreach (var key in dac.DacProperties.Where(property => property.IsKey))
			{
				hasKeys = true;

				if (key.BoundType != BoundType.DbBound)
					return false;				
			}

			return hasKeys;
		}

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var keyDeclarations = GetPrimaryAndUniqueKeyDeclarations(context, dac, symbolContext.CancellationToken).ToList(capacity: 1);

			switch (keyDeclarations.Count)
			{
				case 0:
					ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
					return;

				case 1:
					AnalyzeSinglePrimaryKeyDeclaration(symbolContext, context, dac, keyDeclarations[0]);
					return;

				case 2:
					if (CheckThatAllKeysHaveUniqueSetsOfFields(symbolContext, context, dac, keyDeclarations))
					{
						AnalyzeDeclarationOfTwoPrimaryKeys(symbolContext, context, dac, keyDeclarations);
					}

					return;

				default:
					if (CheckThatAllKeysHaveUniqueSetsOfFields(symbolContext, context, dac, keyDeclarations))
					{
						CheckBigGroupOfKeysForPrimaryKeyAndUniqueKeysContainer(symbolContext, context, dac, keyDeclarations);
					}

					return;
			}
		}

		private IEnumerable<INamedTypeSymbol> GetPrimaryAndUniqueKeyDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken) =>
			 dac.Symbol.GetFlattenedNestedTypes(shouldWalkThroughNestedTypesPredicate: nestedType => !nestedType.IsDacOrExtension(context),
												cancellationToken)
					   .Where(nestedType => nestedType.ImplementsInterface(context.ReferentialIntegritySymbols.IPrimaryKey));

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
														INamedTypeSymbol keyDeclaration)
		{
			if (keyDeclaration.Name != TypeNames.PrimaryKeyClassName)
			{
				string keysHash = GetHashForDacKeys(dac);

				if (keysHash == GetHashForSetOfDacFieldsUsedByKey(dac, keyDeclaration))
					ReportKeyDeclarationWithWrongName(symbolContext, context, keyDeclaration, RefIntegrityDacKeyType.PrimaryKey);
				else
					ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
			}		
		}

		private bool CheckThatAllKeysHaveUniqueSetsOfFields(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
															List<INamedTypeSymbol> keyDeclarations)
		{
			var keysGroupedByFields = GetKeysGroupedBySetOfFields(dac, keyDeclarations, symbolContext.CancellationToken);
			var duplicateKeySets = keysGroupedByFields.Values.Where(keys => keys.Count > 1);
			bool allFieldsUnique = true;

			// We group keys by sets of used fields and then report each set with duplicate keys separately,
			// passing the locations of other duplicate fields in a set to code fix. 
			// This way if there are two different sets of duplicate keys the code fix will affect only the set to which it was applied
			foreach (List<INamedTypeSymbol> duplicateKeys in duplicateKeySets)
			{
				allFieldsUnique = false;
				var locations = duplicateKeys.Select(declaration => declaration.GetSyntax(symbolContext.CancellationToken))
											 .OfType<ClassDeclarationSyntax>()
											 .Select(keyClassDeclaration => keyClassDeclaration.Identifier.GetLocation() ??
																			keyClassDeclaration.GetLocation())
											 .Where(location => location != null)
											 .ToList(capacity: duplicateKeys.Count);

				for (int i = 0; i < locations.Count; i++)
				{
					Location location = locations[i];
					var otherLocations = locations.Where((_, index) => index != i);

					symbolContext.ReportDiagnosticWithSuppressionCheck(
									Diagnostic.Create(Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields, location, otherLocations),
									context.CodeAnalysisSettings);
				}
			}

			return allFieldsUnique;
		}

		private Dictionary<string, List<INamedTypeSymbol>> GetKeysGroupedBySetOfFields(DacSemanticModel dac, List<INamedTypeSymbol> keyDeclarations,
																					   CancellationToken cancellationToken)
		{
			var processedKeysByHash = new Dictionary<string, List<INamedTypeSymbol>>(capacity: keyDeclarations.Count);

			foreach (var primaryOrUniqueKey in keyDeclarations)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var stringHash = GetHashForSetOfDacFieldsUsedByKey(dac, primaryOrUniqueKey);

				if (stringHash == null)
					continue;

				if (processedKeysByHash.TryGetValue(stringHash, out var processedKeysList))
				{
					processedKeysList.Add(primaryOrUniqueKey);
				}
				else
				{
					processedKeysList = new List<INamedTypeSymbol>(capacity: 1) { primaryOrUniqueKey };
					processedKeysByHash.Add(stringHash, processedKeysList);
				}
			}

			return processedKeysByHash;
		}

		private void AnalyzeDeclarationOfTwoPrimaryKeys(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac, 
														List<INamedTypeSymbol> keyDeclarations)
		{
			INamedTypeSymbol firstKeyDeclaration = keyDeclarations[0];
			INamedTypeSymbol secondKeyDeclaration = keyDeclarations[1];

			var primaryKey = firstKeyDeclaration.Name == TypeNames.PrimaryKeyClassName
				? firstKeyDeclaration
				: secondKeyDeclaration.Name == TypeNames.PrimaryKeyClassName
					? secondKeyDeclaration
					: null;

			if (primaryKey == null)
			{
				//If there is no primary key - try to find suitable unique key and rename it. Otherwise report no primary key in DAC
				ProcessDacWithoutPrimaryKeyAndWithSeveralUniqueKeys(symbolContext, context, dac, keyDeclarations);
				return;
			}

			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var uniqueKeyDeclaration = ReferenceEquals(primaryKey, firstKeyDeclaration)
				? secondKeyDeclaration
				: firstKeyDeclaration;

			//The second key is a unique key. If it does not named "UK" we should rename it
			if (uniqueKeyDeclaration.Name != TypeNames.UniqueKeyClassName)
			{
				ReportKeyDeclarationWithWrongName(symbolContext, context, uniqueKeyDeclaration, RefIntegrityDacKeyType.UniqueKey);
			}			
		}

		private void CheckBigGroupOfKeysForPrimaryKeyAndUniqueKeysContainer(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
																			List<INamedTypeSymbol> keyDeclarations)
		{
			var primaryKey = keyDeclarations.Find(key => key.Name == TypeNames.PrimaryKeyClassName);
	
			if (primaryKey == null)
			{
				//If there is no primary key - try to find suitable unique key and rename it. Otherwise report no primary key in DAC
				ProcessDacWithoutPrimaryKeyAndWithSeveralUniqueKeys(symbolContext, context, dac, keyDeclarations);
				return;
			}

			INamedTypeSymbol uniqueKeysContainer = dac.Symbol.GetTypeMembers(TypeNames.UniqueKeyClassName)
															 .FirstOrDefault();

			//We can register code fix only if there is no UK nested type in DAC or there is a public static UK class. Otherwise we will break the code.
			bool registerCodeFix = uniqueKeysContainer == null || 
								   (uniqueKeysContainer.DeclaredAccessibility == Accessibility.Public && uniqueKeysContainer.IsStatic);

			var keyDeclarationsNotInContainer = uniqueKeysContainer == null
				? keyDeclarations.Where(key => key != primaryKey)
				: keyDeclarations.Where(key => key != primaryKey &&
											   key.ContainingType != uniqueKeysContainer &&
											   !key.GetContainingTypes().Contains(uniqueKeysContainer));

			var diagnosticProperties = new Dictionary<string, string>
			{
				{ nameof(RefIntegrityDacKeyType), RefIntegrityDacKeyType.UniqueKey.ToString() },
				{ nameof(UniqueKeyCodeFixType), UniqueKeyCodeFixType.MultipleUniqueKeys.ToString() },
				{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
			}
			.ToImmutableDictionary();

			Location dacLocation = dac.Node.GetLocation();
			Location[] additionalLocations = new[] { dacLocation };

			foreach (var key in keyDeclarationsNotInContainer)
			{
				var keyClassDeclaration = key.GetSyntax(symbolContext.CancellationToken) as ClassDeclarationSyntax;
				var location = keyClassDeclaration?.Identifier.GetLocation() ?? keyClassDeclaration?.GetLocation();

				if (location == null)
					continue;

				symbolContext.ReportDiagnosticWithSuppressionCheck(
									Diagnostic.Create(Descriptors.PX1036_WrongDacMultipleUniqueKeyDeclarations, location, additionalLocations, diagnosticProperties),
									context.CodeAnalysisSettings);			
			}
		}

		private void ProcessDacWithoutPrimaryKeyAndWithSeveralUniqueKeys(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
																		 List<INamedTypeSymbol> keyDeclarations)
		{
			string dacKeysHash = GetHashForDacKeys(dac);

			foreach (INamedTypeSymbol uniqueKey in keyDeclarations)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				string uniqueKeyHash = GetHashForSetOfDacFieldsUsedByKey(dac, uniqueKey);
				
				if (dacKeysHash == uniqueKeyHash)	
				{
					// Report suitable unique keys for renaming. 
					// All key declarations were already checked for duplicates, therefore we can return after we find unique key with a suitable set of fields
					ReportKeyDeclarationWithWrongName(symbolContext, context, uniqueKey, RefIntegrityDacKeyType.PrimaryKey);
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

			return GetHashForSetOfDacFields(dacKeys);
		}

		private string GetHashForSetOfDacFieldsUsedByKey(DacSemanticModel dac, INamedTypeSymbol primaryOrUniqueKey)
		{
			// We don't support custom IPrimaryKey implementations since it will be impossible to deduce referenced set of DAC fields in a general case.
			// Instead we only analyze primary keys made with generic class By<,...,> or derived from it. This should handle 99% of PK use cases
			var byType = primaryOrUniqueKey.GetBaseTypesAndThis()
										   .OfType<INamedTypeSymbol>()
										   .FirstOrDefault(type => type.Name == TypeNames.By_TypeName && !type.TypeArguments.IsDefaultOrEmpty &&
																   type.TypeArguments.All(dacFieldArg => dac.FieldsByNames.ContainsKey(dacFieldArg.Name)));
			return byType != null
				? GetHashForSetOfDacFields(byType.TypeArguments)
				: null;
		}

		/// <summary>
		/// Gets string hash for set of DAC fields. This method is an optimization for <see cref="ImmutableArray{T}"/> which avoids boxing.
		/// </summary>
		/// <param name="dacFields">The DAC fields.</param>
		/// <returns>
		/// The hash for set of DAC fields.
		/// </returns>
		private string GetHashForSetOfDacFields(ImmutableArray<ITypeSymbol> dacFields) =>
			dacFields.Select(dacField => dacField.MetadataName)
					 .OrderBy(metadataName => metadataName)
					 .Join(separator: ",");

		private string GetHashForSetOfDacFields(IEnumerable<ITypeSymbol> dacFields) =>
			dacFields.Select(dacField => dacField.MetadataName)
					 .OrderBy(metadataName => metadataName)
					 .Join(separator: ",");
	}
}