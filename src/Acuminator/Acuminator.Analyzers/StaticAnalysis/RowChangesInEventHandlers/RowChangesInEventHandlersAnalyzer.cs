using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Acuminator.Analyzers.StaticAnalysis.EventHandlers;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using PX.SM;

namespace Acuminator.Analyzers.StaticAnalysis.RowChangesInEventHandlers
{
	public partial class RowChangesInEventHandlersAnalyzer : IEventHandlerAnalyzer
	{
		private enum RowChangesAnalysisMode
		{
			ChangesForbiddenForRowFromEventArgs,
			ChangesAllowedOnlyForRowFromEventArgs,
		}

		private static readonly IReadOnlyDictionary<EventType, RowChangesAnalysisMode> AnalyzedEventTypes = new Dictionary<EventType, RowChangesAnalysisMode>()
		{
			// Changes to e.Row are not allowed
			{ EventType.FieldDefaulting, RowChangesAnalysisMode.ChangesForbiddenForRowFromEventArgs },
			{ EventType.FieldVerifying, RowChangesAnalysisMode.ChangesForbiddenForRowFromEventArgs  },
			{ EventType.RowSelected, RowChangesAnalysisMode.ChangesForbiddenForRowFromEventArgs  },
			// Changes are allowed for e.Row only
			{ EventType.RowInserting, RowChangesAnalysisMode.ChangesAllowedOnlyForRowFromEventArgs  },
			{ EventType.RowSelecting, RowChangesAnalysisMode.ChangesAllowedOnlyForRowFromEventArgs },
		};

		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1047_RowChangesInEventHandlersForbiddenForArgs,
			Descriptors.PX1048_RowChangesInEventHandlersAllowedForArgsOnly);

		public void Analyze(SymbolAnalysisContext context, PXContext pxContext, EventType eventType)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (AnalyzedEventTypes.TryGetValue(eventType, out RowChangesAnalysisMode analysisMode))
			{
				var methodSymbol = (IMethodSymbol) context.Symbol;
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;

				if (methodSyntax != null)
				{
					var semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree, true);
					
					// Find all variables that are declared and assigned with e.Row inside the analyzed method
					var variablesWalker = new VariablesWalker(methodSyntax, semanticModel, pxContext,
						context.CancellationToken);
					methodSyntax.Accept(variablesWalker);

					// Perform analysis
					var diagnosticWalker = new DiagnosticWalker(context, semanticModel, pxContext, variablesWalker.Result,
						analysisMode, eventType);
					methodSyntax.Accept(diagnosticWalker);
				}
			}
		}

	}
}
