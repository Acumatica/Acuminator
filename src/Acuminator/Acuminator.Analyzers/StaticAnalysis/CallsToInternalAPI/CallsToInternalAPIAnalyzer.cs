using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

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

		public CallsToInternalAPIAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSyntaxNodeAction(context => AnalyzeCompilationUnit(context, pxContext), SyntaxKind.CompilationUnit);
		}

		private void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (!(syntaxContext.Node is CompilationUnitSyntax compilationUnitSyntax))
				return;
		
			var semanticModel = pxContext.Compilation.GetSemanticModel(compilationUnitSyntax.SyntaxTree);

			if (semanticModel == null)
				return;

			var commentsWalker = new InternalApiCallsWalker(syntaxContext, pxContext, semanticModel);
			compilationUnitSyntax.Accept(commentsWalker);
		}
	}
}
