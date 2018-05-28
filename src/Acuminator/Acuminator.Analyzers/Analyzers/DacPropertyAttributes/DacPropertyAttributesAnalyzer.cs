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

			var attributesWithInfo = GetFieldAttributesInfos(pxContext, attributes, symbolContext.CancellationToken);

			if (attributesWithInfo.Count == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			ProcessFieldAttributesInfo(pxContext, attributesWithInfo, symbolContext);
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

		private static void ProcessFieldAttributesInfo(PXContext pxContext, SymbolAnalysisContext symbolContext,
													   List<(AttributeData Attribute, FieldAttributeDTO Info)> attributesWithInfo)
		{
			bool multipleFieldAttribute = attributesWithInfo.Count > 1;
			IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;

			foreach (var (attribute, info) in attributesWithInfo)
			{
				bool typesCompatible = info.FieldType.Equals(property.Type);


				Location attributeLocation = await GetAttributeLocation(attributeWithError, symbolContext.CancellationToken);
				symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty,
											   property.Locations.First()));

				if (attributeLocation != null)
				{
					symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty, attributeLocation));
				}
			}
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