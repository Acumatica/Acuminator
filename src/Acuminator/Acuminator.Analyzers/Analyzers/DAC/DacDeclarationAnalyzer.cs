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
	public class DacDeclarationAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1026_UnderscoresInDacDeclaration,
				Descriptors.PX1027_ForbiddenFieldsInDacDeclaration,
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

			CheckDeclarationForUnderscores(dacOrDacExtNode, syntaxContext, dacProperties);
			CheckDeclarationForForbiddenFields(dacOrDacExtNode, syntaxContext, dacProperties);
			CheckDeclarationForConstructors(dacOrDacExtNode, syntaxContext);
		}

		private static void CheckDeclarationForUnderscores(ClassDeclarationSyntax dacOrDacExtNode,
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

				syntaxContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1026_UnderscoresInDacDeclaration, identifier.GetLocation(), diagnosticProperties));
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

				syntaxContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1026_UnderscoresInDacDeclaration, identifierToReport.GetLocation(), diagnosticProperties));
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

		private static void CheckDeclarationForForbiddenFields(ClassDeclarationSyntax dacOrDacExtNode,
																SyntaxNodeAnalysisContext syntaxContext,
																Dictionary<string, List<PropertyDeclarationSyntax>> dacProperties)
		{
			string[] forbiddenFields = GetForbiddenFieldsNames();
			var invalidPropertiesByName = from dacProperty in dacProperties
										  where forbiddenFields.Contains(dacProperty.Key, StringComparer.OrdinalIgnoreCase)
										  select dacProperty;

			foreach (KeyValuePair<string, List<PropertyDeclarationSyntax>> propertiesWithInvalidName in invalidPropertiesByName)
			{
				IEnumerable<ClassDeclarationSyntax> invalidClasses = dacOrDacExtNode.Members
																					.OfType<ClassDeclarationSyntax>()
																					.Where(dacField => string.Equals(dacField.Identifier.ValueText,
																										propertiesWithInvalidName.Key,
																										StringComparison.OrdinalIgnoreCase));
				foreach (ClassDeclarationSyntax dacFieldClass in invalidClasses)
				{
					syntaxContext.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.PX1027_ForbiddenFieldsInDacDeclaration, dacFieldClass.Identifier.GetLocation(),
							dacFieldClass.Identifier.Text));
				}

				foreach (PropertyDeclarationSyntax invalidProperties in propertiesWithInvalidName.Value)
				{
					syntaxContext.ReportDiagnostic(
						Diagnostic.Create(
							Descriptors.PX1027_ForbiddenFieldsInDacDeclaration, invalidProperties.Identifier.GetLocation(),
							invalidProperties.Identifier.Text));
				}
			}
		}

		private static void CheckDeclarationForConstructors(ClassDeclarationSyntax dacOrDacExtNode,
															SyntaxNodeAnalysisContext syntaxContext)
		{
			var dacConstructors = dacOrDacExtNode.Members.OfType<ConstructorDeclarationSyntax>();

			foreach (var constructor in dacConstructors)
			{
				syntaxContext.ReportDiagnostic(
					Diagnostic.Create(
						Descriptors.PX1028_ConstructorInDacDeclaration, constructor.Identifier.GetLocation()));
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
				"DeletedDatabaseRecord",
				"CompanyMask"
			};
		}
	}
}