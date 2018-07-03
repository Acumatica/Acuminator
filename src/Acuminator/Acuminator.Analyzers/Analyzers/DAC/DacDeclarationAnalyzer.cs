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
				Descriptors.PX1026_UnderscoresInDacDeclaration
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

			CheckDeclarationForUnderscores(dacOrDacExtNode, syntaxContext);
		}

		private static void CheckDeclarationForUnderscores(ClassDeclarationSyntax dacOrDacExtNode, SyntaxNodeAnalysisContext syntaxContext)
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
											 where member.IsPublic()
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
	}
}