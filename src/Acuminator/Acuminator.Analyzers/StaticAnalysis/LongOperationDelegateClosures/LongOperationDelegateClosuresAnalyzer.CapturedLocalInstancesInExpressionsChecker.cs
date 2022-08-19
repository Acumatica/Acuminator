#nullable enable

using System;
using System.Linq;
using System.Linq.Expressions;
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

			private CapturedInstancesTypes _capturedInstanceType = CapturedInstancesTypes.None;

			private bool CapturedLocalInstanceFound => _capturedInstanceType != CapturedInstancesTypes.None;
			
			public CapturedLocalInstancesInExpressionsChecker(PassedParametersToNotBeCaptured? outerMethodParametersToNotBeCaptured,
															  SemanticModel semanticModel, PXContext pxContext, CancellationToken cancellation)
			{
				_outerMethodParametersToNotBeCaptured = outerMethodParametersToNotBeCaptured;
				_semanticModel = semanticModel.CheckIfNull(nameof(semanticModel));
				_pxContext = pxContext.CheckIfNull(nameof(pxContext));
				_cancellation = cancellation;
			}

			public CapturedInstancesTypes ExpressionCapturesLocalIntanceInClosure(ExpressionSyntax? expression)
			{
				if (expression == null)
					return CapturedInstancesTypes.None;

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

			private (CapturedInstancesTypes CapturedInstanceType, bool ShouldVisitChildren) ExpressionCapturesLocalInstance(ExpressionSyntax expression)
			{
				switch (expression)
				{
					case AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode:
						{
							CapturedInstancesTypes capturedInstanceType = AnonymousFunctionCapturesSymbol(anonMethodOrLambdaNode);
							return (capturedInstanceType, ShouldVisitChildren: false);
						}
					case IdentifierNameSyntax identifierName:
						{
							CapturedInstancesTypes capturedInstanceType = IdentifierCapturesLocalInstance(identifierName);
							return (capturedInstanceType, ShouldVisitChildren: false);
						}
					case MemberAccessExpressionSyntax memberAccessExpression:
						// Visit element which member is accessed
						memberAccessExpression.Expression.Accept(this);

						// Return the _capturedInstanceType field itself. 
						// It will be result of what visit of memberAccessExpression.Expression subtree found
						return (_capturedInstanceType, ShouldVisitChildren: false);

					case MemberBindingExpressionSyntax:
						// Member binding expression represents type members accessed through conditional access - when accessed instance is not null
						// We always have member binding expression in the conditial access syntax subtree like this: graph?.processor?.MemberFunc
						// We don't need to analyze accessed members, we need only the element being accessed which nodes will be analyzed by other cases of this switch

					case InvocationExpressionSyntax:
						// In case of an invocation expression we have a method call that should return a delegate passed to the method starting long run.
						// Such call itself does not capture graph reference and such cases should be non existant in the codebase
						// Therefore, we won't step in to make a recursive analysis which will be difficult to implement
					
					case LiteralExpressionSyntax:	
						// Case for null passed as argument
						return (CapturedInstanceType: CapturedInstancesTypes.None, ShouldVisitChildren: false);

					default:
						return (CapturedInstanceType: CapturedInstancesTypes.None, ShouldVisitChildren: true);
				}
			}

			private CapturedInstancesTypes AnonymousFunctionCapturesSymbol(AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode)
			{
				DataFlowAnalysis? dfa = _semanticModel.AnalyzeDataFlow(anonMethodOrLambdaNode);

				if (dfa == null || !dfa.Succeeded ||
					(dfa.DataFlowsIn.IsDefaultOrEmpty && dfa.CapturedInside.IsDefaultOrEmpty && dfa.ReadInside.IsDefaultOrEmpty))
				{
					return CapturedInstancesTypes.None;
				}

				var capturedSymbols = dfa.DataFlowsIn
										 .Concat(dfa.CapturedInside)
										 .ConcatStructList(dfa.ReadInside)
										 .OfType<IParameterSymbol>()
										 .Distinct();

				foreach (IParameterSymbol symbol in capturedSymbols)
				{
					CapturedInstancesTypes capturedInstanceType = GetCapturedSymbolType(symbol);

					if (capturedInstanceType != CapturedInstancesTypes.None)
						return capturedInstanceType;
				}

				return CapturedInstancesTypes.None;
			}

			private CapturedInstancesTypes GetCapturedSymbolType(IParameterSymbol symbol)
			{
				if (symbol.IsThis)
				{
					return TypeMemberAccessCapturesGraph(symbol.Type)
						? CapturedInstancesTypes.PXGraph
						: CapturedInstancesTypes.None;
				}

				var nonCapturableParameter = FindNonCapturableParameterPassedToMethod(symbol.Name);
				return nonCapturableParameter?.CapturedTypes ?? CapturedInstancesTypes.None;
			}

			private CapturedInstancesTypes IdentifierCapturesLocalInstance(IdentifierNameSyntax identifierName)
			{
				ISymbol? identifierSymbol = _semanticModel.GetSymbolInfo(identifierName, _cancellation).Symbol;

				if (identifierSymbol == null || identifierSymbol.IsStatic)
					return CapturedInstancesTypes.None;

				switch (identifierSymbol.Kind)
				{
					case SymbolKind.Local:
						if (!(identifierSymbol is ILocalSymbol))
							return CapturedInstancesTypes.None;

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
						// Instance methods, properties, fields and events hold closure
						return TypeMemberAccessCapturesGraph(identifierSymbol.ContainingType)
							? CapturedInstancesTypes.PXGraph
							: CapturedInstancesTypes.None;      

					case SymbolKind.Parameter:
						var nonCapturableParameter = FindNonCapturableParameterPassedToMethod(identifierSymbol.Name);
						return nonCapturableParameter?.CapturedTypes ?? CapturedInstancesTypes.None;

					default:
						return CapturedInstancesTypes.None;
				}
			}

			private PassedParameter? FindNonCapturableParameterPassedToMethod(string parameterName) =>
				_outerMethodParametersToNotBeCaptured?.Count > 0
					? _outerMethodParametersToNotBeCaptured.GetPassedParameter(parameterName)
					: null;

			private bool TypeMemberAccessCapturesGraph(ITypeSymbol? type)
			{
				if (type == null)
					return false;

				return type.IsPXGraphOrExtension(_pxContext) || type.IsCustomBqlCommand(_pxContext);
			}
		}
	}
}