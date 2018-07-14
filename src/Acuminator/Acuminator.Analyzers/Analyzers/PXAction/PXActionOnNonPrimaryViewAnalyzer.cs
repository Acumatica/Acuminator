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
														 SymbolKind.Field, SymbolKind.Property);
		}

		private static async Task AnalyzePXGraphSymbolAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!(symbolContext.Symbol is INamedTypeSymbol graphOrGraphExtension) || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			ClassDeclarationSyntax graphOrGraphExtNode = await graphOrGraphExtension.GetSyntaxAsync(symbolContext.CancellationToken)
																					.ConfigureAwait(false) as ClassDeclarationSyntax;

			if (graphOrGraphExtNode == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			SemanticModel semanticModel = symbolContext.Compilation.GetSemanticModel(graphOrGraphExtNode.SyntaxTree);

			if (semanticModel == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			bool isGraph = graphOrGraphExtension.InheritsFrom(pxContext.PXGraphType);

			if (!isGraph && !graphOrGraphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return;

			var declaredActions = graphOrGraphExtension.GetPXActionsFromGraphOrGraphExtension(pxContext);

			if (declaredActions.IsNullOrEmpty() || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			PrimaryDacFinder primaryDacFinder = PrimaryDacFinder.Create(pxContext, semanticModel, graphOrGraphExtension, 
																		symbolContext.CancellationToken);
			ITypeSymbol primaryDAC = primaryDacFinder.FindPrimaryDAC();

			if (primaryDAC == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var actionsWithWrongDAC = declaredActions.TakeWhile(a => !symbolContext.CancellationToken.IsCancellationRequested)
													 .Where(a => CheckActionIsDeclaredForPrimaryDAC(a, primaryDAC));

			if (actionsWithWrongDAC.IsNullOrEmpty() || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var diagnosticExtraData = new Dictionary<string, string>
			{
				{ DiagnosticProperty.DacName, primaryDAC.Name },
				{ DiagnosticProperty.DacMetadataName, primaryDAC.GetCLRTypeNameFromType() }
			}.ToImmutableDictionary();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			actionsWithWrongDAC.ForEach(action => RegisterDiagnosticForActionAsync(action, primaryDAC.Name, diagnosticExtraData, symbolContext));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		private static bool CheckActionIsDeclaredForPrimaryDAC(INamedTypeSymbol action, ITypeSymbol primaryDAC)
		{
			var actionTypeArgs = action.TypeArguments;

			if (actionTypeArgs.Length == 0)   //Cannot infer action's DAC
				return true;

			ITypeSymbol pxActionDacType = actionTypeArgs[0];
			return pxActionDacType.Equals(primaryDAC);
		}

		private static async Task RegisterDiagnosticForActionAsync(INamedTypeSymbol action, string primaryDacName, 
																   ImmutableDictionary<string, string> diagnosticProperties,
																   SymbolAnalysisContext symbolContext)
		{
			SyntaxNode symbolSyntax = await action.GetSyntaxAsync(symbolContext.CancellationToken).ConfigureAwait(false);
			Location location = GetLocation(symbolSyntax);

			if (location == null)
				return;

			symbolContext.ReportDiagnostic(
				Diagnostic.Create(Descriptors.PX1012_PXActionOnNonPrimaryView, location, diagnosticProperties,
								  action.Name, primaryDacName));
		}

		//private static ITypeSymbol GetMainDacFromPXGraph(INamedTypeSymbol pxGraphType, PXContext pxContext, CancellationToken cancellationToken)
		//{
		//	if (pxGraphType.BaseType == null)
		//		return null;

		//	//var baseGraphType = pxGraphType.GetBaseTypesAndThis()
		//	//							   .OfType<INamedTypeSymbol>()
		//	//							   .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type));

		//	if (baseGraphType != null)  //Case when main DAC is already defined as type parameter
		//	{
		//		return baseGraphType.TypeArguments[1];
		//	}

		//	INamedTypeSymbol firstView = pxGraphType.GetBaseTypesAndThis()
		//											.TakeWhile(type => !IsGraphOrGraphExtensionBaseType(type))
		//											.Reverse()
		//											.SelectMany(type => type.GetAllViewTypesFromPXGraphOrPXGraphExtension(pxContext))
		//											.FirstOrDefault();

		//	if (firstView == null || cancellationToken.IsCancellationRequested)
		//		return null;

		//INamedTypeSymbol baseViewType = firstView.GetBaseTypesAndThis()
		//										 .OfType<INamedTypeSymbol>()
		//										 .FirstOrDefault(type => !type.IsCustomBqlCommand(pxContext));

		//	if (baseViewType?.IsBqlCommand() != true || baseViewType.TypeArguments.Length == 0 || cancellationToken.IsCancellationRequested)
		//		return null;

		//	var mainDacType = baseViewType.TypeArguments[0];
		//	return mainDacType.IsDAC() ? mainDacType : null;
		//}

		//private static ITypeSymbol GetMainDacFromPXGraphExtension(INamedTypeSymbol pxGraphExtensionType, PXContext pxContext,
		//														  CancellationToken cancellationToken)
		//{
		//	if (pxGraphExtensionType.BaseType == null)
		//		return null;

		//	var graphExtTypeArgs = pxGraphExtensionType.BaseType.TypeArguments;

		//	if (graphExtTypeArgs.Length == 0 || cancellationToken.IsCancellationRequested)
		//		return null;

		//	ITypeSymbol firstTypeArg = graphExtTypeArgs[0];

		//	if (!(firstTypeArg is INamedTypeSymbol pxGraphType) || !pxGraphType.IsPXGraph())
		//		return null;

		//	return GetMainDacFromPXGraph(pxGraphType, pxContext, cancellationToken);
		//}

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