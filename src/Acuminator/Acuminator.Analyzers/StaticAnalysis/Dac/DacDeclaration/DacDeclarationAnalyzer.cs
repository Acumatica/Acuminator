﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacDeclaration
{
	/// <summary>
	/// A DAC declaration syntax analyzer.
	/// </summary>
	public class DacDeclarationAnalyzer : DacAggregatedAnalyzerBase
	{
		private const string DeletedDatabaseRecord = "DeletedDatabaseRecord";
		private const string CompanyID = "CompanyID";
		private const string CompanyMask = "CompanyMask";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV,
				Descriptors.PX1028_ConstructorInDacDeclaration
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			CheckDeclarationForForbiddenNames(pxContext, dacOrDacExtNode, context, dacProperties, dacClassDeclarations);
			CheckDeclarationForConstructors(pxContext, dacOrDacExtNode, context);
		}

		private static void AnalyzeDacOrDacExtensionDeclaration(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			
		}

		

		private static void CheckDeclarationForForbiddenNames(PXContext pxContext,
			ClassDeclarationSyntax dacOrDacExtNode,
			SyntaxNodeAnalysisContext syntaxContext,
			Dictionary<string, List<PropertyDeclarationSyntax>> dacProperties,
			Dictionary<string, List<ClassDeclarationSyntax>> dacClassDeclarations)
		{
			string[] forbiddenNames = GetForbiddenFieldsNames();

			var invalidPropertiesByName = from forbiddenFieldName in forbiddenNames
				where dacProperties.ContainsKey(forbiddenFieldName)
				select dacProperties[forbiddenFieldName];

			var invalidClassesByName = from forbiddenClassName in forbiddenNames
				where dacClassDeclarations.ContainsKey(forbiddenClassName)
				select dacClassDeclarations[forbiddenClassName];

			bool isDeletedDatabaseRecord;
			DiagnosticDescriptor showedDescriptor;

			foreach (var listProperties in invalidPropertiesByName)
			{
				foreach (var iProperty in listProperties)
				{
					isDeletedDatabaseRecord = string.Equals(iProperty.Identifier.Text,
						DeletedDatabaseRecord, StringComparison.OrdinalIgnoreCase);

					showedDescriptor = (isDeletedDatabaseRecord && 
					                    pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled == false )
									   ? Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV
									   : Descriptors.PX1027_ForbiddenFieldsInDacDeclaration;

					syntaxContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(
							showedDescriptor,
							iProperty.Identifier.GetLocation(),
							iProperty.Identifier.Text), pxContext.CodeAnalysisSettings);
				}
			}

			foreach (var listClasses in invalidClassesByName)
			{
				foreach (var iClass in listClasses)
				{
					isDeletedDatabaseRecord = string.Equals(iClass.Identifier.Text,
						DeletedDatabaseRecord, StringComparison.OrdinalIgnoreCase);

					showedDescriptor = (isDeletedDatabaseRecord &&
					                    pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled == false)
										? Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV
										: Descriptors.PX1027_ForbiddenFieldsInDacDeclaration;

					syntaxContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(
							showedDescriptor,
							iClass.Identifier.GetLocation(),
							iClass.Identifier.Text), pxContext.CodeAnalysisSettings);
				}
			}
		}

		private static void CheckDeclarationForConstructors(PXContext pxContext, ClassDeclarationSyntax dacOrDacExtNode,
															SyntaxNodeAnalysisContext syntaxContext)
		{
			var dacConstructors = dacOrDacExtNode.Members.OfType<ConstructorDeclarationSyntax>();

			foreach (var constructor in dacConstructors)
			{
				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1028_ConstructorInDacDeclaration, constructor.Identifier.GetLocation()),
					pxContext.CodeAnalysisSettings);
			}
		}


		public static string[] GetForbiddenFieldsNames()
		{
			return new string[]
			{
				DeletedDatabaseRecord,
				CompanyID,
				CompanyMask
			};
		}	
	}
}