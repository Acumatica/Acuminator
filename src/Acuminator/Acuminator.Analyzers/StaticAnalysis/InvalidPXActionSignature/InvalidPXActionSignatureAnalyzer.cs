using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class InvalidPXActionSignatureAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private static void AnalyzeMethod(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is IMethodSymbol method) || symbolContext.CancellationToken.IsCancellationRequested ||
				!CheckIfDiagnosticShouldBeRegisteredForMethod(method, pxContext))
			{
				return;
			}

			var graphOrGraphExt = method.ContainingType;
			bool isGraph = graphOrGraphExt?.InheritsFrom(pxContext.PXGraph.Type) ?? false;

			if (graphOrGraphExt == null ||
				(!isGraph && !graphOrGraphExt.InheritsFrom(pxContext.PXGraphExtensionType)))
			{
				return;
			}

			var actionsWithTypes = isGraph
				? graphOrGraphExt.GetPXActionSymbolsWithTypesFromGraph(pxContext)
				: graphOrGraphExt.GetPXActionSymbolsWithTypesFromGraphExtension(pxContext);

			var action = actionsWithTypes.FirstOrDefault(a => String.Equals(a.ActionSymbol.Name, method.Name, StringComparison.OrdinalIgnoreCase))
										 .ActionSymbol;

			if (action == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			Location methodLocation = method.Locations.FirstOrDefault();

			if (methodLocation != null)
			{
				symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature, methodLocation));
			}
		}

		private static bool CheckIfDiagnosticShouldBeRegisteredForMethod(IMethodSymbol method, PXContext pxContext)
		{
			if (method.ReturnsVoid && method.Parameters.Length > 0)
				return true;

			return method.ReturnType.SpecialType == SpecialType.System_Collections_IEnumerable &&
				(method.Parameters.Length == 0 || !method.Parameters[0].Type.Equals(pxContext.PXAdapterType));
		}
	}
}