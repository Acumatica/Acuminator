#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Walkers
{
	/// <summary>
	/// The delegates walker base class that has some common logic for analysis of delegate expressions.
	/// </summary>
	public abstract class DelegatesWalkerBase : NestedInvocationWalker
	{
		protected DelegatesWalkerBase(Compilation compilation, CancellationToken cancellationToken, CodeAnalysisSettings? codeAnalysisSettings,
									  Func<IMethodSymbol, bool>? bypassMethod = null) :
								 base(compilation, cancellationToken, codeAnalysisSettings, bypassMethod)
		{
		}

		/// <summary>
		/// Gets delegate symbol and node from the <paramref name="delegateExpression"/>.
		/// </summary>
		/// <param name="delegateExpression">The delegate expression node.</param>
		/// <returns>
		/// The delegate expression symbol and node.
		/// </returns>
		protected (ISymbol? DelegateSymbol, SyntaxNode? DelegateNode) GetDelegateSymbolAndNode(ExpressionSyntax delegateExpression)
		{
			delegateExpression.ThrowOnNull(nameof(delegateExpression));
			ThrowIfCancellationRequested();

			switch (delegateExpression)
			{
				case AnonymousFunctionExpressionSyntax anonymousFunction:
					{
						var delegateNode = anonymousFunction.Body;
						var delegateSymbol = GetSemanticModel(delegateNode.SyntaxTree)
												?.GetSymbolInfo(anonymousFunction, CancellationToken).Symbol;

						return (delegateSymbol, delegateNode);
					}
				case CastExpressionSyntax castExpression:
					{
						return GetDelegateSymbolAndNode(castExpression.Expression);
					}
				case ObjectCreationExpressionSyntax objectCreationExpression:
					{
						if (objectCreationExpression.ArgumentList.Arguments.Count != 1)
							return default;

						ArgumentSyntax delegateCreationArg = objectCreationExpression.ArgumentList.Arguments[0];
						return GetDelegateSymbolAndNode(delegateCreationArg.Expression);
					}
				default:
					{
						var delegateSymbol = GetSymbol<ISymbol>(delegateExpression);
						var delegateNode = delegateSymbol?.DeclaringSyntaxReferences
														  .FirstOrDefault()
														 ?.GetSyntax(CancellationToken);
						return (delegateSymbol, delegateNode);
					}
			}
		}

		/// <summary>
		/// Gets delegate syntax node from the <paramref name="delegateExpression"/>.
		/// </summary>
		/// <param name="delegateExpression">The delegate expression node.</param>
		/// <returns>
		/// The delegate syntax node.
		/// </returns>
		protected SyntaxNode? GetDelegateNode(ExpressionSyntax delegateExpression)
		{
			delegateExpression.ThrowOnNull(nameof(delegateExpression));
			ThrowIfCancellationRequested();

			switch (delegateExpression)
			{
				case AnonymousFunctionExpressionSyntax anonymousFunction:
					return anonymousFunction.Body;

				case CastExpressionSyntax castExpression:
					return GetDelegateNode(castExpression.Expression);

				case ObjectCreationExpressionSyntax objectCreationExpression:
					if (objectCreationExpression.ArgumentList.Arguments.Count != 1)
						return null;

					ArgumentSyntax delegateCreationArg = objectCreationExpression.ArgumentList.Arguments[0];
					return GetDelegateNode(delegateCreationArg.Expression);

				default:
					var delegateSymbol = GetSymbol<ISymbol>(delegateExpression);

					return delegateSymbol?.DeclaringSyntaxReferences
										  .FirstOrDefault()
										 ?.GetSyntax(CancellationToken);
			}
		}
	}
}
