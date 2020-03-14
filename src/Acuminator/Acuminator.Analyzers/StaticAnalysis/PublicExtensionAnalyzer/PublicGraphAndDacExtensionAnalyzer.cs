using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PublicExtensionAnalyzer
{
	/// <summary>
	/// An analyzer that checks that DAC and graph extensions are public.
	/// </summary>
	public class PublicGraphAndDacExtensionAnalyzer : IDacAnalyzer, IPXGraphAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1022_NonPublicDacExtension,
								  Descriptors.PX1022_NonPublicGraphExtension);

		bool IDacAnalyzer.ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			dac?.DacType == DacType.DacExtension;

		bool IPXGraphAnalyzer.ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			graph?.Type == GraphType.PXGraphExtension;

		void IDacAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacExtension) =>
			CheckSymbolIsPublic(context, pxContext, dacExtension);

		void IPXGraphAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphExtension) =>
			CheckSymbolIsPublic(context, pxContext, graphExtension);

		private void CheckSymbolIsPublic(SymbolAnalysisContext context, PXContext pxContext, ISemanticModel extension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (extension.Symbol.DeclaredAccessibility == Accessibility.Public)
				return;

			var descriptor = extension is DacSemanticModel
				? Descriptors.PX1022_NonPublicDacExtension
				: Descriptors.PX1022_NonPublicGraphExtension;

			var locations = GetDiagnosticLocations(extension.Symbol, context.CancellationToken);

			foreach (Location location in locations)
			{
				context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, location),
															 pxContext.CodeAnalysisSettings);
			}		
		}

		private IEnumerable<Location> GetDiagnosticLocations(INamedTypeSymbol extensionSymbol, CancellationToken cancellationToken) =>
			extensionSymbol.DeclaringSyntaxReferences
											.Select(reference => reference.GetSyntax(cancellationToken))
											.OfType<ClassDeclarationSyntax>()
											.Where(location => location != null);
	}
}