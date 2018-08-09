using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ConnectionScopeInRowSelectingAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1042_ConnectionScopeInRowSelecting);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			
			if (methodSymbol != null && IsRowSelectingMethod(methodSymbol, pxContext))
			{
				
			}
		}

		private bool IsRowSelectingMethod(IMethodSymbol symbol, PXContext pxContext)
		{
			if (symbol.ReturnsVoid && symbol.TypeParameters.IsEmpty && !symbol.Parameters.IsEmpty)
			{
				// Loosely check method signature because sometimes business logic from event handler calls is extracted to a separate method

				// New generic event syntax
				if (symbol.Parameters[0].Type.OriginalDefinition.Equals(pxContext.Events.RowSelecting))
					return true;

				// Old syntax
				if (symbol.Parameters.Length >= 2
				    && symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCacheType)
				    && symbol.Parameters[1].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.Events.PXRowSelectingEventArgs))
					return true;
			}

			
			return false;
		}
	}
}
