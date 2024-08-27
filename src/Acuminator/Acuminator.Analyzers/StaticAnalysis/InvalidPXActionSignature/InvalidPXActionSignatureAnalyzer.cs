﻿
using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature
{
	public class InvalidPXActionSignatureAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature);

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var actionHandlerWithBadSignature = from method in pxGraph.Symbol.GetMethods()
												where pxGraph.Symbol.Equals(method.ContainingType, SymbolEqualityComparer.Default) &&
													  CheckIfDiagnosticShouldBeRegisteredForMethod(method, pxContext) &&
													  pxGraph.ActionsByNames.ContainsKey(method.Name)
												select method;

			foreach (IMethodSymbol method in actionHandlerWithBadSignature)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();
				Location? methodLocation = method.Locations.FirstOrDefault();

				if (methodLocation != null)
				{
					symbolContext.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature, methodLocation),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

		private static bool CheckIfDiagnosticShouldBeRegisteredForMethod(IMethodSymbol method, PXContext pxContext)
		{
			if (method.ReturnsVoid && method.Parameters.Length > 0)
				return true;

			return method.ReturnType.SpecialType == SpecialType.System_Collections_IEnumerable &&
				(method.Parameters.Length == 0 || !method.Parameters[0].Type.Equals(pxContext.PXAdapterType, SymbolEqualityComparer.Default));
		}
	}
}