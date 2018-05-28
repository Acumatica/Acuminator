using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

#pragma warning disable CS4014
		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzePropertyAsync(c, pxContext), SymbolKind.Property);
		}
#pragma warning restore CS4014

		private static async Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;

			if (!CheckProperty(property, pxContext))
				return;
		
			ImmutableArray<AttributeData> attributes = property.GetAttributes();

			if (attributes.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var attributesWithInfo = GetFieldAttributesInfos(pxContext, attributes, symbolContext.CancellationToken);

			if (attributesWithInfo.Count == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			if (attributesWithInfo.Count > 1)
			{
				var locationTasks = attributesWithInfo.Select(info => GetAttributeLocationAsync(info.Attribute, symbolContext.CancellationToken));
				Location[] attributeLocations = await Task.WhenAll(locationTasks);
					
				foreach (Location attrLocation in attributeLocations)
				{
					if (attrLocation == null)
						continue;

					symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1023_DacPropertyMultipleFieldAttributes, attrLocation));
				}
			}
			else 
			{
				var (fieldAttribute, fieldAttrInfo) = attributesWithInfo[0];

#pragma warning disable CS4014
				CheckAttributeAndPropertyTypesForCompatibilityAsync(fieldAttribute, fieldAttrInfo, pxContext, symbolContext);
#pragma warning restore CS4014 
			}		
		}

		private static bool CheckProperty(IPropertySymbol property, PXContext pxContext)
		{
			var parent = property?.ContainingType;

			if (parent == null || (!parent.ImplementsInterface(pxContext.IBqlTableType) && !parent.InheritsFrom(pxContext.PXCacheExtensionType)))
				return false;

			return property.Type.TypeKind == TypeKind.Struct || 
				   property.Type.TypeKind == TypeKind.Class;
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

		private static async Task CheckAttributeAndPropertyTypesForCompatibilityAsync(AttributeData fieldAttribute, 
																					  FieldAttributeDTO fieldAttributeInfo,
																					  PXContext pxContext, SymbolAnalysisContext symbolContext)
		{
			IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;
			ITypeSymbol typeToCompare = property.Type.IsValueType 
				? (property.Type as INamedTypeSymbol)?.GetUnderlyingTypeFromNullable(pxContext)
				: property.Type;

			if (fieldAttributeInfo.FieldType.Equals(typeToCompare))
				return;

			Location[] locations = await Task.WhenAll(GetPropertyTypeLocationAsync(property, symbolContext.CancellationToken),
													  GetAttributeLocationAsync(fieldAttribute, symbolContext.CancellationToken));

			Location propertyTypeLocation = locations[0];
			Location attributeLocation = locations[1];

			if (propertyTypeLocation != null)
			{
				symbolContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, propertyTypeLocation));
			}

			if (attributeLocation != null)
			{
				symbolContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation));
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

		private static async Task<Location> GetPropertyTypeLocationAsync(IPropertySymbol property, CancellationToken cancellationToken)
		{
			SyntaxNode propertySyntaxNode = null;
			var syntaxRefs = property.DeclaringSyntaxReferences;

			if (syntaxRefs.Length == 0)
				return property.Locations.FirstOrDefault();

			try
			{
				propertySyntaxNode = await syntaxRefs[0].GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
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
			
			return propertySyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax 
				? propertyDeclarationSyntax.Type.GetLocation()
				: property.Locations.FirstOrDefault();
		}
	}
}