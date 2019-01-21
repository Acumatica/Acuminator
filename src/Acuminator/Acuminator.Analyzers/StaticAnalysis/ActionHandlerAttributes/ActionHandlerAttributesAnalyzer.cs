using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1092_MissingAttributesOnActionHandler);

        public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings settings, PXGraphSemanticModel pxGraph)
        {
            foreach (var actionHandler in pxGraph.DeclaredActionHandlers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (actionHandler.Symbol == null || actionHandler.Node == null)
                {
                    continue;
                }

				CheckActionHandler(context, pxContext, actionHandler.Symbol, actionHandler.Node);
            }
        }

		private void CheckActionHandler(SymbolAnalysisContext context, PXContext pxContext,
			IMethodSymbol symbol, MethodDeclarationSyntax node)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var attributes = symbol.GetAttributes();
			var pxUIFieldAttributeType = pxContext.AttributeTypes.PXUIFieldAttribute.Type;
			var pxButtonAttributeType = pxContext.AttributeTypes.PXButtonAttribute;
			var hasPXUIFieldAttribute = false;
			var hasPXButtonAttribute = false;

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
			}

			var fixOption = !hasPXUIFieldAttribute && !hasPXButtonAttribute ? FixOption.AddBothAttributes :
				!hasPXUIFieldAttribute ? FixOption.AddPXUIFieldAttribute :
				FixOption.AddPXButtonAttribute;
			var properties = ImmutableDictionary<string, string>.Empty
				.Add(ActionHandlerAttributesFix.FixOptionKey, fixOption.ToString());
			var diagnostic = Diagnostic.Create(
				Descriptors.PX1092_MissingAttributesOnActionHandler,
				node.Identifier.GetLocation(),
				properties);

			context.ReportDiagnosticWithSuppressionCheck(diagnostic);
		}
    }
}
