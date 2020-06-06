using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;



namespace Acuminator.Analyzers.StaticAnalysis.PXActionOnNonPrimaryView
{
	public class PXActionOnNonPrimaryViewAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1012_PXActionOnNonPrimaryView);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			graph.Type != GraphType.None && base.ShouldAnalyze(pxContext, graph); //-V3063

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext pxContext, PXGraphSemanticModel pxGraph)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			var declaredActions = pxGraph.DeclaredActions.ToList();

			if (declaredActions.Count == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			PrimaryDacFinder primaryDacFinder = PrimaryDacFinder.Create(pxContext, pxGraph, symbolContext.CancellationToken);
			ITypeSymbol primaryDAC = primaryDacFinder?.FindPrimaryDAC();

			if (primaryDAC == null)
				return;

			ImmutableDictionary<string, string> diagnosticExtraData = null;

			foreach (ActionInfo action in declaredActions)
			{
				symbolContext.CancellationToken.ThrowIfCancellationRequested();

				if (CheckActionIsDeclaredForPrimaryDAC(action.Type, primaryDAC))
					continue;

				diagnosticExtraData = diagnosticExtraData ??
					new Dictionary<string, string>
					{
						{ DiagnosticProperty.DacName, primaryDAC.Name },
						{ DiagnosticProperty.DacMetadataName, primaryDAC.GetCLRTypeNameFromType() }
					}
					.ToImmutableDictionary();

				RegisterDiagnosticForAction(action.Symbol, primaryDAC.Name, diagnosticExtraData, symbolContext, pxContext);
			}
		}

		private static bool CheckActionIsDeclaredForPrimaryDAC(INamedTypeSymbol action, ITypeSymbol primaryDAC)
		{
			var actionTypeArgs = action.TypeArguments;

			if (actionTypeArgs.Length == 0)   //Cannot infer action's DAC
				return true;

			ITypeSymbol pxActionDacType = actionTypeArgs[0];
			return pxActionDacType.Equals(primaryDAC);
		}

		private static void RegisterDiagnosticForAction(ISymbol actionSymbol, string primaryDacName, 
														ImmutableDictionary<string, string> diagnosticProperties,
														SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			SyntaxNode symbolSyntax = actionSymbol.GetSyntax(symbolContext.CancellationToken);
			Location location = GetLocation(symbolSyntax);

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1012_PXActionOnNonPrimaryView, location, diagnosticProperties,
								  actionSymbol.Name, primaryDacName), pxContext.CodeAnalysisSettings);
		}

		private static Location GetLocation(SyntaxNode symbolSyntax) =>
			symbolSyntax switch
			{
				PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Type.GetLocation(),
				FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Type.GetLocation(),
				VariableDeclarationSyntax variableDeclaration => variableDeclaration.Type.GetLocation(),
				VariableDeclaratorSyntax variableDeclarator
					when variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration => variableDeclaration.Type.GetLocation(),
				_ => symbolSyntax?.GetLocation(),
			};
	}
}