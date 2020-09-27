using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	/// <summary>
	/// Base class for DAC key declaration analyzers which provides base checks for all types of DAC keys - check for unbound DAC fields in key declaration and check for the duplicate field sets.
	/// This analyzer provides template method for analysis of DAC keys with two derived analyzers providing implementation specific details for foreign DAC keys and primary/unique DAC keys.
	/// </summary>
	public abstract class DacKeyDeclarationAnalyzerBase : DacAggregatedAnalyzerBase
	{
		public override bool ShouldAnalyze(PXContext context, DacSemanticModel dac) =>
			base.ShouldAnalyze(context, dac) &&
			dac.DacType == DacType.Dac && !dac.IsMappedCacheExtension && !dac.Symbol.IsAbstract &&
			IsKeySymbolDefined(context);

		protected abstract bool IsKeySymbolDefined(PXContext context);

		public sealed override void Analyze(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var keys = GetDacKeysDeclarations(context, dac, symbolContext.CancellationToken);
			var dacFieldsByKey = GetUsedDacFieldsByKey(dac, keys);

			// We place checks for key declarations for a wider scope of DACs here. 
			// DACs without PXCacheName or PXPrimaryGraph attributes, fully-unbound DACs and DACs without key properties will all still be checked here
			if (!MakeCommonKeyDeclarationsChecks(symbolContext, context, dac, keys, dacFieldsByKey))
				return;

			// Now we perform additional more specific and code style-related checks.
			// So, we filter out DACs without PXCacheName or PXPrimaryGraph attributes, fully-unbound DACs and some other DACs
			if (ShouldMakeSpecificAnalysisForDacKeys(context, dac))
			{
				MakeSpecificDacKeysAnalysis(symbolContext, context, dac, keys, dacFieldsByKey);
			}
		}	

		protected abstract List<INamedTypeSymbol> GetDacKeysDeclarations(PXContext context, DacSemanticModel dac, CancellationToken cancellationToken);

		private Dictionary<INamedTypeSymbol, List<ITypeSymbol>> GetUsedDacFieldsByKey(DacSemanticModel dac, List<INamedTypeSymbol> keys) =>
			keys.Select(key => (Key: key, KeyFields: GetOrderedDacFieldsUsedByKey(dac, key)))
				.ToDictionary(keyWithFields => keyWithFields.Key, 
							  keyWithFields => keyWithFields.KeyFields);

		protected abstract List<ITypeSymbol> GetOrderedDacFieldsUsedByKey(DacSemanticModel dac, INamedTypeSymbol key);

		protected virtual bool MakeCommonKeyDeclarationsChecks(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
															   List<INamedTypeSymbol> keys, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			//Place all common checks that should be applied to a wider scope of DACs here
			bool baseChecksPassed = CheckDacKeysForUnboundDacFields(symbolContext, context, dac, dacFieldsByKey);
			baseChecksPassed = CheckThatAllKeysHaveUniqueSetsOfFields(symbolContext, context, keys, dacFieldsByKey) && baseChecksPassed;
			return baseChecksPassed;
		}

		private bool CheckDacKeysForUnboundDacFields(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
													 Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			if (dacFieldsByKey.Count == 0)
				return true;

			bool noUnboundFieldsInKeys = true;

			foreach (var (key, usedDacFields) in dacFieldsByKey)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (usedDacFields.Count == 0)
					continue;

				var unboundDacFieldsInKey = usedDacFields.Where(dacField => dac.PropertiesByNames.TryGetValue(dacField.Name, out DacPropertyInfo dacProperty) &&
																			dacProperty.BoundType == BoundType.Unbound);
				ClassDeclarationSyntax keyNode = null;

				foreach (ITypeSymbol unboundDacFieldInKey in unboundDacFieldsInKey)
				{
					keyNode ??= (key.GetSyntax(symbolContext.CancellationToken) as ClassDeclarationSyntax);

					if (keyNode == null)
						break;
					else if (!ReportKeyWithUnboundDacField(symbolContext, context, key, keyNode, unboundDacFieldInKey))
						continue;

					noUnboundFieldsInKeys = false;
				}
			}

			return noUnboundFieldsInKeys;
		}

		private bool ReportKeyWithUnboundDacField(SymbolAnalysisContext symbolContext, PXContext context, INamedTypeSymbol key, ClassDeclarationSyntax keyNode,
												  ITypeSymbol unboundDacFieldInKey)
		{
			var location = GetUnboundDacFieldLocation(keyNode, unboundDacFieldInKey) ?? keyNode.Identifier.GetLocation() ?? keyNode.GetLocation();

			if (location == null)
				return false;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
							Diagnostic.Create(Descriptors.PX1037_UnboundDacFieldInKeyDeclaration, location),
							context.CodeAnalysisSettings);
			return true;
		}

		protected abstract Location GetUnboundDacFieldLocation(ClassDeclarationSyntax keyNode, ITypeSymbol unboundDacFieldInKey);

		protected Location GetUnboundDacFieldLocationFromTypeArguments(GenericNameSyntax genericNodeWithFieldTypeArguments, ITypeSymbol unboundDacFieldInKey)
		{
			foreach (TypeSyntax genericArgNode in genericNodeWithFieldTypeArguments.TypeArgumentList.Arguments)
			{				
				switch (genericArgNode)
				{
					case SimpleNameSyntax fieldNode when fieldNode.Identifier.Text == unboundDacFieldInKey.Name:   //Case when type argument is just a name of a field: Field<SOLineFkAsSimpleKey.inventoryID>
						return fieldNode.GetLocation();

					case QualifiedNameSyntax complexFieldName when complexFieldName.Right.Identifier.Text == unboundDacFieldInKey.Name:  //Case when type argument is a complex name: Field<{optional PX.Objects.SO.}SOLine.inventoryID>

						SimpleNameSyntax dacNameNode = complexFieldName.Left switch
						{
							SimpleNameSyntax dacNameNodeWithoutNamespaces => dacNameNodeWithoutNamespaces,       //case Dac.field
							QualifiedNameSyntax dacNameNodeWithNamespaces => dacNameNodeWithNamespaces.Right,    //cases Namespace.Dac.field and Alias::Namespace.Dac.field
							AliasQualifiedNameSyntax dacNameNodeWithAlias => dacNameNodeWithAlias.Name,          //case  Alias::Dac.field
							_ => null
						};

						return dacNameNode?.Identifier.Text == unboundDacFieldInKey.ContainingType.Name
							? complexFieldName.GetLocation()
							: null;

						// Type argument node can also be an alias (like Alias::Type). However, in most cases it is still QualifiedNameSyntax which may contain alias node in subtree as the leftmost node
						// The only case when type argument node is directly AliasQualifiedNameSyntax is when it has a simple structure Alias::Type. 
						// However, a DAC field is a nested type and you can declare nested type via alias only like this: Alias::Dac.field. And this is QualifiedNameSyntax.
						// Therefore there is no need to consider AliasQualifiedNameSyntax. 
						// Also we don't need to consider array and pointer type arguments as there is no way DAC field is used in such way. 
				}
			}

			return null;
		}

		private bool CheckThatAllKeysHaveUniqueSetsOfFields(SymbolAnalysisContext symbolContext, PXContext context,
															List<INamedTypeSymbol> keyDeclarations, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey)
		{
			if (keyDeclarations.Count < 2 || dacFieldsByKey.Count == 0)
				return true;

			var keysGroupedByFields = GetKeysGroupedBySetOfFields(keyDeclarations, dacFieldsByKey, symbolContext.CancellationToken);
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

		private Dictionary<string, List<INamedTypeSymbol>> GetKeysGroupedBySetOfFields(List<INamedTypeSymbol> keyDeclarations,
																					   Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey, 
																					   CancellationToken cancellationToken)
		{
			var processedKeysByHash = new Dictionary<string, List<INamedTypeSymbol>>(capacity: keyDeclarations.Count);

			foreach (var key in keyDeclarations)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var stringHash = GetHashForSetOfDacFieldsUsedByKey(key, dacFieldsByKey);

				if (stringHash == null)
					continue;

				if (processedKeysByHash.TryGetValue(stringHash, out var processedKeysList))
				{
					processedKeysList.Add(key);
				}
				else
				{
					processedKeysList = new List<INamedTypeSymbol>(capacity: 1) { key };
					processedKeysByHash.Add(stringHash, processedKeysList);
				}
			}

			return processedKeysByHash;
		}

		protected string GetHashForSetOfDacFieldsUsedByKey(INamedTypeSymbol key, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey) =>
			dacFieldsByKey.TryGetValue(key, out List<ITypeSymbol> usedDacFields) && usedDacFields.Count > 0
				? GetHashForSetOfDacFields(usedDacFields, areFieldsOrdered: true)
				: null;

		protected string GetHashForSetOfDacFields(IEnumerable<ITypeSymbol> dacFields, bool areFieldsOrdered)
		{
			var fieldNames = dacFields.Select(dacField => dacField.MetadataName);

			if (!areFieldsOrdered)
				fieldNames = fieldNames.OrderBy(metadataName => metadataName);

			return fieldNames.Join(separator: ",");
		}

		protected virtual bool ShouldMakeSpecificAnalysisForDacKeys(PXContext context, DacSemanticModel dac)
		{
			if (dac.IsFullyUnbound())
				return false;

			var dacAttributes = dac.Symbol.GetAttributes();

			if (dacAttributes.IsDefaultOrEmpty)
				return false;

			var pxCacheNameAttribute = context.AttributeTypes.PXCacheNameAttribute;
			var pxPrimaryGraphAttribute = context.AttributeTypes.PXPrimaryGraphAttribute;

			return dacAttributes.Any(attribute => attribute.AttributeClass != null &&
												 (attribute.AttributeClass.InheritsFromOrEquals(pxCacheNameAttribute) ||
												  attribute.AttributeClass.InheritsFromOrEquals(pxPrimaryGraphAttribute)));
		}

		protected abstract void MakeSpecificDacKeysAnalysis(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac, 
															List<INamedTypeSymbol> allDacKeys, Dictionary<INamedTypeSymbol, List<ITypeSymbol>> dacFieldsByKey);

		protected virtual void ReportKeyDeclarationWithWrongName(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac,
																 INamedTypeSymbol keyDeclaration, RefIntegrityDacKeyType dacKeyType)
		{
			var keyDeclarationNode = keyDeclaration.GetSyntax(symbolContext.CancellationToken);
			Location location = (keyDeclarationNode as ClassDeclarationSyntax)?.Identifier.GetLocation() ?? keyDeclarationNode?.GetLocation();
			Location dacLocation = dac.Node.GetLocation();
			DiagnosticDescriptor px1036Descriptor = GetWrongKeyNameDiagnosticDescriptor(dacKeyType);

			if (location == null || dacLocation == null || px1036Descriptor == null)
				return;

			var additionalLocations = new[] { dacLocation };
			var diagnosticProperties = new Dictionary<string, string>
			{
				{ nameof(RefIntegrityDacKeyType),  dacKeyType.ToString() }
			};

			if (dacKeyType == RefIntegrityDacKeyType.UniqueKey)
			{
				diagnosticProperties.Add(nameof(UniqueKeyCodeFixType), UniqueKeyCodeFixType.SingleUniqueKey.ToString());
			}
			
			symbolContext.ReportDiagnosticWithSuppressionCheck(
										Diagnostic.Create(px1036Descriptor, location, additionalLocations, diagnosticProperties.ToImmutableDictionary()),
										context.CodeAnalysisSettings);
		}

		protected DiagnosticDescriptor GetWrongKeyNameDiagnosticDescriptor(RefIntegrityDacKeyType dacKeyType) =>
			dacKeyType switch
			{
				RefIntegrityDacKeyType.PrimaryKey => Descriptors.PX1036_WrongDacPrimaryKeyName,
				RefIntegrityDacKeyType.UniqueKey  => Descriptors.PX1036_WrongDacSingleUniqueKeyName,
				RefIntegrityDacKeyType.ForeignKey => Descriptors.PX1036_WrongDacForeignKeyDeclaration,
				_ => null
			};

		protected IEnumerable<Location> GetKeysLocations(IEnumerable<INamedTypeSymbol> keys, CancellationToken cancellationToken) =>
			keys.Select(key => key.GetSyntax(cancellationToken))
				.OfType<ClassDeclarationSyntax>()
				.Select(keyClassDeclaration => keyClassDeclaration.Identifier.GetLocation() ?? keyClassDeclaration.GetLocation())
				.Where(location => location != null);
	}
}