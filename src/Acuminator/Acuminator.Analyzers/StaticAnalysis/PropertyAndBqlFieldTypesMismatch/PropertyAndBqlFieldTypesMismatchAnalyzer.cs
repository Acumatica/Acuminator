using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PropertyAndBqlFieldTypesMismatch;

public class PropertyAndBqlFieldTypesMismatchAnalyzer : DacAggregatedAnalyzerBase
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create
		(
			Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch
		);

	public override bool ShouldAnalyze(PXContext pxContext, [NotNullWhen(true)] DacSemanticModel dac) =>
		base.ShouldAnalyze(pxContext, dac) && dac.BqlFieldsByNames.Count > 0;

	public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac)
	{
		symbolContext.CancellationToken.ThrowIfCancellationRequested();

		foreach (var dacField in dac.DeclaredDacFields)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// Skip all DACs without BQL field or field property - it's impossible to compare their types
			// Also skip DAC fields without effective BQL field or field property types.
			// And skip weakly typed BQL fields - there is PX1060 diagnostic to check it
			if (!dacField.HasBqlFieldEffective || !dacField.HasFieldPropertyEffective ||
				dacField.PropertyTypeUnwrappedNullable == null || dacField.BqlFieldDataTypeEffective == null)
			{
				continue;
			}

			// Skip DAC fields with matching BQL field and property types
			if (SymbolEqualityComparer.Default.Equals(dacField.PropertyTypeUnwrappedNullable, dacField.BqlFieldDataTypeEffective))
				continue;

			// DAC field is declared in DAC, but does not have a BQL field redeclaration in DAC
			ReportPropertyAndBqlTypeMismatch(symbolContext, pxContext, dac, dacField);
		}
	}

	private void ReportPropertyAndBqlTypeMismatch(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dac,
												  DacFieldInfo declaredDacFieldWithMismatchingTypes)
	{
		Location? propertyTypeLocation = GetPropertyTypeLocation(declaredDacFieldWithMismatchingTypes);
		Location? bqlTypeLocation	   = GetBqlTypeLocation(declaredDacFieldWithMismatchingTypes);

		if (propertyTypeLocation == null || bqlTypeLocation == null)
			return;

		string propertyTypeName  = declaredDacFieldWithMismatchingTypes.PropertyTypeUnwrappedNullable!.GetSimplifiedName();
		string? bqlFieldTypeName = GetBqlFieldTypeName(declaredDacFieldWithMismatchingTypes);

		var sharedProperties = new Dictionary<string, string?>
		{
			{ DiagnosticProperty.PropertyType, propertyTypeName },
			{ DiagnosticProperty.BqlFieldType, bqlFieldTypeName },
		}
		.ToImmutableDictionary();

		ReportDiagnostic(isProperty: true, symbolContext, pxContext, propertyTypeLocation, bqlTypeLocation, sharedProperties);
		ReportDiagnostic(isProperty: false, symbolContext, pxContext, propertyTypeLocation, bqlTypeLocation, sharedProperties);
	}

	private void ReportDiagnostic(bool isProperty, SymbolAnalysisContext symbolContext, PXContext pxContext, Location propertyTypeLocation,
								  Location bqlTypeLocation, ImmutableDictionary<string, string?> sharedProperties)
	{
		var diagnosticProperties = sharedProperties.Add(DiagnosticProperty.IsProperty, isProperty.ToString());

		(Location locationToReport, Location[] extraLocations) = isProperty
			? (propertyTypeLocation, new[] { bqlTypeLocation })
			: (bqlTypeLocation,		 new[] { propertyTypeLocation });

		var diagnostic = Diagnostic.Create(
							Descriptors.PX1068_PropertyAndBqlFieldTypesMismatch, locationToReport, extraLocations, diagnosticProperties);
		symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
	}

	private Location? GetPropertyTypeLocation(DacFieldInfo declaredDacFieldWithMismatchingTypes)
	{
		if (declaredDacFieldWithMismatchingTypes?.PropertyInfo?.Node == null)
			return null;

		var propertyNode = declaredDacFieldWithMismatchingTypes.PropertyInfo.Node;
		return propertyNode.Type?.GetLocation() ?? propertyNode.Identifier.GetLocation() ?? propertyNode.GetLocation();
	}

	private Location? GetBqlTypeLocation(DacFieldInfo declaredDacFieldWithMismatchingTypes)
	{
		if (declaredDacFieldWithMismatchingTypes?.BqlFieldInfo?.Node == null)
			return null;

		var bqlFieldNode = declaredDacFieldWithMismatchingTypes.BqlFieldInfo.Node;

		if (bqlFieldNode.BaseList?.Types.Count is null or 0)
			return bqlFieldNode.Identifier.GetLocation() ?? bqlFieldNode.GetLocation();

		var baseTypeNode = bqlFieldNode.BaseList.Types[0];

		if (baseTypeNode.Type is not QualifiedNameSyntax bqlTypeNameNodeWithMultipleSegments)
			return baseTypeNode.Type.GetLocation() ?? bqlFieldNode.Identifier.GetLocation() ?? bqlFieldNode.GetLocation();

		if (bqlTypeNameNodeWithMultipleSegments.Left is not QualifiedNameSyntax bqlTypeNameSegments)
		{
			return bqlTypeNameNodeWithMultipleSegments.GetLocation() ?? baseTypeNode.Type.GetLocation() ??
				   bqlFieldNode.Identifier.GetLocation() ?? bqlFieldNode.GetLocation();
		}

		return bqlTypeNameSegments.Right.GetLocation() ?? bqlTypeNameNodeWithMultipleSegments.GetLocation() ??
			   baseTypeNode.Type.GetLocation() ?? bqlFieldNode.Identifier.GetLocation() ?? bqlFieldNode.GetLocation();
	}

	private string? GetBqlFieldTypeName(DacFieldInfo declaredDacFieldWithMismatchingTypes)
	{
		string bqlFieldDataTypeName = declaredDacFieldWithMismatchingTypes.BqlFieldDataTypeEffective!.GetSimplifiedName();

		var dataTypeStrongName	   = new DataTypeName(bqlFieldDataTypeName);
		string? mappedBqlFieldType = DataTypeToBqlFieldTypeMapping.GetBqlFieldType(dataTypeStrongName);

		return mappedBqlFieldType;
	}
}