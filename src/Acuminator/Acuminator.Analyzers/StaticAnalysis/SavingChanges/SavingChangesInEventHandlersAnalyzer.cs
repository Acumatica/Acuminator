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
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
	public class SavingChangesInEventHandlersAnalyzer : EventHandlerAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1043_SavingChangesInEventHandlers,
			Descriptors.PX1043_SavingChangesInRowPerstisting,
			Descriptors.PX1043_SavingChangesInRowPerstistedNonISV);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol)context.Symbol;
			var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
			methodSyntax?.Accept(new Walker(context, pxContext, eventType));
		}

		private class Walker : NestedInvocationWalker
		{
			private SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;
			private readonly EventType _eventType;
			private bool IsTransactionOpened = false;

			private readonly MemberAccessExpressionSyntax leftExpression, rightExpression;
			private readonly BinaryExpressionSyntax tranStatus;
			
			public Walker(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
				: base(context.Compilation, context.CancellationToken, pxContext.CodeAnalysisSettings)
			{
				pxContext.ThrowOnNull(nameof(pxContext));

				_context = context;
				_pxContext = pxContext;
				_eventType = eventType;

				leftExpression = SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.IdentifierName("e"),
					SyntaxFactory.IdentifierName("TranStatus"));

				rightExpression = SyntaxFactory.MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					SyntaxFactory.IdentifierName("PXTranStatus"),
					SyntaxFactory.IdentifierName("Open"));

				tranStatus = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression,
					leftExpression, 
					SyntaxFactory.Token(SyntaxKind.EqualsEqualsToken),
					rightExpression);
			}

			public override void VisitIfStatement(IfStatementSyntax node)
			{
				ThrowIfCancellationRequested();

				if (IsTransactionOpened == false)
				{
					
					if (node.Condition.IsEquivalentTo(tranStatus, true))
						IsTransactionOpened = true;
				}

				base.VisitIfStatement(node);
				IsTransactionOpened = false;
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				ThrowIfCancellationRequested();

				var methodSymbol = GetSymbol<IMethodSymbol>(node);

				if (methodSymbol == null || !AnalyzeAndReportDiagnostic(methodSymbol, node))
				{
					base.VisitInvocationExpression(node);
				}
			}

			private bool AnalyzeAndReportDiagnostic(IMethodSymbol symbol, InvocationExpressionSyntax node)
			{
				var semanticModel = GetSemanticModel(node.SyntaxTree);
				SaveOperationKind saveOperationKind = SaveOperationHelper.GetSaveOperationKind(
					symbol, node, semanticModel, _pxContext);

				PXDBOperationKind saveDatabaseKind = SaveOperationHelper.GetPXDatabaseSaveOperationKind(symbol, _pxContext);

				if (saveOperationKind != SaveOperationKind.None || 
				    saveDatabaseKind != PXDBOperationKind.None)
				{
					if (_eventType == EventType.RowPersisting)
					{
						if (saveOperationKind != SaveOperationKind.CachePersist)
						{
							ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1043_SavingChangesInRowPerstisting, node);
							return true;
						}
					}
					else if (_eventType == EventType.RowPersisted)
					{
						if (IsTransactionOpened) 
							return false;

						ReportDiagnostic(_context.ReportDiagnostic, 
							_pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
								? Descriptors.PX1043_SavingChangesInEventHandlers
								: Descriptors.PX1043_SavingChangesInRowPerstistedNonISV, node);
						return true;
					}
					else
					{
						ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1043_SavingChangesInEventHandlers, node);
						return true;
					}
				}

				return false;
			}
		}
	}
}
