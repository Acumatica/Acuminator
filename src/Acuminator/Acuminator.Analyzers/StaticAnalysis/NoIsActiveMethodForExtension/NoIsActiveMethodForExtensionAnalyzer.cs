﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

namespace Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension
{
	/// <summary>
	/// The analyzer which checks that DAC and Graph extensions have IsActive method declared.
	/// </summary>
	public class NoIsActiveMethodForExtensionAnalyzer : IDacAnalyzer, IPXGraphWithGraphEventsAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1016_NoIsActiveMethodForDacExtension,
								  Descriptors.PX1016_NoIsActiveMethodForGraphExtension);

		public bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dacExtension) =>
			dacExtension?.DacType == DacType.DacExtension && dacExtension.IsActiveMethodInfo == null &&
			!dacExtension.Symbol.IsAbstract && !dacExtension.Symbol.IsGenericType && !dacExtension.IsMappedCacheExtension;

		public void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, DacSemanticModel dacExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// ShouldAnalyze already filtered everything and left only DAC extensions without IsActive
			// We just need to report them
			Location location = dacExtension.Node.Identifier.GetLocation();

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1016_NoIsActiveMethodForDacExtension, location), 
				pxContext.CodeAnalysisSettings);
		}

		public bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graphExtension) =>
			graphExtension.Type == GraphType.PXGraphExtension && graphExtension.IsActiveMethodInfo == null &&
			!graphExtension.Symbol.IsGenericType &&
			(!graphExtension.Symbol.IsAbstract || graphExtension.HasPXProtectedAccess);
		
		public bool ShouldAnalyze(PXContext pxContext, PXGraphEventSemanticModel graphExtensionhWithEvents) =>
			!graphExtensionhWithEvents.ConfiguresWorkflow || IsWorkflowExtensionWithBusinessLogic(graphExtensionhWithEvents);	// Filter out workflow extensions without business logic.

		private bool IsWorkflowExtensionWithBusinessLogic(PXGraphEventSemanticModel graphExtension)
		{
			if (graphExtension.DeclaredActions.Any() || graphExtension.DeclaredActionHandlers.Any() ||
				graphExtension.DeclaredViews.Any()   || graphExtension.DeclaredViewDelegates.Any() ||
				!graphExtension.PXOverrides.IsDefaultOrEmpty)
			{
				return true;
			}

			return graphExtension.GetAllEvents()
								 .Any(graphEvent => graphEvent.Symbol.IsDeclaredInType(graphExtension.Symbol));
		}

		public void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel graphExtension)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			// ShouldAnalyze already filtered everything and left only graph extensions without IsActive
			// We just need to report them
			var syntaxNode = graphExtension.Symbol.GetSyntax(symbolContext.CancellationToken);
			Location? location = (syntaxNode as ClassDeclarationSyntax)?.Identifier.GetLocation() ?? syntaxNode?.GetLocation();

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1016_NoIsActiveMethodForGraphExtension, location),
				pxContext.CodeAnalysisSettings);
		}
	}
}