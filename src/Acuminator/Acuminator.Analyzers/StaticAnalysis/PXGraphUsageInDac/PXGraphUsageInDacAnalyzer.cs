using System.Linq;
using System.Threading;
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

			// Analyze only DAC members, avoid nested types analysis. For possible nested DACs this analyser will be executed separately. 
			// We do not want to analyse possible internal helper classes or analyze DAC's attributes, ID, lis of base classes and so on.
			var membersToVisit = dac.Node.Members.Where(member => !member.IsKind(SyntaxKind.ClassDeclaration) && 
																  !member.IsKind(SyntaxKind.StructDeclaration) &&
																  !member.IsKind(SyntaxKind.InterfaceDeclaration));

			foreach (MemberDeclarationSyntax memberNode in membersToVisit)
			{
				walker.Visit(memberNode);
			}
		}

        private class GraphUsageInDacWalker : CSharpSyntaxWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;

            public GraphUsageInDacWalker(SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
            {
                _context = context;
                _pxContext = pxContext;
				_semanticModel = semanticModel;
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

			public override void VisitBaseList(BaseListSyntax node)
			{
			}

			public override void VisitClassDeclaration(ClassDeclarationSyntax node)
			{
			}

			public override void VisitAttributeList(AttributeListSyntax node)
            {
            }

	        private void ThrowIfCancellationRequested()
	        {
				_context.CancellationToken.ThrowIfCancellationRequested();
			}
        }
    }
}
