using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ConstructorInGraphExtensionAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			if (context.CancellationToken.IsCancellationRequested) return;

			var methodSymbol = (IMethodSymbol) context.Symbol;
			var parentSymbol = methodSymbol.ContainingType;
			if (parentSymbol != null && parentSymbol.InheritsFrom(pxContext.PXGraphExtensionType)
				&& methodSymbol.IsInstanceConstructor())
			{
				context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1040_ConstructorInGraphExtension, 
					methodSymbol.Locations.First()));
			}
		}
	}
}
