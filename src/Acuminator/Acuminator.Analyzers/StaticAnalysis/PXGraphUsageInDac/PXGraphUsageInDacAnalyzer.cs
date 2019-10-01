using System.Linq;
using System.Collections.Immutable;
using System.Collections.Generic;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;
using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphUsageInDac
{
    public class PXGraphUsageInDacAnalyzer : DacAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1029_PXGraphUsageInDac
            );

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			SemanticModel semanticModel = context.Compilation.GetSemanticModel(dac.Node.SyntaxTree);

			if (semanticModel == null)
				return;

			GraphUsageInDacWalker walker = new GraphUsageInDacWalker(context, pxContext, semanticModel);
			var membersToVisit = dac.Node.Members.WhereForStructList(s => s.Kind == )


			walker.Visit(dac.Node);
		}

        private class GraphUsageInDacWalker : CSharpSyntaxWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;
            private bool _inDac;

            public GraphUsageInDacWalker(SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
            {
                _context = context;
                _pxContext = pxContext;
				_semanticModel = semanticModel;
			}

	        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
	        {
		        ThrowIfCancellationRequested();

		        INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node, _context.CancellationToken);

				if (symbol == null)
				{
					base.Visit
				}

		        if (symbol != null && symbol.IsDacOrExtension(_pxContext) && !_inDac)
		        {
                    _inDac = true;

                    base.VisitClassDeclaration(node);

                    _inDac = false;
				}
	        }

	        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                ThrowIfCancellationRequested();

                if (node.IsStatic())
                    return;

                base.VisitMethodDeclaration(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
				ThrowIfCancellationRequested();

				SymbolInfo symbolInfo = _semanticModel.GetSymbolInfo(node, _context.CancellationToken);

                if (symbolInfo.Symbol == null || (symbolInfo.Symbol.Kind == SymbolKind.Method && symbolInfo.Symbol.IsStatic))
                    return;

                base.VisitInvocationExpression(node);
            }

            public override void VisitAttributeList(AttributeListSyntax node)
            {
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
				ThrowIfCancellationRequested();

				TypeInfo typeInfo = _semanticModel.GetTypeInfo(node, _context.CancellationToken);

                if (typeInfo.Type == null || !typeInfo.Type.IsPXGraphOrExtension(_pxContext))
                    return;

                _context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDac, node.GetLocation()),
					_pxContext.CodeAnalysisSettings);

                base.VisitIdentifierName(node);
            }

	        private void ThrowIfCancellationRequested()
	        {
				_context.CancellationToken.ThrowIfCancellationRequested();
			}
        }
    }
}
