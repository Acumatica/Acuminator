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
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac
{
	/// <summary>
	/// A DAC declaration syntax analyzer.
	/// </summary>
	public class ForbiddenFieldsInDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var forbiddenNames = DacFieldNames.Restricted.All;
			CheckDacPropertiesForForbiddenNames(dacOrDacExtension, pxContext, context, forbiddenNames);

			context.CancellationToken.ThrowIfCancellationRequested();
			CheckDacBqlFieldsForForbiddenNames(dacOrDacExtension, pxContext, context, forbiddenNames);


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
				RegisterDiagnosticForIdentifier(property.Node.Identifier, pxContext, context);
			}
		}

		private void CheckDacBqlFieldsForForbiddenNames(DacSemanticModel dacOrDacExtension, PXContext pxContext, SymbolAnalysisContext context,
														in ImmutableArray<string> forbiddenNames)
		{
			var allNestedTypesDictionary = dacOrDacExtension.GetMemberNodes<ClassDeclarationSyntax>()
															.ToLookup(node => node.Identifier.ValueText, StringComparer.OrdinalIgnoreCase);
			var allInvalidFields = forbiddenNames.Where(forbiddenClassName => allNestedTypesDictionary.Contains(forbiddenClassName))
												 .SelectMany(forbiddenClassName => allNestedTypesDictionary[forbiddenClassName]);

			foreach (ClassDeclarationSyntax fieldNode in allInvalidFields)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				RegisterDiagnosticForIdentifier(fieldNode.Identifier, pxContext, context);
			}
		}

		private void RegisterDiagnosticForIdentifier(SyntaxToken identifier, PXContext pxContext, SymbolAnalysisContext context)
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
	}
}