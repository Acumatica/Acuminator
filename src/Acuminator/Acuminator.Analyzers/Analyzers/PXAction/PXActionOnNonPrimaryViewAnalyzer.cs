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

			bool isGraph = graphOrGraphExtension.InheritsFrom(pxContext.PXGraphType);

			if (!isGraph && !graphOrGraphExtension.InheritsFrom(pxContext.PXGraphExtensionType))
				return;

			var actions = GetActionsFromGraphOrGraphExtension(graphOrGraphExtension, pxContext, isGraph, symbolContext.CancellationToken);

			if (actions == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var pxActionTypeArgs = pxActionType.TypeArguments;

			if (pxActionTypeArgs.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			ITypeSymbol pxActionDacType = pxActionTypeArgs[0];
			ITypeSymbol mainDacType = symbol.ContainingType.IsPXGraph()
				? GetMainDacFromPXGraph(symbol.ContainingType, pxContext, symbolContext.CancellationToken)
				: GetMainDacFromPXGraphExtension(symbol.ContainingType, pxContext, symbolContext.CancellationToken);

			if (mainDacType == null || pxActionDacType.Equals(mainDacType) || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			SyntaxNode symbolSyntax = await symbol.GetSyntaxAsync(symbolContext.CancellationToken).ConfigureAwait(false);
			Location location = GetLocation(symbolSyntax);

			if (location == null)
				return;

			var diagnosticExtraData = new Dictionary<string, string>
			{
				{ DiagnosticProperty.DacName, mainDacType.Name },
				{ DiagnosticProperty.DacMetadataName, mainDacType.GetCLRTypeNameFromType() }
			}.ToImmutableDictionary();
			
			symbolContext.ReportDiagnostic(
				Diagnostic.Create(Descriptors.PX1012_PXActionOnNonPrimaryView, location, diagnosticExtraData,
								  symbol.Name, mainDacType.Name));
		}


		private static IEnumerable<INamedTypeSymbol> GetActionsFromGraphOrGraphExtension(INamedTypeSymbol graphOrGraphExtension, PXContext pxContext,
																						 bool isGraph, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
				return null;

			var actions = isGraph
				? graphOrGraphExtension.GetPXActionsFromGraphOrGraphExtension(pxContext)
				: graphOrGraphExtension.GetPXActionsFromGraphExtensionAndItsBaseGraph(pxContext);

			return actions.Where(action => action.IsGenericType);
		}

		private static ITypeSymbol GetMainDacFromPXGraph(INamedTypeSymbol pxGraphType, PXContext pxContext, CancellationToken cancellationToken)
		{
			if (pxGraphType.BaseType == null)
				return null;

			//var baseGraphType = pxGraphType.GetBaseTypesAndThis()
			//							   .OfType<INamedTypeSymbol>()
			//							   .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type));

			if (baseGraphType != null)  //Case when main DAC is already defined as type parameter
			{
				return baseGraphType.TypeArguments[1];
			}

			INamedTypeSymbol firstView = pxGraphType.GetBaseTypesAndThis()
													.TakeWhile(type => !IsGraphOrGraphExtensionBaseType(type))
													.Reverse()
													.SelectMany(type => type.GetAllViewTypesFromPXGraphOrPXGraphExtension(pxContext))
													.FirstOrDefault();

			if (firstView == null || cancellationToken.IsCancellationRequested)
				return null;

			INamedTypeSymbol baseViewType = firstView.GetBaseTypesAndThis()
													 .OfType<INamedTypeSymbol>()
													 .FirstOrDefault(type => !type.IsCustomBqlCommand(pxContext));

			if (baseViewType?.IsBqlCommand() != true || baseViewType.TypeArguments.Length == 0 || cancellationToken.IsCancellationRequested)
				return null;

			var mainDacType = baseViewType.TypeArguments[0];
			return mainDacType.IsDAC() ? mainDacType : null;
		}

		private static ITypeSymbol GetMainDacFromPXGraphExtension(INamedTypeSymbol pxGraphExtensionType, PXContext pxContext,
																  CancellationToken cancellationToken)
		{
			if (pxGraphExtensionType.BaseType == null)
				return null;

			var graphExtTypeArgs = pxGraphExtensionType.BaseType.TypeArguments;

			if (graphExtTypeArgs.Length == 0 || cancellationToken.IsCancellationRequested)
				return null;

			ITypeSymbol firstTypeArg = graphExtTypeArgs[0];

			if (!(firstTypeArg is INamedTypeSymbol pxGraphType) || !pxGraphType.IsPXGraph())
				return null;

			return GetMainDacFromPXGraph(pxGraphType, pxContext, cancellationToken);
		}

		

		private static bool IsGraphOrGraphExtensionBaseType(ITypeSymbol type)
		{
			string typeNameWithoutGenericArgsCount = type.Name.Split('`')[0];
			return typeNameWithoutGenericArgsCount.Equals(TypeNames.PXGraph, StringComparison.Ordinal) ||
				   typeNameWithoutGenericArgsCount.Equals(TypeNames.PXGraphExtension, StringComparison.Ordinal);
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