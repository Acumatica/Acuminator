using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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

namespace Acuminator.Analyzers.StaticAnalysis.ThrowingExceptions
{
	public class ThrowingExceptionsInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1073_ThrowingExceptionsInRowPersisted,
			Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
			EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (eventType != EventType.None)
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;

				if (methodSyntax != null)
				{
					var walker = new Walker(context, pxContext, eventType);

					methodSyntax.Accept(walker);
				}
			}
		}

		private class Walker : WalkerBase
		{
			private readonly EventType _eventType;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
                : base(context, pxContext)
			{
				_eventType = eventType;
			}

			public override void VisitThrowStatement(ThrowStatementSyntax node)
			{
				ThrowIfCancellationRequested();

                var isReported = false;

                if (_eventType == EventType.RowPersisted)
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1073_ThrowingExceptionsInRowPersisted, node);
                    isReported = true;
                }

                if (_eventType != EventType.RowSelected && IsPXSetupNotEnteredException(node))
                {
                    ReportDiagnostic(_context.ReportDiagnostic, Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers, node, _eventType);
                    isReported = true;
                }

                if (!isReported)
                {
                    base.VisitThrowStatement(node);
                }
			}
		}
	}
}
