#nullable enable

using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
	public partial class ThrowingExceptionsInEventHandlersAnalyzer : IEventHandlerAnalyzer, IPXGraphAnalyzer
	{
		private class ThrowInGraphEventsWalker : ThrowInEventsWalker
		{
			private readonly PXGraphEventSemanticModel _graphOrGraphExtension;

			public ThrowInGraphEventsWalker(SymbolAnalysisContext context, PXContext pxContext, EventType eventType,
											PXGraphEventSemanticModel graphOrGraphExtension) :
									   base(context, pxContext, eventType)
			{
				_graphOrGraphExtension = graphOrGraphExtension;
			}

			protected override bool IsThrowingOfThisExceptionInRowPersistedAllowed(ExpressionSyntax? expressionAfterThrowkeyword) =>
				//For processing graphs throwing exceptions in row persisted event is allowed because we don't care about PXCache state in this case
				_graphOrGraphExtension.IsProcessing        
					? true
					: base.IsThrowingOfThisExceptionInRowPersistedAllowed(expressionAfterThrowkeyword);
		}

		private class ThrowInEventsWalker : WalkerBase
		{
			private readonly EventType _eventType;

			public ThrowInEventsWalker(SymbolAnalysisContext context, PXContext pxContext, EventType eventType) : 
									 base(context, pxContext)
			{
				_eventType = eventType;
			}

			public override void VisitThrowExpression(ThrowExpressionSyntax throwExpression)
			{
				bool isReported = CheckThrowExpression(throwExpression.Expression, throwExpression);

				if (!isReported)
				{
					base.VisitThrowExpression(throwExpression);
				}
			}

			public override void VisitThrowStatement(ThrowStatementSyntax throwStatement)
			{
				bool isReported = CheckThrowExpression(throwStatement.Expression, throwStatement);

				if (!isReported)
				{
					base.VisitThrowStatement(throwStatement);
				}
			}

			private bool CheckThrowExpression(ExpressionSyntax expressionAfterThrowkeyword, SyntaxNode throwNodeToReport)
			{
				ThrowIfCancellationRequested();

				bool isReported = false;

				if (_eventType == EventType.RowPersisted && !IsThrowingOfThisExceptionInRowPersistedAllowed(expressionAfterThrowkeyword))
				{
					ReportDiagnostic(_context.ReportDiagnostic,
									 _pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
										? Descriptors.PX1073_ThrowingExceptionsInRowPersisted
										: Descriptors.PX1073_ThrowingExceptionsInRowPersisted_NonISV,
									 throwNodeToReport);
					isReported = true;
				}

				if (_eventType != EventType.RowSelected && IsPXSetupNotEnteredException(expressionAfterThrowkeyword))
				{
					ReportDiagnostic(_context.ReportDiagnostic,
									 Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers,
									 throwNodeToReport, _eventType);
					isReported = true;
				}

				return isReported;
			}

			protected virtual bool IsThrowingOfThisExceptionInRowPersistedAllowed(ExpressionSyntax? expressionAfterThrowkeyword)
			{
				if (expressionAfterThrowkeyword?.SyntaxTree == null)
					return false;                                     // It's better to be conservative here and report thrown exception if we can't verify its type

				SemanticModel? semanticModel = GetSemanticModel(expressionAfterThrowkeyword.SyntaxTree);
				ITypeSymbol? exceptiontype = semanticModel?.GetTypeInfo(expressionAfterThrowkeyword).Type;

				bool isAllowed = exceptiontype != null &&             // It's better to be conservative here and report thrown exception if we can't verify its type
								(exceptiontype.InheritsFromOrEquals(_pxContext.Exceptions.PXRowPersistedException) ||
								 exceptiontype.InheritsFromOrEquals(_pxContext.Exceptions.PXLockViolationException));
				return isAllowed;
			}
		}
	}
}
