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
	public class NoPrimaryViewForPrimaryDacAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1018_NoPrimaryViewForPrimaryDac);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{	
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => 
				AnalyzePXGraphViewsAsync(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzePXGraphViewsAsync(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			if (!(syntaxContext.Node is ClassDeclarationSyntax pxGraphNode) || pxGraphNode.BaseList == null ||
				syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			var pxGraph = syntaxContext.SemanticModel.GetDeclaredSymbol(pxGraphNode, syntaxContext.CancellationToken);

			if (pxGraph == null || !pxGraph.IsPXGraph())
				return;

			ITypeSymbol declaredPrimaryDacType = GetPrimaryDacFromPXGraph(pxGraph, pxContext);

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

		private static ITypeSymbol GetPrimaryDacFromPXGraph(INamedTypeSymbol pxGraph, PXContext pxContext)
		{
			var baseGraphType = pxGraph.GetBaseTypesAndThis()
									   .OfType<INamedTypeSymbol>()
									   .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type)) as INamedTypeSymbol;

			if (baseGraphType == null || baseGraphType.TypeArguments.Length < 2)
				return null;

			ITypeSymbol primaryDacType = baseGraphType.TypeArguments[1];
			return primaryDacType.IsDAC() ? primaryDacType : null;
		}

		private static IEnumerable<ITypeSymbol> GetGraphViewDacTypes(INamedTypeSymbol pxGraph, PXContext pxContext)
		{
			var allViews = pxGraph.GetBaseTypesAndThis()
								  .OfType<INamedTypeSymbol>()
								  .TakeWhile(type => !IsGraphWithPrimaryDacBaseGenericType(type))
								  .SelectMany(type => type.GetAllViewTypesFromPXGraphOrPXGraphExtension(pxContext));

			return allViews.Select(view => GetDacTypeFromView(view, pxContext))
						   .Where(dacType => dacType != null);		
		}

		private static bool IsGraphWithPrimaryDacBaseGenericType(INamedTypeSymbol type) =>
			type.TypeArguments.Length >= 2 && type.Name.Equals(TypeNames.PXGraph, StringComparison.Ordinal);

		private static ITypeSymbol GetDacTypeFromView(INamedTypeSymbol viewType, PXContext pxContext)
		{
			INamedTypeSymbol baseViewType = viewType.GetBaseTypesAndThis()
													.OfType<INamedTypeSymbol>()
													.FirstOrDefault(type => !type.IsCustomBqlCommand(pxContext));

			if (baseViewType?.IsBqlCommand() != true || baseViewType.TypeArguments.Length == 0)
				return null;

			return baseViewType.TypeArguments[0];
		}

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