#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

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

			bool isDacFullyUnbound = dacOrExtension.IsFullyUnbound();

            if (isDacFullyUnbound)
                return;

			foreach (DacPropertyInfo property in dacOrExtension.DeclaredDacProperties)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				AnalyzeProperty(context, pxContext, dacOrExtension, property);
			}
		}

		private static void AnalyzeProperty(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrExtension,
											DacPropertyInfo property)
        {         
            switch (property.EffectiveBoundType)
            {
                case DbBoundnessType.Unbound:
                    AnalyzeUnboundProperty(symbolContext, pxContext, dacOrExtension, property);
                    return;

				case DbBoundnessType.DbBound:
				case DbBoundnessType.PXDBCalced:
				case DbBoundnessType.PXDBScalar:

					if (dacOrExtension.DacType == DacType.DacExtension)
					{
						AnalyzeBoundPropertyAttributes(symbolContext, pxContext, property);         // Analyze bound property only for DAC extensions)
					}
					return;
            }
        }

        private static void AnalyzeUnboundProperty(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacOrExtension,
												   DacPropertyInfo property)
        {
            var (pxDefaultAttribute, hasPersistingCheckNothing) = GetPXDefaultInfo(property);

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
            var pxDefaultAttribute = GetInvalidPXDefaultAttributeFromBoundProperty(property);

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

        private static AttributeData? GetInvalidPXDefaultAttributeFromBoundProperty(DacPropertyInfo property)
        {
            var (pxDefaultAttribute, hasPersistingCheckNothing) = GetPXDefaultInfo(property);

            if (pxDefaultAttribute == null || hasPersistingCheckNothing)
                return null;

            // If Dac extension contains PXDefaultAttribute without PersistingCheck.Nothing
            // we need to look to attribute wich it overrides:
            // if base attribute is also doesn't contain PersistingCheck.Nothing it is legitimately
            foreach (DacPropertyInfo overridenProperty in property.JustOverridenItems())
            {
                var (pxDefaultAttributeBase, hasPersistingCheckNothingBase) = GetPXDefaultInfo(overridenProperty);

                if (pxDefaultAttributeBase == null)
                    continue;      

                return hasPersistingCheckNothingBase	//The values from the latest appropriate override should be used
					? pxDefaultAttribute
					: null;
            }

            return pxDefaultAttribute;
        }

        private static (AttributeData? PXDefaultAttribute, bool HasPersistingCheckNothing) GetPXDefaultInfo(DacPropertyInfo property)
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

        public static Location? GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken) =>
			attribute.ApplicationSyntaxReference
					?.GetSyntax(cancellationToken)
					?.GetLocation();
	}
}