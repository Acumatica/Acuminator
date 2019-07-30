using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac
{
	public class MethodsUsageInDacAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1031_DacCannotContainInstanceMethods,
				Descriptors.PX1032_DacPropertyCannotContainMethodInvocations
			);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			SemanticModel semanticModel = context.Compilation.GetSemanticModel(dac.Node.SyntaxTree);

			if (semanticModel == null)
				return;

			foreach (MethodDeclarationSyntax method in dac.GetMemberNodes<MethodDeclarationSyntax>())
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				AnalyzeMethodDeclarationInDac(method, context, pxContext);
			}

			HashSet<INamedTypeSymbol> whiteList = GetWhitelist(pxContext);
			
			foreach (DacPropertyInfo property in dac.DeclaredProperties)
			{
				AnalyzeMethodInvocationInDacProperty(property, whiteList, context, pxContext, semanticModel);
			}
		}	

        private HashSet<INamedTypeSymbol> GetWhitelist(PXContext pxContext)
        {
            return new HashSet<INamedTypeSymbol>
            {
                pxContext.SystemTypes.Bool,
                pxContext.SystemTypes.Byte,
                pxContext.SystemTypes.Int16,
                pxContext.SystemTypes.Int32,
                pxContext.SystemTypes.Int64,
                pxContext.SystemTypes.Decimal,
                pxContext.SystemTypes.Float,
                pxContext.SystemTypes.Double,
                pxContext.SystemTypes.String,
                pxContext.SystemTypes.Guid,
                pxContext.SystemTypes.DateTime,
                pxContext.SystemTypes.TimeSpan,
                pxContext.SystemTypes.Enum,
                pxContext.SystemTypes.Nullable
            };
        }

		private void AnalyzeMethodDeclarationInDac(MethodDeclarationSyntax method, SymbolAnalysisContext context, PXContext pxContext)
		{
			if (!method.IsStatic())
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1031_DacCannotContainInstanceMethods, method.Identifier.GetLocation()),
					pxContext.CodeAnalysisSettings);
			}
		}

		private void AnalyzeMethodInvocationInDacProperty(DacPropertyInfo property, HashSet<INamedTypeSymbol> whiteList,
														  SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
		{
			foreach (SyntaxNode node in property.Node.DescendantNodes())
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (node is InvocationExpressionSyntax invocation)
				{
					ISymbol symbol = semanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol;

                    if (symbol == null || !(symbol is IMethodSymbol method) || method.IsStatic)
                        continue;

					bool inWhitelist = whiteList.Contains(method.ContainingType) ||
									   whiteList.Contains(method.ContainingType.ConstructedFrom);
                    if (inWhitelist)
                        continue;

                    context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations, invocation.GetLocation()),
						pxContext.CodeAnalysisSettings);
                }
				else if (node is ObjectCreationExpressionSyntax)
				{
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1032_DacPropertyCannotContainMethodInvocations, node.GetLocation()),
						pxContext.CodeAnalysisSettings);
				}
			}
		}	
	}
}