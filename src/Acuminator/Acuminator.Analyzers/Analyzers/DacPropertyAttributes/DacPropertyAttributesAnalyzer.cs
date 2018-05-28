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
				Descriptors.PX1023_DacPropertyMultipleFieldAttributes
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzePropertyAsync(c, pxContext), SymbolKind.Property);
		}

		private static async Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;
			var parent = property?.ContainingType;

			if (parent == null || (!parent.ImplementsInterface(pxContext.IBqlTableType) && !parent.InheritsFrom(pxContext.PXCacheExtensionType)))
				return;

			ImmutableArray<AttributeData> attributes = property.GetAttributes();

			if (attributes.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var attributesWithInfo = GetFieldAttributesInfos(pxContext, attributes, symbolContext.CancellationToken);

			if (attributesWithInfo.Count == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			if (attributesWithInfo.Count > 1)
			{
				SetMultipleFieldAttributesDiagnosticAsync(pxContext, symbolContext, attributesWithInfo.Select(info => info.Attribute));
			}
			else if (!attributesWithInfo[0].Info.FieldType.Equals(property.Type))
			{
				SetFieldTypeMismatchDiagnosticAsync(pxContext, symbolContext, attributesWithInfo[0].Attribute);
			}		
		}

		private static List<(AttributeData Attribute, FieldAttributeDTO Info)> GetFieldAttributesInfos(PXContext pxContext, 
																									   ImmutableArray<AttributeData> attributes,
																									   CancellationToken cancellationToken)
		{
			FieldAttributesInfo fieldAttributesInfo = new FieldAttributesInfo(pxContext);

			if (cancellationToken.IsCancellationRequested)
				return null;

			var fieldInfosList = new List<(AttributeData, FieldAttributeDTO)>(capacity: attributes.Length);

			foreach (AttributeData attribute in attributes)
			{
				if (cancellationToken.IsCancellationRequested)
					return null;
				
				FieldAttributeDTO attrInfo = fieldAttributesInfo.GetFieldAttributeInfo(attribute.AttributeClass);

				if (attrInfo.IsFieldAttribute)
					fieldInfosList.Add((attribute, attrInfo));
			}

			return fieldInfosList;
		}

		private static async Task SetMultipleFieldAttributesDiagnosticAsync(PXContext pxContext, SymbolAnalysisContext symbolContext,
																			IEnumerable<AttributeData> fieldAttributes)
		{
			IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;
			symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1023_DacPropertyMultipleFieldAttributes, property.Locations.First()));
			Location[] attributeLocations = await Task.WhenAll(fieldAttributes.Select(a => GetAttributeLocationAsync(a, symbolContext.CancellationToken)));

			foreach (Location attrLocation in attributeLocations)
			{
				if (attrLocation == null)
					continue;
			
				symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1023_DacPropertyMultipleFieldAttributes, attrLocation));			
			}
		}

		private static async Task SetFieldTypeMismatchDiagnosticAsync(PXContext pxContext, SymbolAnalysisContext symbolContext, AttributeData attribute)
		{
			IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;
			symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, property.Locations.First()));

			Location attributeLocation = await GetAttributeLocationAsync(attribute, symbolContext.CancellationToken);
			
			if (attributeLocation != null)
			{
				symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation));
			}
		}

		private static async Task<Location> GetAttributeLocationAsync(AttributeData attribute, CancellationToken cancellationToken)
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