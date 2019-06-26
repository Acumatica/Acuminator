using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacDeclaration
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacDeclarationAnalyzer : PXDiagnosticAnalyzer, ISymbolAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1026_UnderscoresInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV,
				Descriptors.PX1028_ConstructorInDacDeclaration
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext =>
				AnalyzeDacOrDacExtensionDeclaration(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeDacOrDacExtensionDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (!(syntaxContext.Node is ClassDeclarationSyntax dacOrDacExtNode) || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			INamedTypeSymbol dacOrDacExt = syntaxContext.SemanticModel.GetDeclaredSymbol(dacOrDacExtNode, syntaxContext.CancellationToken);

			if (dacOrDacExt == null || (!dacOrDacExt.IsDAC() && !dacOrDacExt.IsDacExtension()) ||
				syntaxContext.CancellationToken.IsCancellationRequested)
				return;


			var dacProperties = dacOrDacExtNode.Members.OfType<PropertyDeclarationSyntax>()
													   .GroupBy(p => p.Identifier.ValueText, StringComparer.OrdinalIgnoreCase)
													   .ToDictionary(group => group.Key,
																	 group => group.ToList(), StringComparer.OrdinalIgnoreCase);
			var dacClassDeclarations = dacOrDacExtNode.Members.OfType<ClassDeclarationSyntax>()
				.GroupBy(p => p.Identifier.ValueText, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(group => group.Key,
								group => group.ToList(), StringComparer.OrdinalIgnoreCase);

			CheckDeclarationForUnderscores(pxContext, dacOrDacExtNode, syntaxContext, dacProperties);
			CheckDeclarationForForbiddenNames(pxContext, dacOrDacExtNode, syntaxContext, dacProperties,dacClassDeclarations);
			CheckDeclarationForConstructors(pxContext, dacOrDacExtNode, syntaxContext);
		}

		private static void CheckDeclarationForUnderscores(PXContext pxContext, ClassDeclarationSyntax dacOrDacExtNode,
														   SyntaxNodeAnalysisContext syntaxContext,
														   Dictionary<string, List<PropertyDeclarationSyntax>> dacProperties)
		{
			SyntaxToken identifier = dacOrDacExtNode.Identifier;

			if (identifier.ValueText.Contains("_"))
			{
				bool registerCodeFix = !IdentifierContainsOnlyUnderscores(identifier.ValueText);
				var diagnosticProperties = new Dictionary<string, string>
				{
					{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
				}.ToImmutableDictionary();

				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1026_UnderscoresInDacDeclaration, identifier.GetLocation(), diagnosticProperties),
					pxContext.CodeAnalysisSettings);
			}

			var identifiersWithUnderscores = from member in dacOrDacExtNode.Members
											 where ShouldCheckIdentifier(member, dacProperties)
											 from memberIdentifier in member.GetIdentifiers()
											 where memberIdentifier.ValueText.Contains("_")
											 select memberIdentifier;

			foreach (SyntaxToken identifierToReport in identifiersWithUnderscores)
			{
				bool registerCodeFix = !IdentifierContainsOnlyUnderscores(identifierToReport.ValueText);
				var diagnosticProperties = new Dictionary<string, string>
				{
					{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
				}.ToImmutableDictionary();

				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1026_UnderscoresInDacDeclaration, identifierToReport.GetLocation(), diagnosticProperties),
					pxContext.CodeAnalysisSettings);
			}

			//*************************************Local Functions**********************************************************************
			bool IdentifierContainsOnlyUnderscores(string identifierName)
			{
				for (int i = 0; i < identifierName.Length; i++)
				{
					if (identifierName[i] != '_')
						return false;
				}

				return true;
			}
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

			foreach (var listProperties in invalidPropertiesByName)
			{
				foreach (var iProperty in listProperties)
				{
					syntaxContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(
							(pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled &&
							 iProperty.Identifier.Text == _DeletedDatabaseRecord)
								? Descriptors.PX1027_ForbiddenFieldsInDacDeclaration
								: Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV,
							iProperty.Identifier.GetLocation(),
							iProperty.Identifier.Text), pxContext.CodeAnalysisSettings);
				}
			}

			foreach (var listClasses in invalidClassesByName)
			{
				foreach (var iClass in listClasses)
				{
					syntaxContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(
							(pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled &&
							 iClass.Identifier.Text == _DeletedDatabaseRecord)
								? Descriptors.PX1027_ForbiddenFieldsInDacDeclaration
								: Descriptors.PX1027_ForbiddenFieldsInDacDeclaration_NonISV,
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

		private static bool ShouldCheckIdentifier(MemberDeclarationSyntax member, Dictionary<string, List<PropertyDeclarationSyntax>> dacProperties)
		{
			if (!member.IsPublic() && !member.IsInternal())
				return false;

			if (member is ClassDeclarationSyntax dacFieldClassNode && !dacProperties.ContainsKey(dacFieldClassNode.Identifier.ValueText))
				return false;

			return true;
		}

		public static string[] GetForbiddenFieldsNames()
		{
			return new string[]
			{
				"CompanyID",
				_DeletedDatabaseRecord ,
				"CompanyMask"
			};
		}

		private static readonly string _DeletedDatabaseRecord = "DeletedDatabaseRecord";
	}
}