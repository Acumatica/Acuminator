using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Data;

namespace Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacExtensionDefaultAttributeAnalyzer : PXDiagnosticAnalyzer
	{
		private const string _PersistingCheck = "PersistingCheck";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1030_DefaultAttibuteToExisitingRecords
			);
		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(async symbolContext =>
				await AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.NamedType);
		}
		private static Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExt) || !dacOrDacExt.IsDacOrExtension(pxContext))
				return Task.FromResult(false);

			AttributeInformation attributeInformation = new AttributeInformation(pxContext);

			Task[] allTasks = dacOrDacExt.GetMembers()
				.OfType<IPropertySymbol>()
				.Select(property => CheckDacPropertyAsync(property, symbolContext, pxContext, attributeInformation))
				.ToArray();

			return Task.WhenAll(allTasks);
		}
        private static async Task CheckDacPropertyAsync(IPropertySymbol property, SymbolAnalysisContext symbolContext, PXContext pxContext,
                                                        AttributeInformation attributeInformation)
        {
            ImmutableArray<AttributeData> attributes = property.GetAttributes();

            if (attributes.Length == 0)
                return;

            symbolContext.CancellationToken.ThrowIfCancellationRequested();

            bool isBoundField = attributeInformation.ContainsBoundAttributes(attributes);

            if (isBoundField)
            {
                await AnalyzeAttributesWithinBoundFieldAsync(property, attributes, pxContext, symbolContext, attributeInformation).ConfigureAwait(false);
            }
            else if (!isBoundField)
            {
                await AnalyzeAttributesWithinUnBoundFieldAsync(property, attributes, pxContext, symbolContext, attributeInformation).ConfigureAwait(false);
            }

        }

		private static async Task AnalyzeAttributesWithinBoundFieldAsync(IPropertySymbol property, ImmutableArray<AttributeData> attributes,
																			PXContext pxContext,
																			SymbolAnalysisContext symbolContext,
																			AttributeInformation attributeInformation)
		{
			if (property.ContainingType.IsDAC()) // BQLTable class bound field
				return;

			foreach (var attribute in attributes)
			{

				if (attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxContext.AttributeTypes.PXDefaultAttribute))
				{
					foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
					{
						if (isAttributeContainsPersistingCheckNothing(argument))
							return;
					}

					Location attributeLocation = await GetAttributeLocationAsync(attribute, symbolContext.CancellationToken);

					if (attributeLocation != null)
					{
						var diagnosticProperties = new Dictionary<string, string>
						{
							{ DiagnosticProperty.IsBoundField,true.ToString() }
						}.ToImmutableDictionary();

						symbolContext.ReportDiagnostic(
							Diagnostic.Create(
								Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attributeLocation, diagnosticProperties));
					}
				}
			}
		}

		private static bool isAttributeContainsPersistingCheckNothing(KeyValuePair<string, TypedConstant> argument)
		{
			return argument.Key.Contains(_PersistingCheck) &&
					argument.Value.Value is int persistingCheckValue &&
					persistingCheckValue == (int)PXPersistingCheck.Nothing;
		}

		private static async Task AnalyzeAttributesWithinUnBoundFieldAsync(IPropertySymbol property, ImmutableArray<AttributeData> attributes,
			PXContext pxContext, SymbolAnalysisContext symbolContext, AttributeInformation attributeInformation)
		{
			foreach (AttributeData attribute in attributes)
			{

				if (attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxContext.AttributeTypes.PXDefaultAttribute) &&
					!attributeInformation.IsAttributeDerivedFromClass(attribute.AttributeClass, pxContext.AttributeTypes.PXUnboundDefaultAttribute))
				{
					foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
					{
						if (isAttributeContainsPersistingCheckNothing(argument))
							return;
					}
					Location attributeLocation = await GetAttributeLocationAsync(attribute, symbolContext.CancellationToken);

					if (attributeLocation != null)
					{
                        var diagnosticProperties = new Dictionary<string, string>
						{
							{ DiagnosticProperty.IsBoundField, false.ToString() }
						}.ToImmutableDictionary();

                        if (!property.ContainingType.IsDAC()) 
                        {
                            symbolContext.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attributeLocation, diagnosticProperties));
                        }
                        else // BQLTable class 
                        {
                            symbolContext.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.PX1030_DefaultAttibuteToExisitingRecordsOnDAC, attributeLocation, diagnosticProperties));
                        }

                    }
				}
			}
		}

		public static async Task<Location> GetAttributeLocationAsync(AttributeData attribute, CancellationToken cancellationToken)
		{
			SyntaxNode attributeSyntaxNode = await attribute.ApplicationSyntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);

			return attributeSyntaxNode?.GetLocation();
		}
	}
}
