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
using CommonServiceLocator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DatabaseQueries
{
	public class DatabaseQueriesInRowSelectedAnalyzer : IEventHandlerAnalyzer
	{
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1049_DatabaseQueriesInRowSelected);
		
		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, CodeAnalysisSettings codeAnalysisSettings, 
			EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			if (codeAnalysisSettings.IsvSpecificAnalyzersEnabled && eventType == EventType.RowSelected)
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as CSharpSyntaxNode;
				methodSyntax?.Accept(new Walker(context, pxContext, Descriptors.PX1049_DatabaseQueriesInRowSelected));
			}
		}
	}
}
