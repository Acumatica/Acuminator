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

			protected override bool IsThrowingOfThisExceptionInRowPersistedAllowed(ThrowStatementSyntax throwStatement) =>
				//For processing graphs throwing exceptions in row persisted event is allowed because we don't care about PXCache state in this case
				_graphOrGraphExtension.IsProcessing        
					? true
					: base.IsThrowingOfThisExceptionInRowPersistedAllowed(throwStatement);
		}

		private class ThrowInEventsWalker : WalkerBase
		{
			private readonly EventType _eventType;

			public ThrowInEventsWalker(SymbolAnalysisContext context, PXContext pxContext, EventType eventType) : 
									 base(context, pxContext)
			{
				_eventType = eventType;
			}

			public override void VisitThrowStatement(ThrowStatementSyntax throwStatement)
			{
				ThrowIfCancellationRequested();

				var isReported = false;

				if (_eventType == EventType.RowPersisted && !IsThrowingOfThisExceptionInRowPersistedAllowed(throwStatement))
				{
					ReportDiagnostic(
						_context.ReportDiagnostic,
						_pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled
							? Descriptors.PX1073_ThrowingExceptionsInRowPersisted
							: Descriptors.PX1073_ThrowingExceptionsInRowPersisted_NonISV,
						throwStatement);
					isReported = true;
				}

				if (_eventType != EventType.RowSelected && IsPXSetupNotEnteredException(throwStatement))
				{
					ReportDiagnostic(_context.ReportDiagnostic,
						Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers,
						throwStatement, _eventType);
					isReported = true;
				}

				if (!isReported)
				{
					base.VisitThrowStatement(throwStatement);
				}
			}

			protected virtual bool IsThrowingOfThisExceptionInRowPersistedAllowed(ThrowStatementSyntax throwStatement)
			{
				SemanticModel? semanticModel = GetSemanticModel(throwStatement.SyntaxTree);
				ITypeSymbol? exceptiontype = semanticModel?.GetTypeInfo(throwStatement.Expression).Type;

				bool isAllowed = exceptiontype != null &&             // It's better to be conservative here and report thrown exception if we can't verify its type
								(exceptiontype.InheritsFromOrEquals(_pxContext.Exceptions.PXRowPersistedException) ||
								 exceptiontype.InheritsFromOrEquals(_pxContext.Exceptions.PXLockViolationException));
				return isAllowed;
			}
		}
	}
}
