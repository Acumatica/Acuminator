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
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;


namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NoPrimaryViewForPrimaryDacAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1018_NoPrimaryViewForPrimaryDac);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{	
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => 
				AnalyzePXGraphViews(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzePXGraphViews(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (!(syntaxContext.Node is ClassDeclarationSyntax pxGraphNode) || pxGraphNode.BaseList == null ||
				syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			var pxGraph = syntaxContext.SemanticModel.GetDeclaredSymbol(pxGraphNode, syntaxContext.CancellationToken);

			if (pxGraph == null || !pxGraph.IsPXGraph())
				return;

			ITypeSymbol declaredPrimaryDacType = pxGraph.GetDeclaredPrimaryDacFromGraphOrGraphExtension(pxContext);

			if (declaredPrimaryDacType == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			var graphViewsDacTypes = GetGraphViewDacTypes(pxGraph, pxContext);

			if (graphViewsDacTypes.Contains(declaredPrimaryDacType) || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			Location location = GetLocation(pxGraphNode, declaredPrimaryDacType, syntaxContext);

			if (location == null)
				return;

			syntaxContext.ReportDiagnostic(
				Diagnostic.Create(Descriptors.PX1018_NoPrimaryViewForPrimaryDac, location));
		}

		private static IEnumerable<ITypeSymbol> GetGraphViewDacTypes(INamedTypeSymbol pxGraph, PXContext pxContext) =>
			pxGraph.GetViewsFromPXGraph(pxContext)
				   .Select(view => view.GetDacFromView(pxContext))
				   .Where(dacType => dacType != null);		

		private static Location GetLocation(ClassDeclarationSyntax pxGraphNode, ITypeSymbol declaredPrimaryDacType,
											SyntaxNodeAnalysisContext syntaxContext)
		{
			foreach (BaseTypeSyntax baseTypeNode in pxGraphNode.BaseList.Types)
			{
				if (!(baseTypeNode.Type is GenericNameSyntax genericBaseType))
					continue;

				INamedTypeSymbol baseTypeSymbol =
					syntaxContext.SemanticModel.GetSymbolInfo(genericBaseType, syntaxContext.CancellationToken).Symbol as INamedTypeSymbol;

				if (baseTypeSymbol?.IsPXGraph() == true)
				{
					if (baseTypeSymbol.TypeArguments.Length >= 2)
					{
						int indexOfDac = baseTypeSymbol.TypeArguments.IndexOf(declaredPrimaryDacType);

						if (indexOfDac >= 0)
						{
							return genericBaseType.TypeArgumentList.Arguments[indexOfDac].GetLocation();
						}
					}

					break;
				}
			}

			return pxGraphNode.Identifier.GetLocation();
		}
	}
}