#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries
{
	public partial class PXGraphCreationForBqlQueriesAnalyzer
	{
		/// <summary>
		/// Collects all PXGraph arguments from BQL Select / Search invocations.
		/// </summary>
		private class BqlGraphArgWalker : CSharpSyntaxWalker
		{
			private const string SelectMethodName = "Select";
			private const string SearchMethodName = "Search";

			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private readonly CancellationToken _cancellation;

			private readonly List<ExpressionSyntax> _graphArguments = new List<ExpressionSyntax>();
			public ImmutableArray<ExpressionSyntax> GraphArguments => _graphArguments.ToImmutableArray();

			public BqlGraphArgWalker(SemanticModel semanticModel, PXContext pxContext, CancellationToken cancellation)
			{				
				_semanticModel = semanticModel.CheckIfNull(nameof(semanticModel));
				_pxContext = pxContext.CheckIfNull(nameof(pxContext));
				_cancellation = cancellation;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_cancellation.ThrowIfCancellationRequested();

				var methodSymbol = _semanticModel.GetSymbolOrFirstCandidate(node, _cancellation) as IMethodSymbol;

				// Check BQL Select / Search methods by name because
				// variations of these methods are declared in different PXSelectBase-derived classes
				var declaringType = methodSymbol?.ContainingType?.OriginalDefinition;

				if (declaringType != null && declaringType.IsBqlCommand(_pxContext) 
					&& !methodSymbol!.Parameters.IsEmpty && methodSymbol.Parameters[0].Type.IsPXGraph(_pxContext) 
					&& node.ArgumentList.Arguments.Count > 0 &&
				    (methodSymbol.Name.StartsWith(SelectMethodName, StringComparison.Ordinal) ||
				     methodSymbol.Name.StartsWith(SearchMethodName, StringComparison.Ordinal)))
				{
					var graphArg = node.ArgumentList.Arguments[0].Expression;
					_graphArguments.Add(graphArg);
				}
			}
		}
	}
}
