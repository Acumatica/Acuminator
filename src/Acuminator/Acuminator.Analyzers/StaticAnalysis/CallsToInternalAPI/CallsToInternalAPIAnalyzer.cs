
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.CallsToInternalAPI
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CallsToInternalAPIAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1076_CallToPXInternalUseOnlyAPI_OnlyISV);

		protected override bool ShouldAnalyze(PXContext pxContext) =>
			pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled &&
			pxContext.AttributeTypes.PXInternalUseOnlyAttribute != null && 
			base.ShouldAnalyze(pxContext);

		public CallsToInternalAPIAnalyzer() : this(null)
		{ }

		public CallsToInternalAPIAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeCompilationUnit(context, pxContext), SyntaxKind.CompilationUnit);
		}

		private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (syntaxContext.Node is not CompilationUnitSyntax compilationUnitSyntax)
				return;
			
			var commentsWalker = new InternalApiCallsWalker(syntaxContext, pxContext, syntaxContext.SemanticModel);
			compilationUnitSyntax.Accept(commentsWalker);
		}
	}
}
