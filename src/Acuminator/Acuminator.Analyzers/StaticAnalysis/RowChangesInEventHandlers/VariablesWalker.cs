using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer
	{
		/// <summary>
		/// Collects all variables that are declared inside the method, and assigned with <code>e.Row</code>
		/// </summary>
		private class VariablesWalker : CSharpSyntaxWalker
		{
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private CancellationToken _cancellationToken;
			private readonly ImmutableHashSet<ILocalSymbol> _variables;
			private readonly EventArgsRowWalker _eventArgsRowWalker;

			private readonly ISet<ILocalSymbol> _result = new HashSet<ILocalSymbol>();
			public ImmutableArray<ILocalSymbol> Result => _result.ToImmutableArray();

			public VariablesWalker(MethodDeclarationSyntax methodSyntax, SemanticModel semanticModel, PXContext pxContext,
				CancellationToken cancellationToken)
			{
				methodSyntax.ThrowOnNull(nameof (methodSyntax));
				semanticModel.ThrowOnNull(nameof (semanticModel));
				pxContext.ThrowOnNull(nameof (pxContext));

				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_cancellationToken = cancellationToken;

				_eventArgsRowWalker = new EventArgsRowWalker(semanticModel, pxContext);

				if (methodSyntax.Body != null || methodSyntax.ExpressionBody?.Expression != null)
				{
					var dataFlow = methodSyntax.Body != null
						? semanticModel.AnalyzeDataFlow(methodSyntax.Body)
						: semanticModel.AnalyzeDataFlow(methodSyntax.ExpressionBody.Expression);

					if (dataFlow.Succeeded)
					{
						_variables = dataFlow.WrittenInside
							.Intersect(dataFlow.VariablesDeclared)
							.OfType<ILocalSymbol>()
							.ToImmutableHashSet();
					}
				}
				
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignment)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (assignment.Left is IdentifierNameSyntax variableNode && assignment.Right != null)
				{
					var variableSymbol = _semanticModel.GetSymbolInfo(variableNode, _cancellationToken).Symbol as ILocalSymbol;
					ValidateThatVariableIsSetToDacFromEvent(variableSymbol, assignment.Right);
				}
			}

			public override void VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				foreach (var variableDeclarator in variableDeclaration.Variables.Where(v => v.Initializer?.Value != null))
				{
					var variableSymbol = _semanticModel.GetDeclaredSymbol(variableDeclarator, _cancellationToken) as ILocalSymbol;
					ValidateThatVariableIsSetToDacFromEvent(variableSymbol, variableDeclarator.Initializer.Value);
				}
			}

			public override void VisitIsPatternExpression(IsPatternExpressionSyntax isPatternExpression)
			{
				_cancellationToken.ThrowIfCancellationRequested();

				if (isPatternExpression.Pattern is DeclarationPatternSyntax declarationPattern && declarationPattern.Designation != null)
				{
					var variableSymbol = _semanticModel.GetDeclaredSymbol(declarationPattern.Designation, _cancellationToken) as ILocalSymbol;
					ValidateThatVariableIsSetToDacFromEvent(variableSymbol, isPatternExpression.Expression);
				}
			}

			private void ValidateThatVariableIsSetToDacFromEvent(ILocalSymbol variableSymbol, ExpressionSyntax variableInitializerExpression)
			{
				if (variableSymbol == null || !_variables.Contains(variableSymbol))
					return;

				_eventArgsRowWalker.Reset();
				variableInitializerExpression.Accept(_eventArgsRowWalker);

				if (_eventArgsRowWalker.Success)
					_result.Add(variableSymbol);
			}
		}
	}
}
