using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacExtensionDefaultAttributeAnalyzer : PXDiagnosticAnalyzer
	{
		private const string _PersistingCheck = "PersistingCheck";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
                Descriptors.PX1030_DefaultAttibuteToExisitingRecordsError);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(symbolContext =>
                AnalyzeDacOrExtension(symbolContext, pxContext), SymbolKind.NamedType);
		}

		private static void AnalyzeDacOrExtension(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
            symbolContext.CancellationToken.ThrowIfCancellationRequested();

            if (!(symbolContext.Symbol is INamedTypeSymbol namedTypeSymbol) ||
                !namedTypeSymbol.IsDacOrExtension(pxContext))
            {
                return;
            }

            // Analyze only declared properties
            var properties = namedTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => namedTypeSymbol.Equals(p.ContainingType));

            foreach (var p in properties)
            {
                AnalyzeProperty(symbolContext, pxContext, namedTypeSymbol, p);
            }
		}

        private static void AnalyzeProperty(SymbolAnalysisContext symbolContext, PXContext pxContext,
            INamedTypeSymbol dacOrExtension, IPropertySymbol property)
        {
            symbolContext.CancellationToken.ThrowIfCancellationRequested();

            var attributeInformation = new AttributeInformation(pxContext);
            var attributes = property.GetAttributes();
            var boundType = GetPropertyBoundType(pxContext, dacOrExtension, property,
                attributeInformation, symbolContext.CancellationToken);

            switch (boundType)
            {
                case BoundType.Unbound:
                    AnalyzeUnboundProperty(symbolContext, pxContext, dacOrExtension, attributeInformation, attributes);
                    break;
                case BoundType.DbBound
                when dacOrExtension.IsDacExtension(pxContext): // Analyze bound property only for extensions
                    AnalyzeBoundPropertyAttributes(symbolContext, pxContext, dacOrExtension, attributeInformation, property.Name, attributes);
                    break;
                default:
                    break;
            }
        }

        private static void AnalyzeUnboundProperty(SymbolAnalysisContext symbolContext, PXContext pxContext,
            INamedTypeSymbol dacOrExtension, AttributeInformation attributeInformation, ImmutableArray<AttributeData> attributes)
        {
            var (pxDefaultAttribute, hasPersistingCheckNothing) =
                GetPXDefaultInfo(pxContext, attributeInformation, attributes, symbolContext.CancellationToken);
            if (pxDefaultAttribute == null || hasPersistingCheckNothing)
            {
                return;
            }

            var attributeLocation = GetAttributeLocation(pxDefaultAttribute, symbolContext.CancellationToken);
            if (attributeLocation == null)
            {
                return;
            }

            var diagnosticProperties = ImmutableDictionary<string, string>.Empty
                .Add(DiagnosticProperty.IsBoundField, bool.FalseString);
            var descriptor = dacOrExtension.IsDac(pxContext) ?
                Descriptors.PX1030_DefaultAttibuteToExisitingRecordsOnDAC :
                Descriptors.PX1030_DefaultAttibuteToExisitingRecordsError;
            var diagnostic = Diagnostic.Create(descriptor, attributeLocation, diagnosticProperties);

            symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic);
        }

        private static void AnalyzeBoundPropertyAttributes(SymbolAnalysisContext symbolContext, PXContext pxContext,
            INamedTypeSymbol dacExtension, AttributeInformation attributeInformation, string propertyName,
            ImmutableArray<AttributeData> attributes)
        {
            var pxDefaultAttribute = GetInvalidPXDefaultAttributeFromBoundProperty(
                pxContext, dacExtension, attributeInformation, propertyName, attributes, symbolContext.CancellationToken);
            if (pxDefaultAttribute == null)
            {
                return;
            }

            var attributeLocation = GetAttributeLocation(pxDefaultAttribute, symbolContext.CancellationToken);
            if (attributeLocation == null)
            {
                return;
            }

            var diagnosticProperties = ImmutableDictionary<string, string>.Empty
                .Add(DiagnosticProperty.IsBoundField, bool.TrueString);
            var diagnostic = Diagnostic.Create(
                Descriptors.PX1030_DefaultAttibuteToExisitingRecordsWarning,
                attributeLocation,
                diagnosticProperties);

            symbolContext.ReportDiagnosticWithSuppressionCheck(diagnostic);
        }

        private static AttributeData GetInvalidPXDefaultAttributeFromBoundProperty(PXContext pxContext, INamedTypeSymbol dacExtension,
            AttributeInformation attributeInformation, string propertyName, ImmutableArray<AttributeData> attributes,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (pxDefaultAttribute, hasPersistingCheckNothing) =
                GetPXDefaultInfo(pxContext, attributeInformation, attributes, cancellationToken);
            if (pxDefaultAttribute == null || hasPersistingCheckNothing)
            {
                return null;
            }

            
            var baseExtensionAndDacTypes = dacExtension.GetDacExtensionsWithDac(pxContext).Skip(1);

            // If Dac extension contains PXDefaultAttribute without PersistingCheck.Nothing
            // we need to look to attribute wich it overrides:
            // if base attribute is also doesn't contain PersistingCheck.Nothing it is legitimately
            foreach (var darOrExtension in baseExtensionAndDacTypes)
            {
                var declaredProperty = GetDeclaredPropertyFromTypeByName(darOrExtension, propertyName);
                if (declaredProperty == null)
                {
                    continue;
                }

                var attributesBase = declaredProperty.GetAttributes();
                var (pxDefaultAttributeBase, hasPersistingCheckNothingBase) =
                    GetPXDefaultInfo(pxContext, attributeInformation, attributesBase, cancellationToken);

                if (pxDefaultAttributeBase == null)
                {
                    continue;
                }

                return hasPersistingCheckNothingBase ?
                    pxDefaultAttribute :
                    null;
            }

            return pxDefaultAttribute;
        }

        private static (AttributeData PXDefaultAttribute, bool HasPersistingCheckNothing) GetPXDefaultInfo (
            PXContext pxContext, AttributeInformation attributeInformation,
            ImmutableArray<AttributeData> attributes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pxDefaultAttribute = GetPXDefaultAttribute(pxContext, attributeInformation, attributes, cancellationToken);
            if (pxDefaultAttribute == null)
            {
                return (null, false);
            }

            var hasPersistingCheckNothing = pxDefaultAttribute.NamedArguments
                .Where(na => nameof(PXDefaultAttribute.PersistingCheck).Equals(na.Key, StringComparison.Ordinal))
                .Select(na => na.Value.Value)
                .Where(v => v is int persistingCheck && persistingCheck == (int)PXPersistingCheck.Nothing)
                .Any();

            return (pxDefaultAttribute, hasPersistingCheckNothing);
        }

        private static AttributeData GetPXDefaultAttribute(PXContext pxContext, AttributeInformation attributeInformation,
            ImmutableArray<AttributeData> attributes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return attributes
                .Where(a => attributeInformation.IsAttributeDerivedFromClass(a.AttributeClass, pxContext.AttributeTypes.PXDefaultAttribute) &&
                !attributeInformation.IsAttributeDerivedFromClass(a.AttributeClass, pxContext.AttributeTypes.PXUnboundDefaultAttribute))
                .FirstOrDefault();
        }

        private static BoundType GetPropertyBoundType(PXContext pxContext, INamedTypeSymbol dacOrExtension,
            IPropertySymbol property, AttributeInformation attributeInformation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (dacOrExtension.IsDac(pxContext))
            {
                return GetBoundTypeFromDeclaredProperty(property, attributeInformation);
            }

            var extensionAndDacTypes = dacOrExtension.GetDacExtensionsWithDac(pxContext);

            // We need to analyze base Dac extensions and Dac to find a first occurrence of bound/unbound attribute
            // because extension can be PXNonInstantiatedExtension
            foreach (var extension in extensionAndDacTypes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var declaredProperty = GetDeclaredPropertyFromTypeByName(extension, property.Name);
                var boundType = GetBoundTypeFromDeclaredProperty(declaredProperty, attributeInformation);

                if (boundType == BoundType.DbBound || boundType == BoundType.Unbound)
                {
                    return boundType;
                }
            }

            return BoundType.NotDefined;
        }

        private static BoundType GetBoundTypeFromDeclaredProperty(IPropertySymbol prop, AttributeInformation attributeInformation)
        {
            var propertyAttributes = prop.GetAttributes();

            if (attributeInformation.ContainsBoundAttributes(propertyAttributes))
            {
                return BoundType.DbBound;
            }

            if (attributeInformation.ContainsUnboundAttributes(propertyAttributes))
            {
                return BoundType.Unbound;
            }

            return BoundType.NotDefined;
        }

        private static IPropertySymbol GetDeclaredPropertyFromTypeByName(ITypeSymbol typeSymbol, string name)
        {
            return typeSymbol.GetMembers(name)
                .OfType<IPropertySymbol>()
                .Where(p => typeSymbol.Equals(p.ContainingType))
                .FirstOrDefault();
        }

        public static Location GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var attributeSyntaxNode = attribute.ApplicationSyntaxReference.GetSyntax(cancellationToken);

            return attributeSyntaxNode?.GetLocation();
        }
	}
}
