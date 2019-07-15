using System;
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

namespace Acuminator.Analyzers.StaticAnalysis.UnderscoresInDac
{
	/// <summary>
	/// An underscores in DAC analyzer.
	/// </summary>
	public class UnderscoresInDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1026_UnderscoresInDacDeclaration);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExt)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			SyntaxToken dacIdentifier = dacOrDacExt.DacNode.Identifier;
			CheckIdentifierForUnderscores(dacIdentifier, context, pxContext);

			var fieldsIdentifiers = dacOrDacExt.Fields.Where(field => ShouldCheckDacMember(field.Symbol))
													  .SelectMany(field => field.Node.GetIdentifiers());
			var propertiesIdentifiers = dacOrDacExt.Properties.Where(property => ShouldCheckDacMember(property.Symbol))
															  .SelectMany(property => property.Node.GetIdentifiers());

			var identifiersToCheck = fieldsIdentifiers.Concat(propertiesIdentifiers);

			foreach (SyntaxToken identifier in identifiersToCheck)
			{
				CheckIdentifierForUnderscores(identifier, context, pxContext);
			}			
		}

		private static void CheckIdentifierForUnderscores(SyntaxToken identifier, SymbolAnalysisContext context, PXContext pxContext)
		{
			if (!identifier.ValueText.Contains("_"))
				return;

			Location location = identifier.GetLocation();

			if (location == null)
				return;

			bool registerCodeFix = !IdentifierContainsOnlyUnderscores(identifier.ValueText);

			var diagnosticProperties = new Dictionary<string, string>
			{
				{ DiagnosticProperty.RegisterCodeFix, registerCodeFix.ToString() }
			}.ToImmutableDictionary();

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1026_UnderscoresInDacDeclaration, location, diagnosticProperties),
				pxContext.CodeAnalysisSettings);
		}

		private static bool IdentifierContainsOnlyUnderscores(string identifier)
		{
			for (int i = 0; i < identifier.Length; i++)
			{
				if (identifier[i] != '_')
					return false;
			}

			return true;
		}

		private static bool ShouldCheckDacMember(ISymbol memberSymbol) =>
			memberSymbol.DeclaredAccessibility == Accessibility.Public ||
			memberSymbol.DeclaredAccessibility == Accessibility.Internal;
	
	}
}