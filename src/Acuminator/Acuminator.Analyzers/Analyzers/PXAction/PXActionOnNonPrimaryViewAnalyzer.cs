using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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
            compilationStartContext.RegisterSymbolAction(c => AnalyzePXActionSymbol(c, pxContext), SymbolKind.Field, SymbolKind.Property);
        }

		private static void AnalyzePXActionSymbol(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (!IsDiagnosticValidForSymbol(symbolContext, pxContext))
				return;

			INamedTypeSymbol pxActionType = GetPXActionType(symbolContext);

			if (pxActionType == null || !pxActionType.IsGenericType)
				return;

			var pxActionTypeArgs = pxActionType.TypeArguments;

			if (pxActionTypeArgs.Length == 0 || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var pxActionDacType = pxActionTypeArgs[0];


           

           
                
            symbolContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1010_StartRowResetForPaging, selectInvocation.GetLocation()));

        }

		private static bool IsDiagnosticValidForSymbol(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			if (symbolContext.Symbol?.ContainingType == null || symbolContext.CancellationToken.IsCancellationRequested)
				return false;

			INamedTypeSymbol containingType = symbolContext.Symbol.ContainingType;
			return containingType.InheritsFrom(pxContext.PXGraphType) || 
				   containingType.InheritsFrom(pxContext.PXGraphExtensionType); 
		}

		private static INamedTypeSymbol GetPXActionType(SymbolAnalysisContext symbolContext)
		{
			if (symbolContext.CancellationToken.IsCancellationRequested)
				return null;

			switch (symbolContext.Symbol)
			{
				case IFieldSymbol fieldSymbol when fieldSymbol.Type.IsPXAction():
					return fieldSymbol.Type as INamedTypeSymbol;
				case IPropertySymbol propertySymbol when propertySymbol.Type.IsPXAction():
					return propertySymbol.Type as INamedTypeSymbol;
				default:
					return null;
			}
		}

		private static ITypeSymbol GetMainDacFromPXGraph(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			INamedTypeSymbol pxGraphType = symbolContext.Symbol.ContainingType;
			var graphTypeArgs = pxGraphType.TypeArguments;
			
			if (pxGraphType.IsGenericType && graphTypeArgs.Length >= 2)  //Case when main DAC is already defined as type parameter
			{
				return graphTypeArgs[1];
			}



		}
	}
}