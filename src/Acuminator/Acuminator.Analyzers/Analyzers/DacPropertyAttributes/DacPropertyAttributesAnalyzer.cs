using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;


namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacPropertyAttributesAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, 
				Descriptors.PX1023_DacPropertyBothBoundAndUnbound
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeProperty(c, pxContext), SymbolKind.Property);
		}

        private static async void AnalyzeProperty(SymbolAnalysisContext symbolContext, PXContext pxContext)
        {
            IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;
            var parent = property?.ContainingType;

            if (parent == null || (!parent.ImplementsInterface(pxContext.IBqlTableType) && !parent.InheritsFrom(pxContext.PXCacheExtensionType)))
                return;

            ImmutableArray<AttributeData> attributes = property.GetAttributes();

            if (attributes.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
                return;

            AttributeData attributeWithError = attributes.FirstOrDefault(a => !CheckPropertyAttribute(property, a, symbolContext.CancellationToken));

            if (attributeWithError == null || symbolContext.CancellationToken.IsCancellationRequested)
                return;

            Location attributeLocation = await GetAttributeLocation(attributeWithError, symbolContext.CancellationToken);
            symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty,
                                           property.Locations.First()));

            if (attributeLocation != null)
            {
                symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation));
            }
        }

        private static bool CheckPropertyAttribute(IPropertySymbol property, AttributeData attribute, CancellationToken cancellationToken)
        {
            return true;
           // attribute.AttributeClass.
        }

        private static async Task<Location> GetAttributeLocation(AttributeData attribute, CancellationToken cancellationToken)
        {
            SyntaxNode attributeSyntaxNode = null;

            try
            {
                attributeSyntaxNode = await attribute.ApplicationSyntaxReference.GetSyntaxAsync(cancellationToken)
                                                                                .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception e)
            {
                //TODO log error here
                return null;
            }

            return attributeSyntaxNode?.GetLocation();
        }
	}
}