using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.ConstructorInGraphExtension
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ConstructorInGraphExtensionAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1040_ConstructorInGraphExtension);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeNamedType(c, pxContext), SymbolKind.NamedType);
		}

		private void AnalyzeNamedType(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var typeSymbol = (INamedTypeSymbol) context.Symbol;
			
			if (typeSymbol != null && typeSymbol.InheritsFrom(pxContext.PXGraphExtensionType)
				&& !typeSymbol.IsGraphExtensionBaseType())
			{
				foreach (var constructor in typeSymbol.InstanceConstructors
					.Where(c => !c.IsImplicitlyDeclared))
				{
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1040_ConstructorInGraphExtension,
						constructor.Locations.First()));
				}
			}
		}
	}
}
