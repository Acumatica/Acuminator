using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
{
	public class PXGraphCreateInstanceInEventHandlersAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
									 EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			var walker = new Walker(context);

			methodSyntax?.Accept(walker);
		}

		private class Walker : NestedInvocationWalker
		{
			private const string CreateInstanceMethodName = "CreateInstance";

			private readonly SymbolAnalysisContext _context;

			public Walker(SymbolAnalysisContext context)
				: base(context.Compilation, context.CancellationToken)
			{
				_context = context;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(node);

				if (methodSymbol?.ContainingType?.OriginalDefinition != null 
					&& methodSymbol.ContainingType.OriginalDefinition.IsPXGraph() 
					&& methodSymbol.Name == CreateInstanceMethodName)
				{
					ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers, node);
				}
				else
				{
					base.VisitInvocationExpression(node);
				}
			}

			public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				if (node.Type != null)
				{
					var typeSymbol = GetSymbol<ITypeSymbol>(node.Type);

					if (typeSymbol != null && typeSymbol.IsPXGraph())
					{
						ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1045_PXGraphCreateInstanceInEventHandlers, node);
					}
				}

				base.VisitObjectCreationExpression(node);
			}
		}
	}
}
