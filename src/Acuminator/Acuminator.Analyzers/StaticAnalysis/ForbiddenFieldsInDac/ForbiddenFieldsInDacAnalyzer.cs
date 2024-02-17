#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac
{
	/// <summary>
	/// A DAC declaration syntax analyzer.
	/// </summary>
	public class ForbiddenFieldsInDacAnalyzer : DacAggregatedAnalyzerBase
	{
		private const string CompanyPrefix = "Company";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV,
				Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var forbiddenNames = DacFieldNames.Restricted.All;
			CheckDacPropertiesForForbiddenNames(dacOrDacExtension, pxContext, context, forbiddenNames);

			var allNestedTypes = dacOrDacExtension.GetMemberNodes<ClassDeclarationSyntax>()
												  .ToLookup(node => node.Identifier.ValueText, StringComparer.OrdinalIgnoreCase);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDacBqlFieldsForForbiddenNames(pxContext, context, forbiddenNames, allNestedTypes);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDacPropertiesForCompanyPrefix(dacOrDacExtension, pxContext, context);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDacBqlFieldsForCompanyPrefix(allNestedTypes, pxContext, context);
		}
		
		private void CheckDacPropertiesForForbiddenNames(DacSemanticModel dacOrDacExtension, PXContext pxContext, SymbolAnalysisContext context,
														 in ImmutableArray<string> forbiddenNames)
		{
			var invalidProperties = from forbiddenFieldName in forbiddenNames
									where dacOrDacExtension.PropertiesByNames.ContainsKey(forbiddenFieldName)
									select dacOrDacExtension.PropertiesByNames[forbiddenFieldName];

			foreach (DacPropertyInfo property in invalidProperties.Where(p => p.Symbol.ContainingSymbol == dacOrDacExtension.Symbol))
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				RegisterForbiddenFieldDiagnosticForIdentifier(property.Node.Identifier, pxContext, context);
			}
		}

		private void CheckDacBqlFieldsForForbiddenNames(PXContext pxContext, SymbolAnalysisContext context, in ImmutableArray<string> forbiddenNames,
														ILookup<string, ClassDeclarationSyntax> allNestedTypes)
		{
			var allInvalidFields = forbiddenNames.Where(forbiddenClassName		=> allNestedTypes.Contains(forbiddenClassName))
												 .SelectMany(forbiddenClassName => allNestedTypes[forbiddenClassName]);

			foreach (ClassDeclarationSyntax fieldNode in allInvalidFields)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				RegisterForbiddenFieldDiagnosticForIdentifier(fieldNode.Identifier, pxContext, context);
			}
		}

		private void RegisterForbiddenFieldDiagnosticForIdentifier(SyntaxToken identifier, PXContext pxContext, SymbolAnalysisContext context)
		{
			bool isDeletedDatabaseRecord = DacFieldNames.Restricted.DeletedDatabaseRecord.Equals(identifier.ValueText, StringComparison.OrdinalIgnoreCase);
			DiagnosticDescriptor descriptorToShow = 
				isDeletedDatabaseRecord && !pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
					? Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV
					: Descriptors.PX1027_ForbiddenFieldsInDacDeclaration;

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(descriptorToShow, identifier.GetLocation(), identifier.ValueText), 
				pxContext.CodeAnalysisSettings);
		}

		private void CheckDacPropertiesForCompanyPrefix(DacSemanticModel dacOrDacExtension, PXContext pxContext, SymbolAnalysisContext context)
		{
			var propertiesWithInvalidPrefix = 
				dacOrDacExtension.DeclaredDacProperties.Where(property => property.Name.StartsWith(CompanyPrefix, StringComparison.OrdinalIgnoreCase));

			foreach (DacPropertyInfo property in propertiesWithInvalidPrefix)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				RegisterCompanyPrefixDiagnosticForIdentifier(property.Node.Identifier, pxContext, context);
			}
		}

		private void CheckDacBqlFieldsForCompanyPrefix(ILookup<string, ClassDeclarationSyntax> allNestedTypes, PXContext pxContext, 
													   SymbolAnalysisContext context)
		{
			var fieldsWithInvalidPrefix = 
				allNestedTypes.Where(groupedByName => groupedByName.Key.StartsWith(CompanyPrefix, StringComparison.OrdinalIgnoreCase))
							  .SelectMany(groupedByName => groupedByName);

			foreach (ClassDeclarationSyntax fieldNode in fieldsWithInvalidPrefix)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				RegisterCompanyPrefixDiagnosticForIdentifier(fieldNode.Identifier, pxContext, context);
			}
		}

		private void RegisterCompanyPrefixDiagnosticForIdentifier(SyntaxToken identifier, PXContext pxContext, SymbolAnalysisContext context)
		{
			var diagnosticProperties = ImmutableDictionary<string, string>.Empty
																		  .Add(DiagnosticProperty.RegisterCodeFix, bool.FalseString);
			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(
					Descriptors.PX1027_ForbiddenCompanyPrefixInDacFieldName, identifier.GetLocation(), diagnosticProperties),
				pxContext.CodeAnalysisSettings);
		}
	}
}