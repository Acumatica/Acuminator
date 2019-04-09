using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Threading.Tasks;

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
			compilationStartContext.RegisterSymbolAction(async symbolContext =>
														 await AnalyzeDacFieldTypeAsync(symbolContext, pxContext).ConfigureAwait(false), SymbolKind.NamedType);
		}

		private static async Task AnalyzeDacFieldTypeAsync(SymbolAnalysisContext symbolContext, PXContext pxContext)
		{
			INamedTypeSymbol dacFieldType = symbolContext.Symbol as INamedTypeSymbol;

			if (!IsDacFieldType(dacFieldType, pxContext) || dacFieldType.IsAbstract || symbolContext.CancellationToken.IsCancellationRequested)
				return;

			var declarations = dacFieldType.DeclaringSyntaxReferences;

			if (declarations.Length != 1)
				return;

			var dacFieldDeclaration = await declarations[0].GetSyntaxAsync(symbolContext.CancellationToken)
														   .ConfigureAwait(false) as ClassDeclarationSyntax;
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