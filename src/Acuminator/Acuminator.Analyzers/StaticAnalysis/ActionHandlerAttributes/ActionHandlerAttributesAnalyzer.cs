using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.ActionHandlerAttributes
{
	public class ActionHandlerAttributesAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		internal const string FixOptionKey = nameof(FixOptionKey);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1092_MissingAttributesOnActionHandler);

        public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraph)
        {
            foreach (var actionHandler in pxGraph.DeclaredActionHandlers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (actionHandler.Symbol == null || actionHandler.Node == null)
                {
                    continue;
                }

				CheckActionHandler(context, pxContext, actionHandler.Symbol, actionHandler.Node, pxGraph.Type);
            }
        }

		private void CheckActionHandler(SymbolAnalysisContext context, PXContext pxContext, IMethodSymbol symbol, MethodDeclarationSyntax node,
										GraphType graphType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var attributes = symbol.GetAttributes();
			var pxUIFieldAttributeType = pxContext.AttributeTypes.PXUIFieldAttribute.Type;
			var pxButtonAttributeType = pxContext.AttributeTypes.PXButtonAttribute;
			var pxOverrideAttributeType = pxContext.AttributeTypes.PXOverrideAttribute;
			var hasPXUIFieldAttribute = false;
			var hasPXButtonAttribute = false;
			var hasPXOverrideAttribute = false;

			foreach (var attr in attributes)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (attr.AttributeClass == null)
				{
					continue;
				}

				if (attr.AttributeClass.InheritsFromOrEquals(pxUIFieldAttributeType))
				{
					hasPXUIFieldAttribute = true;
				}

				if (attr.AttributeClass.InheritsFromOrEquals(pxButtonAttributeType))
				{
					hasPXButtonAttribute = true;
				}

				if (hasPXUIFieldAttribute && hasPXButtonAttribute)
				{
					return;
				}

				if (attr.AttributeClass.InheritsFromOrEquals(pxOverrideAttributeType))
				{
					hasPXOverrideAttribute = true;
				}

				if (graphType == GraphType.PXGraphExtension && hasPXOverrideAttribute)
				{
					return;
				}
			}

			var fixOption = !hasPXUIFieldAttribute && !hasPXButtonAttribute ? FixOption.AddBothAttributes :
				!hasPXUIFieldAttribute ? FixOption.AddPXUIFieldAttribute :
				FixOption.AddPXButtonAttribute;
			var properties = ImmutableDictionary<string, string>.Empty
				.Add(FixOptionKey, fixOption.ToString());
			var diagnostic = Diagnostic.Create(
				Descriptors.PX1092_MissingAttributesOnActionHandler,
				node.Identifier.GetLocation(),
				properties);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
    }
}
