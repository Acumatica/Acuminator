#nullable enable

using System;
using System.Linq;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	public partial class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
	{
		/// <summary>
		/// Values that represent captured instance types.
		/// </summary>
		private enum CapturedInstanceType
		{
			/// <summary>
			/// Captured instance is PXGraph.
			/// </summary>
			PXGraph,

			/// <summary>
			/// Captured instance is PXAdapter.
			/// </summary>
			PXAdapter
		}


		/// <summary>
		/// An expression nodes checker that looks for closures that capture local instance.
		/// </summary>
		private class CapturedLocalInstancesInExpressionsChecker : CSharpSyntaxWalker
		{
			private readonly PXContext _pxContext;
			private readonly SemanticModel _semanticModel;
			private readonly CancellationToken _cancellation;

			private const int MaxRecursionDepth = 1000;
			private int _recursionDepth = 0;

			private readonly PassedParametersToNotBeCaptured? _outerMethodParametersToNotBeCaptured;

			private CapturedInstanceType? _capturedInstanceType;

			private bool CapturedLocalInstanceFound => _capturedInstanceType.HasValue;
			
			public CapturedLocalInstancesInExpressionsChecker(PassedParametersToNotBeCaptured? outerMethodParametersToNotBeCaptured,
															  SemanticModel semanticModel, PXContext pxContext, CancellationToken cancellation)
			{
				_outerMethodParametersToNotBeCaptured = outerMethodParametersToNotBeCaptured;
				_semanticModel = semanticModel.CheckIfNull(nameof(semanticModel));
				_pxContext = pxContext.CheckIfNull(nameof(pxContext));
				_cancellation = cancellation;
			}

			public CapturedInstanceType? ExpressionCapturesLocalIntanceInClosure(ExpressionSyntax? expression)
			{
				if (expression == null)
					return null;

				if (!CapturedLocalInstanceFound)
					expression.Accept(this);

				return _capturedInstanceType;
			}

			public override void DefaultVisit(SyntaxNode node)
			{
				if (CapturedLocalInstanceFound)
					return;

				_cancellation.ThrowIfCancellationRequested();
				bool shouldVisitChildren = true;

				if (node is ExpressionSyntax expression)
					(_capturedInstanceType, shouldVisitChildren) = ExpressionCapturesLocalInstance(expression);

				if (!CapturedLocalInstanceFound && shouldVisitChildren && _recursionDepth <= MaxRecursionDepth)
				{
					try
					{
						_recursionDepth++;
						base.DefaultVisit(node);
					}
					finally
					{
						_recursionDepth--;
					}
				}
			}

			private (CapturedInstanceType? CapturedInstanceType, bool ShouldVisitChildren) ExpressionCapturesLocalInstance(ExpressionSyntax expression)
			{
				switch (expression)
				{
					case AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode:
						{
							CapturedInstanceType? capturedInstanceType = AnonymousFunctionCapturesSymbol(anonMethodOrLambdaNode);
							return (capturedInstanceType, ShouldVisitChildren: false);
						}
					case IdentifierNameSyntax identifierName:
						{
							CapturedInstanceType? capturedInstanceType = IdentifierCapturesLocalInstance(identifierName);
							return (capturedInstanceType, ShouldVisitChildren: false);
						}
					default:
						return (CapturedInstanceType: null, ShouldVisitChildren: true);
				}
			}

			private CapturedInstanceType? AnonymousFunctionCapturesSymbol(AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode)
			{
				DataFlowAnalysis? dfa = _semanticModel.AnalyzeDataFlow(anonMethodOrLambdaNode);

				if (dfa == null || !dfa.Succeeded || dfa.DataFlowsIn.IsDefaultOrEmpty)
					return null;

				foreach (IParameterSymbol symbol in dfa.DataFlowsIn.OfType<IParameterSymbol>())
				{
					CapturedInstanceType? capturedInstanceType = GetCapturedSymbolType(symbol);

					if (capturedInstanceType.HasValue)
						return capturedInstanceType;
				}

				return null;
			}

			private CapturedInstanceType? GetCapturedSymbolType(IParameterSymbol symbol)
			{
				if (symbol.IsThis)
				{
					return symbol.Type.IsPXGraphOrExtension(_pxContext)
						? CapturedInstanceType.PXGraph
						: null;
				}

				bool isCapturedNonCapturableParameter = _outerMethodParametersToNotBeCaptured?.PassedInstancesCount > 0
					? _outerMethodParametersToNotBeCaptured.Contains(symbol.Name)
					: false;

				if (!isCapturedNonCapturableParameter)
					return null;

				// We add to PassedInstances only graphs and adapters that should not be captured.
				// Therefore, we can only check if the symbol type here is PXAdapter to understand what CapturedInstanceType should be returned
				return symbol.Type.InheritsFrom(_pxContext.PXAdapterType)
					? CapturedInstanceType.PXAdapter
					: CapturedInstanceType.PXGraph;
			}

			private CapturedInstanceType? IdentifierCapturesLocalInstance(IdentifierNameSyntax identifierName)
			{
				ISymbol? identifierSymbol = _semanticModel.GetSymbolInfo(identifierName, _cancellation).Symbol;

				if (identifierSymbol?.ContainingType == null || identifierSymbol.IsStatic || !identifierSymbol.ContainingType.IsPXGraphOrExtension(_pxContext))
					return null;

				switch (identifierSymbol.Kind)
				{
					case SymbolKind.Local:
						if (!(identifierSymbol is ILocalSymbol))
							return null;

						var localVariableDeclarator = identifierSymbol.DeclaringSyntaxReferences
																	  .FirstOrDefault()
																	 ?.GetSyntax(_cancellation) as VariableDeclaratorSyntax;

						// Check variable declaration to investigated assigned values.
						// We do not check for assignments to the variable done after the declaration since this case is both difficult to analyze and very rare.
						return ExpressionCapturesLocalIntanceInClosure(localVariableDeclarator?.Initializer?.Value);

					case SymbolKind.Method:
					case SymbolKind.Property:
					case SymbolKind.Event:
					case SymbolKind.Field:
						return CapturedInstanceType.PXGraph;      // Instance methods, properties, fields and events hold closure

					case SymbolKind.Parameter:    
						// Parameter here is a parameter of the outer method passed to the StartLongOperation or SetProcessDelegate which means its a delegate
						// We can't analyze arbitrary delegates, so assume that they don't contain delegate with incorrect closures 
					default:
						return null;
				}
			}
		}
	}
}