using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;


namespace Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DacNonAbstractFieldTypeAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1024_DacNonAbstractFieldType
			);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(symbolContext => AnalyzeDacFieldType(symbolContext, pxContext), SymbolKind.NamedType);
		}

		private static void AnalyzeDacFieldType(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			INamedTypeSymbol dacFieldType = symbolContext.Symbol as INamedTypeSymbol;

			if (!IsDacFieldType(dacFieldType, pxContext) || dacFieldType.IsAbstract || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var declarations = dacFieldType.DeclaringSyntaxReferences;

			if (declarations.Length != 1)
				return;

			var dacFieldDeclaration = declarations[0].GetSyntax(symbolContext.CancellationToken) as ClassDeclarationSyntax;
			Location dacFieldLocation = dacFieldDeclaration?.Identifier.GetLocation();

			if (dacFieldLocation == null || symbolContext.CancellationToken.IsCancellationRequested)
				return;
	
			symbolContext.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(Descriptors.PX1024_DacNonAbstractFieldType, dacFieldLocation),
															   pxContext.CodeAnalysisSettings);		
		}

		private static bool IsDacFieldType(ITypeSymbol dacFieldType, PXContext pxContext)
		{
			if (dacFieldType == null || dacFieldType.TypeKind != TypeKind.Class || !dacFieldType.IsDacField() && dacFieldType.ContainingType != null)
				return false;

			return dacFieldType.ContainingType.IsDAC() || dacFieldType.ContainingType.IsDacExtension();
		}
	}
}