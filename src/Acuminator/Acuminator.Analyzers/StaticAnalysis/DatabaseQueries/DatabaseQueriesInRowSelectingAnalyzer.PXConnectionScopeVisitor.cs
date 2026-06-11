using System;
using System.Threading;

using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	public partial class DatabaseQueriesInRowSelectingAnalyzer
	{
		private class PXConnectionScopeVisitor : CSharpSyntaxVisitor<bool>
		{
			private readonly DiagnosticWalker _parent;
			private readonly PXContext _pxContext;
			private readonly Func<SyntaxTree, SemanticModel?> _semanticModelGetter;
			private readonly CancellationToken _cancellation;

			public PXConnectionScopeVisitor(DiagnosticWalker parent, PXContext pxContext, Func<SyntaxTree, SemanticModel?> semanticModelGetter,
											CancellationToken cancellation)
			{
				_parent 			 = parent;
				_pxContext 	   		 = pxContext;
				_semanticModelGetter = semanticModelGetter;
				_cancellation  		 = cancellation;
			}

			public override bool VisitUsingStatement(UsingStatementSyntax node)
			{
				return (node.Declaration?.Accept(this) ?? false) || (node.Expression?.Accept(this) ?? false);
			}

			public override bool VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				var semanticModel = _semanticModelGetter(node.SyntaxTree);
				var typeSymbol = semanticModel?.GetSymbolOrBestCandidate(node.Type, _cancellation) as ITypeSymbol;
				return IsPXConnectionScope(typeSymbol);
			}

			public override bool VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
			{
				var semanticModel = _semanticModelGetter(node.SyntaxTree);
				var constructor = semanticModel?.GetSymbolOrBestCandidate(node, _cancellation) as IMethodSymbol;

				if (constructor?.ContainingType == null || constructor.MethodKind != MethodKind.Constructor)
					return false;

				return IsPXConnectionScope(constructor.ContainingType);
			}

			private bool IsPXConnectionScope(ITypeSymbol? typeSymbol) =>
				typeSymbol != null &&
				(typeSymbol.Equals(_pxContext.PXConnectionScope, SymbolEqualityComparer.Default) ||
				 typeSymbol.OriginalDefinition?.Equals(_pxContext.PXConnectionScope, SymbolEqualityComparer.Default) == true);
		}
	}
}
