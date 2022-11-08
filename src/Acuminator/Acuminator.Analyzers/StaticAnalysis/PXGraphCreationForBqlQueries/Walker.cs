#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

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

				if (node.ArgumentList?.Arguments.Count is null or 0)
				{
					base.VisitInvocationExpression(node);
					return;
				}

				var methodSymbol = _semanticModel.GetSymbolOrFirstCandidate(node, _cancellation) as IMethodSymbol;

				if (IsBqlSelectOrSearch(methodSymbol))
				{
					var bqlCallGraphArg = node.ArgumentList.Arguments[0].Expression;

					if (!ArgumentIsPropertyOrField(bqlCallGraphArg))
						_graphArguments.Add(bqlCallGraphArg);
				}

				base.VisitInvocationExpression(node);
			}

			private bool IsBqlSelectOrSearch(IMethodSymbol? methodSymbol)
			{
				// Check BQL Select / Search methods by name because
				// variations of these methods are declared in different PXSelectBase-derived classes
				var declaringType = methodSymbol?.ContainingType?.OriginalDefinition;

				return declaringType != null && declaringType.IsBqlCommand(_pxContext) &&
					   !methodSymbol!.Parameters.IsEmpty && methodSymbol.Parameters[0].Type.IsPXGraph(_pxContext) &&
						(methodSymbol.Name.StartsWith(SelectMethodName, StringComparison.Ordinal) ||
						 methodSymbol.Name.StartsWith(SearchMethodName, StringComparison.Ordinal));
			}

			private bool ArgumentIsPropertyOrField(ExpressionSyntax bqlCallGraphArg)
			{
				if (bqlCallGraphArg is IdentifierNameSyntax identifier)
					return ArgumentIsPropertyOrField(identifier);

				// analyze complex expressions passed as argument
				return bqlCallGraphArg.DescendantNodes()
									  .OfType<IdentifierNameSyntax>()
									  .Any(ArgumentIsPropertyOrField);
			}

			private bool ArgumentIsPropertyOrField(IdentifierNameSyntax identifier)
			{
				var graphArgSymbol = _semanticModel.GetSymbolOrFirstCandidate(identifier, _cancellation);
				return graphArgSymbol is IPropertySymbol or IFieldSymbol;
			}
		}
	}
}
