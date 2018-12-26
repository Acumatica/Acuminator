using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphDeclarationTypeParameter
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXGraphDeclarationTypeParameterAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1093_GraphDeclarationViolation);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeGraphDeclarationTypeParameter(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeGraphDeclarationTypeParameter(SyntaxNodeAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!(context.Node is ClassDeclarationSyntax classDeclaration))
			{
				return;
			}

			var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
			if (typeSymbol == null || !typeSymbol.IsPXGraph(pxContext))
			{
				return;
			}

			if (classDeclaration.BaseList == null)
			{
				return;
			}

			var graphBaseNode = GetBaseGraphTypeNode(context, pxContext, classDeclaration.BaseList.Types);
			if (graphBaseNode == null)
			{
				return;
			}

			// Get last identifier to handle cases like SO.SOSetupMaint
			var graphArgumentIdentifier = graphBaseNode
				.DescendantNodes()
				.OfType<IdentifierNameSyntax>()
				.Last();

			var graphTypeArgument = context.SemanticModel.GetTypeInfo(graphArgumentIdentifier).Type;

			if (typeSymbol.Equals(graphTypeArgument) || graphTypeArgument?.Kind == SymbolKind.TypeParameter)
			{
				return;
			}

			context.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1093_GraphDeclarationViolation, graphArgumentIdentifier.GetLocation()));
		}

		private BaseTypeSyntax GetBaseGraphTypeNode(SyntaxNodeAnalysisContext context, PXContext pxContext,
			SeparatedSyntaxList<BaseTypeSyntax> baseTypes)
		{
			foreach (var typeSyntax in baseTypes)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (typeSyntax?.Type == null)
				{
					continue;
				}

				if (!(context.SemanticModel.GetTypeInfo(typeSyntax.Type).Type is INamedTypeSymbol baseTypeSymbol))
				{
					continue;
				}

				var isGraphBaseType = baseTypeSymbol.ConstructedFrom.Equals(pxContext.PXGraph.GenericTypeGraph) ||
					baseTypeSymbol.ConstructedFrom.Equals(pxContext.PXGraph.GenericTypeGraphDac) ||
					baseTypeSymbol.ConstructedFrom.Equals(pxContext.PXGraph.GenericTypeGraphDacField);
				if (isGraphBaseType)
				{
					return typeSyntax;
				}
			}

			return null;
		}
	}
}
