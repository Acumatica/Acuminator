﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	public class DatabaseQueriesInRowSelectingAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1042_DatabaseQueriesInRowSelecting);

		public override bool ShouldAnalyze(PXContext pxContext, EventType eventType) =>
			eventType == EventType.RowSelecting && !pxContext.IsAcumatica2023R1_OrGreater;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = context.Symbol as IMethodSymbol;
			var methodSyntax = methodSymbol?.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			methodSyntax?.Accept(new DiagnosticWalker(context, pxContext));
		}


		private class DiagnosticWalker : Walker
		{
			private class PXConnectionScopeVisitor : CSharpSyntaxVisitor<bool>
			{
				private readonly DiagnosticWalker _parent;
				private readonly PXContext _pxContext;

				public PXConnectionScopeVisitor(DiagnosticWalker parent, PXContext pxContext)
				{
					_parent = parent.CheckIfNull();
					_pxContext = pxContext.CheckIfNull();
				}

				public override bool VisitUsingStatement(UsingStatementSyntax node)
				{
					return (node.Declaration?.Accept(this) ?? false) || (node.Expression?.Accept(this) ?? false);
				}

				public override bool VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
				{
					var semanticModel = _parent.GetSemanticModel(node.SyntaxTree);
					if (semanticModel == null)
						return false;

					var symbolInfo = semanticModel.GetSymbolInfo(node.Type, _parent.CancellationToken);
					return symbolInfo.Symbol?.OriginalDefinition != null
					       && symbolInfo.Symbol.OriginalDefinition.Equals(_pxContext.PXConnectionScope);
				}
			}

			private readonly PXConnectionScopeVisitor _connectionScopeVisitor;
			private bool _insideConnectionScope;

			public DiagnosticWalker(SymbolAnalysisContext context, PXContext pxContext)
				: base(context, pxContext, Descriptors.PX1042_DatabaseQueriesInRowSelecting)
			{
				_connectionScopeVisitor = new PXConnectionScopeVisitor(this, pxContext);
			}

			public override void VisitUsingStatement(UsingStatementSyntax node)
			{
				ThrowIfCancellationRequested();

				if (_insideConnectionScope)
				{
					base.VisitUsingStatement(node);
				}
				else
				{
					_insideConnectionScope = node.Accept(_connectionScopeVisitor);
					base.VisitUsingStatement(node);
					_insideConnectionScope = false;
				}
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				if (!_insideConnectionScope)
					base.VisitInvocationExpression(node);
			}
		}
	}
}
