using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;



namespace Acuminator.Analyzers.StaticAnalysis.NoPrimaryViewForPrimaryDac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NoPrimaryViewForPrimaryDacAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1018_NoPrimaryViewForPrimaryDac);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) => graph.Type == GraphType.PXGraph;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graph)
		{
			ITypeSymbol declaredPrimaryDacType = graph.Symbol.GetDeclaredPrimaryDacFromGraphOrGraphExtension(pxContext);

			if (declaredPrimaryDacType == null || context.CancellationToken.IsCancellationRequested)
				return;

			bool hasViewForPrimaryDac = graph.Views.Select(view => view.DAC).Contains(declaredPrimaryDacType);

			if (hasViewForPrimaryDac || context.CancellationToken.IsCancellationRequested)
				return;

			Location location = GetLocation(graph, declaredPrimaryDacType, context);

			if (location == null)
				return;

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1018_NoPrimaryViewForPrimaryDac, location),
				pxContext.CodeAnalysisSettings);
		}

		private static Location GetLocation(PXGraphSemanticModel graph, ITypeSymbol declaredPrimaryDacType,
											SymbolAnalysisContext context)
		{
			if (!(graph.Symbol.GetSyntax(context.CancellationToken) is ClassDeclarationSyntax graphNode))
				return null;

			SemanticModel semanticModel = context.Compilation.GetSemanticModel(graphNode.SyntaxTree);

			if (semanticModel == null)
				return graphNode.Identifier.GetLocation();

			var baseClassesTypeNodes = graphNode.BaseList.Types.Select(baseTypeNode => baseTypeNode.Type)
															   .OfType<GenericNameSyntax>();

			foreach (GenericNameSyntax baseClassTypeNode in baseClassesTypeNodes)
			{
				var baseClassTypeSymbol = semanticModel.GetSymbolInfo(baseClassTypeNode, context.CancellationToken).Symbol as INamedTypeSymbol;
				Location location = GetLocationFromBaseClassTypeNode(baseClassTypeNode, baseClassTypeSymbol, declaredPrimaryDacType);

				if (location != null)
					return location;
			}

			return graphNode.Identifier.GetLocation();
		}

		private static Location GetLocationFromBaseClassTypeNode(GenericNameSyntax baseClassTypeNode, INamedTypeSymbol baseClassTypeSymbol,
																 ITypeSymbol declaredPrimaryDacType)
		{
			if (baseClassTypeSymbol == null || !baseClassTypeSymbol.IsPXGraph() || baseClassTypeSymbol.TypeArguments.Length < 2)
				return null;

			int indexOfDac = baseClassTypeSymbol.TypeArguments.IndexOf(declaredPrimaryDacType);
			return indexOfDac >= 0
				? baseClassTypeNode.TypeArgumentList.Arguments[indexOfDac].GetLocation()
				: null;
		}
	}
}