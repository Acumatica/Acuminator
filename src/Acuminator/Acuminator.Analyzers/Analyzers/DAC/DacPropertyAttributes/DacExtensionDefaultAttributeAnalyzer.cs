using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using System.Collections.Immutable;
using PX.Data;
using System.Threading;

namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	class DacExtensionDefaultAttributeAnalyzer : PXDiagnosticAnalyzer
	{

		private const string _PersistingCheck = "PersistingCheck";
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1030_DefaultAttibuteToExisitingRecords
			);
#pragma warning disable CS4014
		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			/*compilationStartContext.RegisterSymbolAction(symbolContext =>
				AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.Property);*/
			compilationStartContext.RegisterSymbolAction(symbolContext =>
				AnalyzePropertyAsync(symbolContext, pxContext), SymbolKind.NamedType);
		}
#pragma warning restore CS4014
		private static Task AnalyzePropertyAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			//IPropertySymbol property = symbolContext.Symbol as IPropertySymbol;

			if (!(symbolContext.Symbol is INamedTypeSymbol dacOrDacExt) || !dacOrDacExt.IsDacOrExtension(pxContext))
				return Task.FromResult(false);

			AttributeInformation attributeInformation = new AttributeInformation(pxContext);

			Task[] allTasks = dacOrDacExt.GetMembers()
				.OfType<IPropertySymbol>()
				.Select(property => CheckDacPropertyAsync(property, symbolContext,pxContext,attributeInformation))
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
			
			bool isBoundField = attributeInformation.ContainsBoundAttributes(attributes.Select(a => a.AttributeClass));

			if (isBoundField)
			{
				if (IsIBqlTableTypeImplementation(property)) // BQLTable class bound field
					return;

				//check 
				foreach (var attribute in attributes)
				{
					var typesHierarchy = attribute.AttributeClass.GetBaseTypesAndThis();
					if(typesHierarchy.Contains(pxContext.AttributeTypes.PXDefaultAttribute))
					{
						foreach (var argument in attribute.NamedArguments)
						{
							if (argument.Key.Contains(_PersistingCheck) && (int)argument.Value.Value == (int)PXPersistingCheck.Nothing)
								return;
						}
						Location[] locations = await Task.WhenAll(GetAttributeLocationAsync(attribute, symbolContext.CancellationToken));
						Location attributeLocation = locations[0];

						if (attributeLocation != null)
						{
							var diagnosticProperties = new Dictionary<string, string>{
															{ DiagnosticProperty.IsBoundField,
															  isBoundField.ToString() }}.ToImmutableDictionary();

							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attributeLocation, diagnosticProperties));
						}
					}
				}
			}
			else
			{
				foreach (var attribute in attributes)
				{
					var typesHierarchy = attribute.AttributeClass.GetBaseTypesAndThis();
					if(typesHierarchy.Contains(pxContext.AttributeTypes.PXDefaultAttribute) &&
						!typesHierarchy.Contains(pxContext.AttributeTypes.PXUnboundDefaultAttribute))
					{
						Location[] locations = await Task.WhenAll(GetAttributeLocationAsync(attribute, symbolContext.CancellationToken));
						Location attributeLocation = locations[0];

						if (attributeLocation != null)
						{
							var diagnosticProperties = new Dictionary<string, string>
						{
							{ DiagnosticProperty.IsBoundField, isBoundField.ToString() }
						}.ToImmutableDictionary();

							symbolContext.ReportDiagnostic(
								Diagnostic.Create(
									Descriptors.PX1030_DefaultAttibuteToExisitingRecords, attributeLocation, diagnosticProperties));
						}
					}

				}
			}
			return;
		}

	/*	private static bool CheckProperty(IPropertySymbol property, PXContext pxContext)
		{
			var parent = property?.ContainingType;

			if (parent == null || (!parent.ImplementsInterface(pxContext.IBqlTableType) && !parent.InheritsFrom(pxContext.PXCacheExtensionType)))
				return false;
			return property.Type.TypeKind == TypeKind.Struct ||
				   property.Type.TypeKind == TypeKind.Class ||
				   property.Type.TypeKind == TypeKind.Array;
		}*/

		private static bool IsIBqlTableTypeImplementation(IPropertySymbol property)
		{
			var parent = property?.ContainingType;

			if (parent == null || !parent.IsDAC())
				return false;
			return true;
		}


		public static async Task<Location> GetAttributeLocationAsync(AttributeData attribute, CancellationToken cancellationToken)
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
