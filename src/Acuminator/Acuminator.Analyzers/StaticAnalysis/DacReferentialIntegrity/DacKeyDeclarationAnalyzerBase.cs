using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	/// <summary>
	/// Base class for DAC key declaration analyzers which provides filtering of the DACs for the diagnostics.
	/// </summary>
	public abstract class DacKeyDeclarationAnalyzerBase : DacAggregatedAnalyzerBase
	{
		public override bool ShouldAnalyze(PXContext context, DacSemanticModel dac) =>
			base.ShouldAnalyze(context, dac) &&
			dac.DacType == DacType.Dac && !dac.IsMappedCacheExtension && !dac.Symbol.IsAbstract &&
			IsKeySymbolDefined(context) && ShouldAnalyzeDac(context, dac);

		protected abstract bool IsKeySymbolDefined(PXContext context);

		protected virtual bool ShouldAnalyzeDac(PXContext context, DacSemanticModel dac)
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
				RefIntegrityDacKeyType.ForeignKey => Descriptors.PX1036_WrongDacForeignKeyName,
				_ => null
			};

		protected string GetHashForSetOfDacFields(IEnumerable<ITypeSymbol> dacFields) =>
			dacFields.Select(dacField => dacField.MetadataName)
					 .OrderBy(metadataName => metadataName)
					 .Join(separator: ",");
	}
}