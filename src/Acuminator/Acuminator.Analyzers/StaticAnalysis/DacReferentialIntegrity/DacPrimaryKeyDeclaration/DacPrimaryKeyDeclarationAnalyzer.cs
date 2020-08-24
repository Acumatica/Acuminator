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
				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac,
				Descriptors.PX1036_WrongDacPrimaryKeyName,
				Descriptors.PX1036_WrongDacSingleUniqueKeyName,
				Descriptors.PX1036_WrongDacMultipleUniqueKeyName
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

			var keyDeclarations = GetPrimaryKeyDeclarations(context, dac, symbolContext.CancellationToken).ToList(capacity: 1);

			switch (keyDeclarations.Count)
			{
				case 0:
					ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
					return;

				case 1:
					AnalyzeSinglePrimaryKeyDeclaration(symbolContext, context, keyDeclarations[0]);
					return;

				case 2:
					if (CheckThatAllPrimaryKeysHaveUniqueSetsOfFields(symbolContext, context, dac, keyDeclarations))
					{
						AnalyzeDeclarationOfTwoPrimaryKeys(symbolContext, context, dac, keyDeclarations[0], keyDeclarations[1]);
					}

					return;

				default:
					if (CheckThatAllPrimaryKeysHaveUniqueSetsOfFields(symbolContext, context, dac, keyDeclarations))
					{
						CheckBigGroupOfKeysForPrimaryKeyAndUniqueKeysContainer(symbolContext, context, dac, keyDeclarations);
					}

					return;
			}
		}

		private IEnumerable<INamedTypeSymbol> GetPrimaryKeyDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken) =>
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

		private void AnalyzeSinglePrimaryKeyDeclaration(SymbolAnalysisContext symbolContext, PXContext context, INamedTypeSymbol keyDeclaration)
		{
			if (keyDeclaration.Name != TypeNames.PrimaryKeyClassName)
			{
				ReportKeyDeclarationWithWrongName(symbolContext, context, keyDeclaration, RefIntegrityDacKeyType.PrimaryKey);
			}		
		}

		private bool CheckThatAllPrimaryKeysHaveUniqueSetsOfFields(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
																   List<INamedTypeSymbol> keyDeclarations)
		{
			var primaryKeysGroupedByFields = GetPrimaryKeysGroupedBySetOfFields(context, dac, keyDeclarations, symbolContext.CancellationToken);
			var duplicateKeySets = primaryKeysGroupedByFields.Values.Where(keys => keys.Count > 1);
			bool hasDuplicates = false;

			// We group keys by sets of used fields and then report each set with duplicate keys separately,
			// passing the locations of other duplicate fields in a set to code fix. 
			// This way if there are two different sets of duplicate keys the code fix will affect only the set to which it was applied
			foreach (List<INamedTypeSymbol> duplicateKeys in duplicateKeySets)
			{
				hasDuplicates = true;
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
									Diagnostic.Create(Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac, location, otherLocations),
									context.CodeAnalysisSettings);
				}
			}

			return hasDuplicates;
		}

		private Dictionary<string, List<INamedTypeSymbol>> GetPrimaryKeysGroupedBySetOfFields(PXContext context, DacSemanticModel dac, List<INamedTypeSymbol> keyDeclarations,
																							  CancellationToken cancellationToken)
		{
			var processedPrimaryKeysByHash = new Dictionary<string, List<INamedTypeSymbol>>(capacity: keyDeclarations.Count);

			foreach (var primaryKey in keyDeclarations)
			{
				// We don't check custom IPrimaryKey implementations since it will be impossible to deduce referenced set of DAC fields in a general case.
				// Instead we only analyze primary keys made with generic class By<,...,> or derived from it. This should handle 99% of PK use cases
				var byType = primaryKey.GetBaseTypesAndThis()
									   .OfType<INamedTypeSymbol>()
									   .FirstOrDefault(type => type.Name == TypeNames.By_TypeName && !type.TypeArguments.IsDefaultOrEmpty &&
															   type.TypeArguments.All(dacFieldArg => dac.FieldsByNames.ContainsKey(dacFieldArg.Name)));
				if (byType == null)
					continue;

				cancellationToken.ThrowIfCancellationRequested();

				var stringHash = byType.TypeArguments
									   .Select(dacFieldUsedByKey => dacFieldUsedByKey.MetadataName)
									   .OrderBy(metadataName => metadataName)
									   .Join(separator: ",");

				if (processedPrimaryKeysByHash.TryGetValue(stringHash, out var processedKeysList))
				{
					processedKeysList.Add(primaryKey);
				}
				else
				{
					processedKeysList = new List<INamedTypeSymbol>(capacity: 1) { primaryKey };
					processedPrimaryKeysByHash.Add(stringHash, processedKeysList);
				}
			}

			return processedPrimaryKeysByHash;
		}

		private void AnalyzeDeclarationOfTwoPrimaryKeys(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
														INamedTypeSymbol firstKeyDeclaration, INamedTypeSymbol secondKeyDeclaration)
		{
			var primaryKey = firstKeyDeclaration.Name == TypeNames.PrimaryKeyClassName
				? firstKeyDeclaration
				: secondKeyDeclaration.Name == TypeNames.PrimaryKeyClassName
					? secondKeyDeclaration
					: null;

			//If there is no primary key - suggest to rename one of the keys and quit
			if (primaryKey == null)
			{
				ReportKeyDeclarationWithWrongName(symbolContext, context, firstKeyDeclaration, RefIntegrityDacKeyType.PrimaryKey);
				ReportKeyDeclarationWithWrongName(symbolContext, context, secondKeyDeclaration, RefIntegrityDacKeyType.PrimaryKey);
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

			//If there is no primary key - suggest to rename one of the keys and quit
			if (primaryKey == null)
			{
				keyDeclarations.ForEach(key => ReportKeyDeclarationWithWrongName(symbolContext, context, key, RefIntegrityDacKeyType.PrimaryKey));
				return;
			}

			var uniqueKeysContainer = dac.Symbol.GetTypeMembers(TypeNames.UniqueKeyClassName)
												.FirstOrDefault();
			
		}	
	}
}