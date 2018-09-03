using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.ConnectionScopeInRowSelecting;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EventHandlerAnalyzer : PXDiagnosticAnalyzer
	{
		private readonly ImmutableArray<IEventHandlerAnalyzer> _innerAnalyzers;

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

		public EventHandlerAnalyzer()
			: this(new ConnectionScopeInRowSelectingAnalyzer())
		{
		}

		/// <summary>
		/// Constructor for the unit tests.
		/// </summary>
		public EventHandlerAnalyzer(params IEventHandlerAnalyzer[] innerAnalyzers)
		{
			_innerAnalyzers = ImmutableArray.CreateRange(innerAnalyzers);
			SupportedDiagnostics = ImmutableArray.CreateRange(innerAnalyzers.SelectMany(a => a.SupportedDiagnostics));
		}

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();
			
			if (context.Symbol is IMethodSymbol methodSymbol)
			{
				EventType eventType = methodSymbol.GetEventHandlerType(pxContext);

				if (eventType != EventType.None)
				{
					foreach (var innerAnalyzer in _innerAnalyzers)
					{
						innerAnalyzer.Analyze(context, pxContext, eventType);
					}
				}
			}
		}
	}
}
