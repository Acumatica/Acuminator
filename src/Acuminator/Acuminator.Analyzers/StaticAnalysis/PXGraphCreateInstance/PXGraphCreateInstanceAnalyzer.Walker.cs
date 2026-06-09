using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
{
	public partial class PXGraphCreateInstanceAnalyzer
	{
		private class Walker : CSharpSyntaxWalker
		{
			private readonly SyntaxNodeAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;

			private static readonly DiagnosticDescriptor _px1001Descriptor = Descriptors.PX1001_PXGraphCreateInstance;
			private static readonly DiagnosticDescriptor _px1003Descriptor = Descriptors.PX1003_BasePXGraphCreateInstance;

			public Walker(SyntaxNodeAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
			{
				_context = context;
				_pxContext = pxContext;
				_semanticModel = semanticModel;
			}

			public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				if (node.Type == null || _semanticModel.GetSymbolOrBestCandidate(node.Type, _context.CancellationToken) is not ITypeSymbol typeSymbol)
				{
					base.VisitObjectCreationExpression(node);
					return;
				}

				DiagnosticDescriptor? descriptor = GetDiagnosticDescriptor(typeSymbol);

				if (descriptor != null)
				{
					_context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, node.GetLocation()),
						_pxContext.CodeAnalysisSettings);
				}
				else
				{
					base.VisitObjectCreationExpression(node);
				}
			}

			public override void VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				var constructor = _semanticModel.GetSymbolOrBestCandidate(node, _context.CancellationToken) as IMethodSymbol;

				if (constructor?.ContainingType == null || constructor.MethodKind != MethodKind.Constructor) 
				{
					base.VisitImplicitObjectCreationExpression(node);
					return;
				}

				DiagnosticDescriptor? descriptor = GetDiagnosticDescriptor(constructor.ContainingType);

				if (descriptor != null)
				{
					_context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, node.GetLocation()),
						_pxContext.CodeAnalysisSettings);
				}
				else
				{
					base.VisitImplicitObjectCreationExpression(node);
				}
			}

			private DiagnosticDescriptor? GetDiagnosticDescriptor(ITypeSymbol typeSymbol)
			{
				if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.IsPXGraph(_pxContext))
				{
					return _px1001Descriptor;
				}
				else if (typeSymbol.InheritsFrom(_pxContext.PXGraph.Type))
				{
					return _px1001Descriptor;
				}
				else if (typeSymbol.Equals(_pxContext.PXGraph.Type, SymbolEqualityComparer.Default))
				{
					return _px1003Descriptor;
				}

				return null;
			}

			public override void DefaultVisit(SyntaxNode node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();
				base.DefaultVisit(node);
			}
		}
	}
}
