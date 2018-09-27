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
					var semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
					var walker = new Walker(context, semanticModel, pxContext, eventType);

					methodSyntax?.Accept(walker);
				}
			}
		}


		private class Walker : CSharpSyntaxWalker
		{
			private SymbolAnalysisContext _context;
			private readonly SemanticModel _semanticModel;
			private readonly PXContext _pxContext;
			private readonly EventType _eventType;

			public Walker(SymbolAnalysisContext context, SemanticModel semanticModel, PXContext pxContext, EventType eventType)
			{
				semanticModel.ThrowOnNull(nameof (semanticModel));
				pxContext.ThrowOnNull(nameof (pxContext));

				_context   = context;
				_semanticModel = semanticModel;
				_pxContext = pxContext;
				_eventType = eventType;
			}

			public override void VisitThrowStatement(ThrowStatementSyntax node)
			{
				if (_eventType == EventType.RowPersisted)
				{
					_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1073_ThrowingExceptionsInRowPersisted,
						node.GetLocation()));
				}

				// new PXSetupNotEnteredException(...)
				if (_eventType != EventType.RowSelected 
				    && node.Expression is ObjectCreationExpressionSyntax objCreationSyntax 
					&& objCreationSyntax.Type != null 
					&& _semanticModel.GetSymbolInfo(objCreationSyntax.Type).Symbol is INamedTypeSymbol exceptionType 
					&& exceptionType.InheritsFromOrEquals(_pxContext.Exceptions.PXSetupNotEnteredException))
				{
					_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1074_ThrowingSetupNotEnteredExceptionInEventHandlers,
						node.GetLocation(), _eventType));
				}
			}
		}
	}
}
