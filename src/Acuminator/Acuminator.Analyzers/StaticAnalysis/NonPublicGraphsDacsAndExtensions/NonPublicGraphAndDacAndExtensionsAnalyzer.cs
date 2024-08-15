#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using ModifierSetsLocations = System.Collections.Generic.List<Microsoft.CodeAnalysis.Location>;

namespace Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions
{
	/// <summary>
	/// An analyzer that checks that DACs, graphs, DAC and graph extensions are public.
	/// </summary>
	public class NonPublicGraphAndDacAndExtensionsAnalyzer : IDacAnalyzer, IPXGraphAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1022_NonPublicDac,
				Descriptors.PX1022_NonPublicDacExtension,
				Descriptors.PX1022_NonPublicGraph,
				Descriptors.PX1022_NonPublicGraphExtension
			);

		bool IDacAnalyzer.ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) => dac?.IsInSource == true;

		bool IPXGraphAnalyzer.ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graph) => 
			graph != null && graph.Type != GraphType.None;

		void IDacAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dacOrDacExtension) =>
			CheckSymbolIsPublic(context, pxContext, dacOrDacExtension, 
								checkedSymbolKind: dacOrDacExtension.DacType == DacType.Dac
													? CheckedSymbolKind.Dac
													: CheckedSymbolKind.DacExtension);

		void IPXGraphAnalyzer.Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel graphOrGraphExtension) =>
			CheckSymbolIsPublic(context, pxContext, graphOrGraphExtension, 
								checkedSymbolKind: graphOrGraphExtension.Type == GraphType.PXGraph
													? CheckedSymbolKind.Graph
													: CheckedSymbolKind.GraphExtension);

		private void CheckSymbolIsPublic(SymbolAnalysisContext context, PXContext pxContext, ISemanticModel dacOrGraphOrExtensionModel, 
										 CheckedSymbolKind checkedSymbolKind)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (dacOrGraphOrExtensionModel.Symbol.IsPublicWithAllContainingTypes() ||
				GetDiagnosticDescriptor(checkedSymbolKind) is not DiagnosticDescriptor descriptor)
			{
				return;
			}

			var modifierSetsLocationsForTypeDeclarations = 
				GetBadModifierSetsLocationsForTypeDeclarations(dacOrGraphOrExtensionModel.Symbol, context.CancellationToken);
			ImmutableDictionary<string, string>? diagnosticProperties = null;

			foreach (ModifierSetsLocations typeDeclarationModifierSetsLocations in modifierSetsLocationsForTypeDeclarations)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				foreach (Location modifierSetLocation in typeDeclarationModifierSetsLocations)
				{
					diagnosticProperties ??= ImmutableDictionary<string, string>.Empty
																				.Add(nameof(CheckedSymbolKind), checkedSymbolKind.ToString());

					var diagnostic = Diagnostic.Create(descriptor, modifierSetLocation, diagnosticProperties);
					context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
				}
			}
		}

		private DiagnosticDescriptor? GetDiagnosticDescriptor(CheckedSymbolKind checkedSymbolKind) =>
			checkedSymbolKind switch
			{
				CheckedSymbolKind.Dac 			 => Descriptors.PX1022_NonPublicDac,
				CheckedSymbolKind.DacExtension 	 => Descriptors.PX1022_NonPublicDacExtension,
				CheckedSymbolKind.Graph 		 => Descriptors.PX1022_NonPublicGraph,
				CheckedSymbolKind.GraphExtension => Descriptors.PX1022_NonPublicGraphExtension,
				_								 => null
			};

		private IEnumerable<ModifierSetsLocations> GetBadModifierSetsLocationsForTypeDeclarations(INamedTypeSymbol dacOrGraphType, 
																								  CancellationToken cancellationToken)
		{
			if (dacOrGraphType.DeclaringSyntaxReferences.IsDefaultOrEmpty)
				return [];

			if (dacOrGraphType.DeclaringSyntaxReferences.Length == 1)
			{
				if (dacOrGraphType.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is not ClassDeclarationSyntax classNode)
					return [];

				var modifierSetsLocations = GetModifierSetsLocationsFromClassNode(classNode);
				return [modifierSetsLocations];
			}

			return GetModifierSetsLocationsFromPartialType(dacOrGraphType, cancellationToken);
		}

		private IEnumerable<ModifierSetsLocations> GetModifierSetsLocationsFromPartialType(INamedTypeSymbol partialDacOrGraphType, 
																						   CancellationToken cancellationToken)
		{
			var partialTypeDeclarations = partialDacOrGraphType.DeclaringSyntaxReferences
															   .Select(reference => reference.GetSyntax(cancellationToken))
															   .OfType<ClassDeclarationSyntax>();
			foreach (var typeNode in partialTypeDeclarations)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var modifierSetsLocations = GetModifierSetsLocationsFromClassNode(typeNode);
				yield return modifierSetsLocations;
			}
		}

		private ModifierSetsLocations GetModifierSetsLocationsFromClassNode(ClassDeclarationSyntax classNode)
		{
			if (classNode.Modifiers.Count == 0)
				return [classNode.Identifier.GetLocation()];

			List<List<SyntaxToken>> continousModifiersSets = GetContinousModifiersSets(classNode);
			var locations = TransformContinousModifiersSetsToLocations(classNode, continousModifiersSets);

			return locations;
		}

		private List<List<SyntaxToken>> GetContinousModifiersSets(ClassDeclarationSyntax classNode)
		{
			List<List<SyntaxToken>> continousModifiersSets = new();
			bool hasPreviousAccessModifier = false;

			for (int i = 0; i < classNode.Modifiers.Count; i++)
			{
				SyntaxToken modifier = classNode.Modifiers[i];

				if (SyntaxFacts.IsAccessibilityModifier(modifier.Kind()))
				{
					AddModifier(modifier);
					hasPreviousAccessModifier = true;
				}
				else
					hasPreviousAccessModifier = false;
			}

			return continousModifiersSets;

			//------------------------------------------------Local Function------------------------------------------------------------
			void AddModifier(in SyntaxToken modifier)
			{
				bool needToAddNewContinousModifiersSet = !hasPreviousAccessModifier;

				if (hasPreviousAccessModifier)
				{
					List<SyntaxToken>? lastModifiersSet = continousModifiersSets.LastOrDefault();

					if (lastModifiersSet != null)
						lastModifiersSet.Add(modifier);
					else
						needToAddNewContinousModifiersSet = true;
				}

				if (needToAddNewContinousModifiersSet)
				{
					List<SyntaxToken> continousModifiers = [modifier];
					continousModifiersSets.Add(continousModifiers);
				}
			}
		}

		private ModifierSetsLocations TransformContinousModifiersSetsToLocations(ClassDeclarationSyntax classNode, 
																				 List<List<SyntaxToken>> continousModifiersSets)
		{
			if (continousModifiersSets.Count == 0)
				return [classNode.Identifier.GetLocation()];

			bool classIdentifierLocationWasAdded = false;
			ModifierSetsLocations locations = new(capacity: continousModifiersSets.Count);

			foreach (List<SyntaxToken> modifiersSet in continousModifiersSets)
			{
				if (CreateLocationFromModifiersSet(modifiersSet) is Location location)
					locations.Add(location);
			}

			return locations;

			//---------------------------------------------Local Function----------------------------------------------------------
			Location? CreateLocationFromModifiersSet(List<SyntaxToken> modifiersSet)
			{
				if (modifiersSet.Count == 1)    
				{
					var modifier = modifiersSet[0];

					if (modifier.IsKind(SyntaxKind.PublicKeyword))  //Special case - public type nested in non public
					{
						if (!classIdentifierLocationWasAdded)
						{
							classIdentifierLocationWasAdded = true;
							return classNode.Identifier.GetLocation();
						}

						return null;
					}
					else
						return modifier.GetLocation();
				}
				else
				{
					var firstModifier = modifiersSet[0];
					var lastModifier  = modifiersSet[^1];
					var span		  = TextSpan.FromBounds(firstModifier.SpanStart, lastModifier.Span.End);

					return Location.Create(firstModifier.SyntaxTree, span);
				}
			}
		}
	}
}