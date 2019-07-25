using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using System.Collections.Generic;

namespace Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute
{

	public enum PXPersistingCheckValues
	{
		Null = 0,
		NullOrBlank = 1,
		Nothing = 2
	}

	public class DacExtensionDefaultAttributeAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
                Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC,
                Descriptors.PX1030_DefaultAttibuteToExistingRecordsError,
                Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			bool isDacFullyUnbound = IsDacFullyUnbound(dacOrExtension);

			foreach (DacPropertyInfo property in dacOrExtension.DeclaredDacProperties)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				AnalyzeProperty(context, pxContext, dacOrExtension, property, isDacFullyUnbound);
			}
		}

		private static bool IsDacFullyUnbound(DacSemanticModel dacOrExtension)
		{
			if (dacOrExtension.DacType != DacType.Dac)
				return false;

			return dacOrExtension.DacProperties.All(p => p.BoundType != BoundType.DbBound && p.BoundType != BoundType.Unknown);
		}

        private static void AnalyzeProperty(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrExtension,
											DacPropertyInfo property, bool isDacFullyUnbound)
        {         
            var boundType = GetPropertyBoundType(dacOrExtension, property);

            switch (boundType)
            {
                case BoundType.Unbound when !isDacFullyUnbound:
                    AnalyzeUnboundProperty(symbolContext, pxContext, dacOrExtension, property);
                    return;
                case BoundType.DbBound
                when dacOrExtension.DacType == DacType.DacExtension: // Analyze bound property only for extensions
                    AnalyzeBoundPropertyAttributes(symbolContext, pxContext, property);
					return;
            }
        }

        private static void AnalyzeUnboundProperty(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrExtension,
												   DacPropertyInfo property)
        {
            var (pxDefaultAttribute, hasPersistingCheckNothing) = GetPXDefaultInfo(pxContext, property);

            if (pxDefaultAttribute == null || hasPersistingCheckNothing)
                return;

            var attributeLocation = GetAttributeLocation(pxDefaultAttribute, symbolContext.CancellationToken);
            if (attributeLocation == null)
            {
                return;
            }

            var diagnosticProperties = ImmutableDictionary<string, string>.Empty
																		  .Add(DiagnosticProperty.IsBoundField, bool.FalseString);
            var descriptor = dacOrExtension.DacType == DacType.Dac
				? Descriptors.PX1030_DefaultAttibuteToExistingRecordsOnDAC 
				: Descriptors.PX1030_DefaultAttibuteToExistingRecordsError;
            var diagnostic = Diagnostic.Create(descriptor, attributeLocation, diagnosticProperties);

            symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
        }

        private static void AnalyzeBoundPropertyAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext, DacPropertyInfo property)
        {
            var pxDefaultAttribute = GetInvalidPXDefaultAttributeFromBoundProperty(pxContext, property);

            if (pxDefaultAttribute == null)
                return;

            var attributeLocation = GetAttributeLocation(pxDefaultAttribute, symbolContext.CancellationToken);
            if (attributeLocation == null)
                return;

            var diagnosticProperties = ImmutableDictionary<string, string>.Empty
																		  .Add(DiagnosticProperty.IsBoundField, bool.TrueString);
            var diagnostic = Diagnostic.Create(Descriptors.PX1030_DefaultAttibuteToExistingRecordsWarning, attributeLocation, diagnosticProperties);

            symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
        }

        private static AttributeData GetInvalidPXDefaultAttributeFromBoundProperty(PXContext pxContext, DacPropertyInfo property)
        {
            var (pxDefaultAttribute, hasPersistingCheckNothing) = GetPXDefaultInfo(pxContext, property);

            if (pxDefaultAttribute == null || hasPersistingCheckNothing)
                return null;

            // If Dac extension contains PXDefaultAttribute without PersistingCheck.Nothing
            // we need to look to attribute wich it overrides:
            // if base attribute is also doesn't contain PersistingCheck.Nothing it is legitimately
            foreach (DacPropertyInfo overridenProperty in property.JustOverridenItems())
            {
                var (pxDefaultAttributeBase, hasPersistingCheckNothingBase) = GetPXDefaultInfo(pxContext, overridenProperty);

                if (pxDefaultAttributeBase == null)
                    continue;      

                return hasPersistingCheckNothingBase	//The values from the latest appropriate override should be used
					? pxDefaultAttribute
					: null;
            }

            return pxDefaultAttribute;
        }

        private static (AttributeData PXDefaultAttribute, bool HasPersistingCheckNothing) GetPXDefaultInfo(PXContext pxContext, DacPropertyInfo property)
        {
			var defaultAttributes = property.Attributes.Where(a => a.IsDefaultAttribute).ToList();

			if (defaultAttributes.Count != 1)	//We don't check in case of multiple pxdefault
				return (null, false);

			var pxDefaultAttribute = defaultAttributes[0].AttributeData;
			var hasPersistingCheckNothing = (from arg in pxDefaultAttribute.NamedArguments
											 where TypeNames.PersistingCheck.Equals(arg.Key, StringComparison.Ordinal)
											 select arg.Value.Value)
											.Any(value => value is int persistingCheck && 
														  persistingCheck == (int)PXPersistingCheckValues.Nothing);

            return (pxDefaultAttribute, hasPersistingCheckNothing);
        }

        private static BoundType GetPropertyBoundType(DacSemanticModel dacOrExtension, DacPropertyInfo property)
        {
            if (dacOrExtension.DacType == DacType.Dac)
                return property.BoundType;
            
            // We need to analyze base Dac extensions and Dac to find a first occurrence of bound/unbound attribute
            // because extension can be PXNonInstantiatedExtension
            foreach (DacPropertyInfo propertyInOverridesChain in property.ThisAndOverridenItems())
            {
				switch (propertyInOverridesChain.BoundType)
				{		
					case BoundType.Unbound:
					case BoundType.DbBound:
					case BoundType.Unknown:
						return propertyInOverridesChain.BoundType;				
				}
            }

            return property.BoundType;
        }

        public static Location GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken) =>
			attribute.ApplicationSyntaxReference
					?.GetSyntax(cancellationToken)
					?.GetLocation();
	}
}