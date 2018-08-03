using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Data;
using Acuminator.Utilities;
using Acuminator.Utilities.PrimaryDAC;



namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXActionOnNonPrimaryViewAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1012_PXActionOnNonPrimaryView);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(symbolContext => AnalyzePXGraphSymbolAsync(symbolContext, pxContext),
														 SymbolKind.NamedType);
		}

		private static async Task AnalyzePXGraphSymbolAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is INamedTypeSymbol graphOrGraphExtension) || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			bool isGraph = graphOrGraphExtension.InheritsFrom(pxContext.PXGraphType);

			if (!isGraph && !graphOrGraphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return;

			ClassDeclarationSyntax graphOrGraphExtNode = await graphOrGraphExtension.GetSyntaxAsync(symbolContext.CancellationToken)
																					.ConfigureAwait(false) as ClassDeclarationSyntax;

			if (graphOrGraphExtNode == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			SemanticModel semanticModel = symbolContext.Compilation.GetSemanticModel(graphOrGraphExtNode.SyntaxTree);

			if (semanticModel == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var declaredActionsWithTypes = isGraph 
				? graphOrGraphExtension.GetPXActionSymbolsWithTypesFromGraph(pxContext, includeActionsFromInheritanceChain: false)
				: graphOrGraphExtension.GetPXActionSymbolsWithTypesFromGraphExtension(pxContext, includeActionsFromInheritanceChain: false);

			if (declaredActionsWithTypes.IsNullOrEmpty() || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			PrimaryDacFinder primaryDacFinder = PrimaryDacFinder.Create(pxContext, semanticModel, graphOrGraphExtension, 
																		symbolContext.CancellationToken);
			ITypeSymbol primaryDAC = primaryDacFinder?.FindPrimaryDAC();

			if (primaryDAC == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var actionsWithWrongDAC = declaredActionsWithTypes.TakeWhile(a => !symbolContext.CancellationToken.IsCancellationRequested)
															  .Where(a => !CheckActionIsDeclaredForPrimaryDAC(a.ActionType, primaryDAC))
															  .ToList();

			if (actionsWithWrongDAC.Count == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var diagnosticExtraData = new Dictionary<string, string>
			{
				{ DiagnosticProperty.DacName, primaryDAC.Name },
				{ DiagnosticProperty.DacMetadataName, primaryDAC.GetCLRTypeNameFromType() }
			}.ToImmutableDictionary();

			var registrationTasks = actionsWithWrongDAC.Select(a => RegisterDiagnosticForActionAsync(a.ActionSymbol, primaryDAC.Name, 
																									 diagnosticExtraData, symbolContext));
			await Task.WhenAll(registrationTasks);
		}

		private static bool CheckActionIsDeclaredForPrimaryDAC(INamedTypeSymbol action, ITypeSymbol primaryDAC)
		{
			var actionTypeArgs = action.TypeArguments;

			if (actionTypeArgs.Length == 0)   //Cannot infer action's DAC
				return true;

			ITypeSymbol pxActionDacType = actionTypeArgs[0];
			return pxActionDacType.Equals(primaryDAC);
		}

		private static async Task RegisterDiagnosticForActionAsync(ISymbol actionSymbol, string primaryDacName, 
																   ImmutableDictionary<string, string> diagnosticProperties,
																   SymbolAnalysisContext symbolContext)
		{
			SyntaxNode symbolSyntax = await actionSymbol.GetSyntaxAsync(symbolContext.CancellationToken).ConfigureAwait(false);
			Location location = GetLocation(symbolSyntax);

			if (location == null)
				return;

			symbolContext.ReportDiagnostic(
				Diagnostic.Create(Descriptors.PX1012_PXActionOnNonPrimaryView, location, diagnosticProperties,
								  actionSymbol.Name, primaryDacName));
		}

		private static Location GetLocation(SyntaxNode symbolSyntax)
		{
			switch (symbolSyntax)
			{
				case PropertyDeclarationSyntax propertyDeclaration:
					return propertyDeclaration.Type.GetLocation();
				case FieldDeclarationSyntax fieldDeclaration:
					return fieldDeclaration.Declaration.Type.GetLocation();
				case VariableDeclarationSyntax variableDeclaration:
					return variableDeclaration.Type.GetLocation();
				case VariableDeclaratorSyntax variableDeclarator 
				when variableDeclarator.Parent is VariableDeclarationSyntax variableDeclaration:
					return variableDeclaration.Type.GetLocation();
				default:
					return symbolSyntax?.GetLocation();
			}
		}
	}
}