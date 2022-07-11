#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
    /// <summary>
    /// An expression nodes checker that looks for closures that capture local instance.
    /// </summary>
    internal class CapturedLocalInstancesInExpressionsChecker : CSharpSyntaxWalker
    {
		private readonly PXContext _pxContext;
		private readonly SemanticModel _semanticModel;
		private readonly CancellationToken _cancellation;

		private const int MaxRecursionDepth = 1000;
		private int _recursionDepth = 0;
		private bool _capturedLocalInstance;

		public CapturedLocalInstancesInExpressionsChecker(SemanticModel semanticModel, PXContext pxContext, CancellationToken cancellation)
		{
			_semanticModel = semanticModel.CheckIfNull(nameof(semanticModel));
			_pxContext = pxContext.CheckIfNull(nameof(pxContext));
			_cancellation = cancellation;
		}

		public bool ExpressionCapturesLocalIntanceInClosure(ExpressionSyntax? expression)
		{
			if (expression == null)
				return false;

			if (!_capturedLocalInstance)
				expression.Accept(this);

			return _capturedLocalInstance;
		}

		public override void DefaultVisit(SyntaxNode node)
		{
			if (_capturedLocalInstance)
				return;

			_cancellation.ThrowIfCancellationRequested();

			if (node is ExpressionSyntax expression)
				_capturedLocalInstance = ExpressionCapturesLocalInstance(expression);

			if (!_capturedLocalInstance && _recursionDepth <= MaxRecursionDepth)
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

		private bool ExpressionCapturesLocalInstance(ExpressionSyntax expression)
		{
			switch (expression)
			{
				case AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode:
					DataFlowAnalysis? dfa = _semanticModel.AnalyzeDataFlow(anonMethodOrLambdaNode);
					return dfa != null && dfa.Succeeded && dfa.DataFlowsIn.OfType<IParameterSymbol>().Any(p => p.IsThis);

				case IdentifierNameSyntax identifierName:
					return IdentifierCapturesLocalInstance(identifierName);
				default:
					return false;
			}
		}

		private bool IdentifierCapturesLocalInstance(IdentifierNameSyntax identifierName)
		{
			ISymbol? identifierSymbol = _semanticModel.GetSymbolInfo(identifierName, _cancellation).Symbol;

			if (identifierSymbol?.ContainingType == null || identifierSymbol.IsStatic || !identifierSymbol.ContainingType.IsPXGraphOrExtension(_pxContext))
				return false;

			switch (identifierSymbol.Kind)
			{
				case SymbolKind.Local:
					if (!(identifierSymbol is ILocalSymbol))
						return false;

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
					return true;             // Instance methods, properties, fields and events hold closure

				case SymbolKind.Parameter:    // We can't analyze parameter, so assume that they don't contains delegate with incorrect closures 
				default:
					return false;
			}
		}
	}
}