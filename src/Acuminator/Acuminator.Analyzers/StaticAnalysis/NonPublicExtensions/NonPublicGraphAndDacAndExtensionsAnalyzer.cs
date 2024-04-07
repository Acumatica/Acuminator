#nullable enable

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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
	/// <summary>
	/// An analyzer that checks that DACs, graphs, DAC and graph extensions are public.
	/// </summary>
	public class NonPublicGraphAndDacAndExtensionsAnalyzer : IDacAnalyzer, IPXGraphAnalyzer
	{
		private enum CheckedSymbolKind
		{
			Dac,
			Graph,
			DacExtension,
			GraphExtension
		}

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1022_NonPublicDac,
				Descriptors.PX1022_NonPublicDacExtension,
				Descriptors.PX1022_NonPublicGraph,
				Descriptors.PX1022_NonPublicGraphExtension
			);

		bool IDacAnalyzer.ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) => dac != null;

		bool IPXGraphAnalyzer.ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) => 
			graph != null && graph.Type != GraphType.None;

		void IDacAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension) =>
			CheckSymbolIsPublic(context, pxContext, dacOrDacExtension, 
								checkedSymbolKind: dacOrDacExtension.DacType == DacType.Dac
													? CheckedSymbolKind.Dac
													: CheckedSymbolKind.DacExtension);

		void IPXGraphAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graphOrGraphExtension) =>
			CheckSymbolIsPublic(context, pxContext, graphOrGraphExtension, 
								checkedSymbolKind: graphOrGraphExtension.Type == GraphType.PXGraph
													? CheckedSymbolKind.Graph
													: CheckedSymbolKind.GraphExtension);

		private void CheckSymbolIsPublic(SymbolAnalysisContext context, PXContext pxContext, ISemanticModel dacOrGraphOrExtensionModel, 
										 CheckedSymbolKind checkedSymbolKind)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (dacOrGraphOrExtensionModel.Symbol.IsPublicWithAllContainingTypes())
				return;

			

			string extensionType;
			DiagnosticDescriptor descriptor; 
			
			if (dacOrGraphOrExtensionModel is DacSemanticModel)
			{
				extensionType = ExtensionType.DAC;
				descriptor = Descriptors.PX1022_NonPublicDacExtension;
			}
			else
			{
				extensionType = ExtensionType.Graph;
				descriptor = Descriptors.PX1022_NonPublicGraphExtension;
			}
			
			var locations = GetDiagnosticLocations(dacOrGraphOrExtensionModel.Symbol, context.CancellationToken);
			var diagnosticProperties = new Dictionary<string, string>
			{
				{ nameof(ExtensionType),  extensionType}
			}
			.ToImmutableDictionary();

			foreach (Location location in locations)
			{
				context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, location, diagnosticProperties),
															 pxContext.CodeAnalysisSettings);
			}		
		}

		private IEnumerable<Location> GetDiagnosticLocations(INamedTypeSymbol symbol, CancellationToken cancellationToken) =>
			symbol.DeclaringSyntaxReferences
				  .Select(reference => reference.GetSyntax(cancellationToken))
				  .OfType<ClassDeclarationSyntax>()
				  .Select(classNode => GetLocationFromClassNode(classNode));
		
		private Location GetLocationFromClassNode(ClassDeclarationSyntax classNode)
		{
			if (classNode.Modifiers.Count == 0)
				return classNode.Identifier.GetLocation();

			int lastModifierIndex = classNode.Modifiers.Count - 1;

			for (int i = 0; i < classNode.Modifiers.Count; i++)
			{
				SyntaxToken modifier = classNode.Modifiers[i];

				if (GetLocationFromModifier(classNode, i, modifier) is Location modifierLocation)
					return modifierLocation;
			}

			return classNode.Identifier.GetLocation();

			//------------------------------------------------Local Function------------------------------------------------------------
			Location? GetLocationFromModifier(ClassDeclarationSyntax classNode, int modifierIndex, in SyntaxToken modifier) =>
				modifier.Kind() switch
				{
					SyntaxKind.PublicKeyword 	=> classNode.Identifier.GetLocation(),     //Case when nested public class is declared inside non-public type
					SyntaxKind.PrivateKeyword 	=> GetLocationForComplexAccessModifier(modifier, modifierIndex, secondPartKind: SyntaxKind.ProtectedKeyword),
					SyntaxKind.ProtectedKeyword => GetLocationForComplexAccessModifier(modifier, modifierIndex, secondPartKind: SyntaxKind.InternalKeyword),
					SyntaxKind.InternalKeyword 	=> modifier.GetLocation(),
					_ 							=> null
				};

			//--------------------------------------------------------------------------------------------------------------------------
			Location GetLocationForComplexAccessModifier(in SyntaxToken modifier, int modifierIndex, SyntaxKind secondPartKind)
			{
				if (modifierIndex == lastModifierIndex)
					return modifier.GetLocation();

				SyntaxToken nextModifier = classNode.Modifiers[modifierIndex + 1];
				return nextModifier.IsKind(secondPartKind)
					? Location.Create(modifier.SyntaxTree, TextSpan.FromBounds(modifier.SpanStart, nextModifier.Span.End))
					: modifier.GetLocation();
			}
		}
	}
}